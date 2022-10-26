using BarracudaTestBot.Checkers;
using BarracudaTestBot.Services;

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

        builder.Services.AddHostedService<PingService>();
        builder.Services.AddSingleton<BotService>();
        builder.Services.AddSingleton<WordChecker>();
        builder.Services.AddSingleton<StickerChecker>();
        builder.Services.AddSingleton<RussianLossesService>();
        builder.Services.AddSingleton<PutinGenerator>();

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
        botService.Start(cts);

        System.Diagnostics.Trace.WriteLine($"app starting at {startDate}");
        app.Run();
        System.Diagnostics.Trace.WriteLine($"app started");

        // Send cancellation request to stop bot
        cts.Cancel();

    }
}