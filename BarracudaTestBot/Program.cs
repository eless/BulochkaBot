using BarracudaTestBot.Checkers;
using BarracudaTestBot.Services;
using Telegram.Bot;

internal class Program
{
    public static void Main(string[] args)
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

        var token = builder.Configuration.GetValue<string>("TelegramToken");
        builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(token));
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
            _ = botService.Start(cts);
        }

        System.Diagnostics.Trace.WriteLine($"app starting at {startDate}");
        app.Run();
        System.Diagnostics.Trace.WriteLine($"app started");

        // Send cancellation request to stop bot
        cts.Cancel();

    }
}