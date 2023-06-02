using DSharpPlus;
using DSharpPlus.EventArgs;
using OmniBot;
using OmniBot.Commands;
using OmniBot.Sinks;
using Serilog;
using Serilog.Core;

Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<DiscordClient>(provider =>
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

        services
            .AddSingleton<ILogEventSink, DiscordSink>()
            .AddSingleton<Commands>()
            .AddHostedService<OmniBotService>();
    })
    .ConfigureHostConfiguration(hostConfig =>
        hostConfig.Add(new SecretsConfigurationSource("OmniBot_Secrets")))
    .UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console();
    })
    .RunConsoleAsync();

await Log.CloseAndFlushAsync();

internal sealed class OmniBotService : IHostedService
{
    private readonly DiscordClient _discord;

    public OmniBotService(DiscordClient discord, Commands commands, IConfiguration config)
    {
        _discord = discord;
        _discord.Ready += OnReady;
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _discord.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _discord.DisconnectAsync();
    }

    private Task OnReady(DiscordClient c, ReadyEventArgs a)
    {
        var botUser = _discord.CurrentUser!;
        var username = $"{botUser.Username}#{botUser.Discriminator}";
        _discord.Logger.LogInformation("Connected as {Username}", username);
        return Task.CompletedTask;
    }
}