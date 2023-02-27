using BarracudaTestBot.Checkers;
using BarracudaTestBot.Services;
using Telegram.Bot;
using Microsoft.ApplicationInsights;
using Telegram.Bot.Types;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;

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
        builder.Services.AddHostedService<RussianLossesDailyReport>();
        builder.Services.AddSingleton<RussianLossesSender>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddApplicationInsightsTelemetry();
        var configuration = TelemetryConfiguration.CreateDefault();
        var telemetryClient = new TelemetryClient(configuration);
        builder.Services.AddSingleton(telemetryClient);
       
        try
        {
            // Code that can potentially throw an exception
            int x = 0;
            int y = 1 / x;
        }
        catch (Exception ex)
        {
            //TODO: remove after the test
            telemetryClient.TrackTrace($" Test exeption catched {ex}");
            telemetryClient.TrackException(ex);
        }
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

        telemetryClient.TrackTrace($"app starting at {startDate}");
        app.Run();
        // Send cancellation request to stop bot
        telemetryClient.TrackTrace($"app stoped at {DateTime.UtcNow}");
        cts.Cancel();
    }
}