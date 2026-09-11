using System.ClientModel;
using Microsoft.ApplicationInsights;
using OpenAI;
using OpenAI.Chat;

namespace BarracudaTestBot.Services;

/// <summary>
/// Chat completion over a chain of OpenAI-compatible providers (Groq, Gemini, OpenAI).
/// Providers are tried in order; one that answers 429 (quota / rate limit) is paused for a while
/// and the next one is used. Returns null when nobody can answer, so the bot simply stays quiet.
/// </summary>
public class AiChatService
{
    private static readonly TimeSpan PauseAfterQuotaError = TimeSpan.FromHours(1);

    private sealed class Provider(string name, ChatClient client)
    {
        public string Name { get; } = name;
        public ChatClient Client { get; } = client;
        public DateTime PausedUntil { get; set; } = DateTime.MinValue;
        public bool IsPaused => DateTime.UtcNow < PausedUntil;
    }

    private readonly List<Provider> _providers = [];
    private readonly TelemetryClient _telemetry;

    public bool HasProviders => _providers.Count > 0;

    public AiChatService(IConfiguration configuration, TelemetryClient telemetry)
    {
        _telemetry = telemetry;

        // Order matters: free tiers first, paid OpenAI as the last resort.
        AddProvider("Groq", configuration, "GROQ_API_KEY", "GROQ_MODEL", "openai/gpt-oss-120b",
            new Uri("https://api.groq.com/openai/v1"));
        AddProvider("Gemini", configuration, "GEMINI_API_KEY", "GEMINI_MODEL", "gemini-3.5-flash-lite",
            new Uri("https://generativelanguage.googleapis.com/v1beta/openai/"));
        AddProvider("OpenAI", configuration, "OPENAI_API_KEY", "OPENAI_MODEL", "gpt-4.1", endpoint: null);

        _telemetry.TrackTrace(HasProviders
            ? $"AI providers configured: {string.Join(", ", _providers.Select(p => p.Name))}"
            : "No AI providers configured, AI answers are disabled");
    }

    private void AddProvider(string name, IConfiguration configuration, string keySetting, string modelSetting,
        string defaultModel, Uri? endpoint)
    {
        var apiKey = configuration.GetValue<string>(keySetting);
        if (string.IsNullOrWhiteSpace(apiKey)) return;

        var model = configuration.GetValue<string>(modelSetting);
        if (string.IsNullOrWhiteSpace(model)) model = defaultModel;

        var options = new OpenAIClientOptions();
        if (endpoint != null) options.Endpoint = endpoint;

        _providers.Add(new Provider($"{name}/{model}", new ChatClient(model, new ApiKeyCredential(apiKey), options)));
    }

    public async Task<string?> CompleteAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken)
    {
        ChatMessage[] messages = [new SystemChatMessage(systemPrompt), new UserChatMessage(userMessage)];

        foreach (var provider in _providers.Where(p => !p.IsPaused))
        {
            try
            {
                var completion = await provider.Client.CompleteChatAsync(messages, cancellationToken: cancellationToken);
                var text = completion.Value.Content.FirstOrDefault()?.Text;
                if (!string.IsNullOrWhiteSpace(text)) return text;

                _telemetry.TrackTrace($"AI provider {provider.Name} returned an empty answer, trying next");
            }
            catch (ClientResultException ex) when (ex.Status == 429)
            {
                provider.PausedUntil = DateTime.UtcNow + PauseAfterQuotaError;
                _telemetry.TrackTrace($"AI provider {provider.Name} answered HTTP 429, paused until {provider.PausedUntil:u}: {ex.Message}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _telemetry.TrackTrace($"AI provider {provider.Name} failed, trying next: {ex.Message}");
                _telemetry.TrackException(ex);
            }
        }

        _telemetry.TrackTrace("No AI provider could answer, AI answer skipped");
        return null;
    }
}
