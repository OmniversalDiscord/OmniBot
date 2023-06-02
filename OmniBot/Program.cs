using DSharpPlus;
using OmniBot;
using OmniBot.Commands;
using OmniBot.Sinks;
using Serilog;
using Serilog.Core;

Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddSecrets("OmniBot_Secrets");

builder.Services
    .AddSingleton<DiscordClient>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();

        return new DiscordClient(new DiscordConfiguration
        {
            Token = config["DiscordToken"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LogUnknownEvents = false,
            LoggerFactory = new LoggerFactory().AddSerilog()
        });
    });

builder.Services
    .AddSingleton<ILogEventSink, DiscordSink>()
    .AddSingleton<Commands>()
    .AddHostedService<OmniBotHost>();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>())
        .ReadFrom.Services(services)
        .WriteTo.Console();
});

await builder.Build().RunAsync();

await Log.CloseAndFlushAsync();