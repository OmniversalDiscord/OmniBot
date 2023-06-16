using DSharpPlus;
using OmniBot;
using OmniBot.Commands;
using OmniBot.Models;
using OmniBot.Services;
using OmniBot.Sinks;
using Serilog;
using Serilog.Core;
using Serilog.Settings.Configuration;

Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddYamlFile("appsettings.yml");
builder.Configuration.AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yml", true);
builder.Configuration.AddSecrets("SECRETS");

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
    .Configure<GeneralOptions>(builder.Configuration.GetSection(GeneralOptions.Section))
    .Configure<RolesOptions>(builder.Configuration.GetSection(RolesOptions.Section))
    .Configure<DiscordLoggingOptions>(builder.Configuration.GetSection(DiscordLoggingOptions.Section));

// Only add the Discord sink in production
if (builder.Environment.IsProduction())
    builder.Services.AddSingleton<ILogEventSink, DiscordSink>();

builder.Services
    .AddSerilog((services, configuration) =>
    {
        var options = new ConfigurationReaderOptions { SectionName = "Logging" };

        configuration
            .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>(), options)
            .ReadFrom.Services(services)
            .WriteTo.Console();
    });

builder.Services
    .AddSingleton<RolesCollection>()
    .AddTransient<ColorService>();

builder.Services
    .AddSingleton<Commands>()
    .AddHostedService<OmniBotHost>();

await builder.Build().RunAsync();

await Log.CloseAndFlushAsync();