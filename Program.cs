using BarracudaTestBot.Services;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile($"appsettings.json", false, true)
                .AddJsonFile($"appsettings.Local.json", true, true)
                .AddEnvironmentVariables();
        });
        builder.Services.AddControllers();

        builder.Services.AddSingleton<BotService>();

        var app = builder.Build();

        app.UseStaticFiles();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context => { await context.Response.WriteAsync($"{app.Environment.ApplicationName} has started at {DateTime.UtcNow} UTC. Hallo, Sweetie!"); });
        });

        app.Start();

        var botService = app.Services.GetService<BotService>();

        using var cts = new CancellationTokenSource();
        await botService.Start(cts);

        // Send cancellation request to stop bot
        cts.Cancel();
    }
}