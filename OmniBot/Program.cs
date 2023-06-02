using DSharpPlus;
using DSharpPlus.EventArgs;
using OmniBot;
using OmniBot.Commands;
using Serilog;

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
        services.AddSingleton<Commands>();
        services.AddHostedService<OmniBotService>();
    })
    .ConfigureHostConfiguration(hostConfig =>
        hostConfig.Add(new SecretsConfigurationSource("OmniBot_Secrets")))
    .UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
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