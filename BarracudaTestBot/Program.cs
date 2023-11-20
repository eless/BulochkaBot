using BarracudaTestBot.Checkers;
using BarracudaTestBot.Services;
using Telegram.Bot;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using BarracudaTestBot.Database;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static TelemetryClient Telemetry => new TelemetryClient(TelemetryConfiguration.CreateDefault());

    public static void Main(string[] args)
    {
        // Subscribe to the UnhandledException event
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<BotDbContext>(options => {
            options.UseSqlServer(builder.Configuration.GetConnectionString("BulochkaDBConnectionString"));
        });

        var token = builder.Configuration.GetValue<string>("TelegramToken");
        builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token!));
        builder.Services.AddHostedService<PingService>();
        builder.Services.AddSingleton<BotService>();
        builder.Services.AddSingleton<WordChecker>();
        builder.Services.AddSingleton<StickerChecker>();
        builder.Services.AddSingleton<RussianLossesService>();
        builder.Services.AddSingleton<PutinGenerator>();
        builder.Services.AddSingleton<AirAlarmChecker>();
        builder.Services.AddSingleton<AirAlarmAlertNotifier>();
        builder.Services.AddSingleton<AirAlarmAllClearNotifier>();
        builder.Services.AddSingleton<AirAlarmGenericNotifier>();
        builder.Services.AddHostedService<AirAlarmMonitor>();
        builder.Services.AddSingleton<AirAlarmStickerSelector>();
        builder.Services.AddHostedService<RussianLossesDailyReport>();
        builder.Services.AddSingleton<RussianLossesSender>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddSingleton(Telemetry);
        
        var app = builder.Build();

        app.UseExceptionHandler("/Error");
        app.UseDeveloperExceptionPage();
        app.UseStaticFiles();

        app.UseRouting();
        var startDate = DateTime.UtcNow;
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context => { await context.Response.WriteAsync($"{app.Environment.ApplicationName} has started at {startDate} UTC. Hallo, Sweetie!"); });
        });

        var botService = app.Services.GetService<BotService>();

        using var cts = new CancellationTokenSource();
        if (botService != null && cts != null)
        {
            botService.Start(cts);
        }

        Telemetry.TrackTrace($"app starting at {startDate}");
        app.Run();
        // Send cancellation request to stop bot
        Telemetry.TrackTrace($"app stoped at {DateTime.UtcNow}");
        cts?.Cancel();
    }

    static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs ex)
    {
        Telemetry.TrackTrace("An unhandled exception occurred: " + ex.ExceptionObject);
        Telemetry.TrackException((Exception) ex.ExceptionObject);
    }
}