using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using OmniBot.Commands.Modules;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

await Host.CreateDefaultBuilder(args)
    .UseConsoleLifetime()
    .ConfigureServices(services =>
    {
        services.AddLogging(logging => logging.ClearProviders().AddSerilog());
        services.AddHostedService<OmniBotService>();
    })
    .RunConsoleAsync();

await Log.CloseAndFlushAsync();


internal sealed class OmniBotService : IHostedService
{
    private readonly DiscordClient _discord;
    private readonly ILogger<OmniBotService> _logger;
    
    private Task OnReady(DiscordClient client, ReadyEventArgs args)
    {
        var botUser = client.CurrentUser!;
        var username = $"{botUser.Username}#{botUser.Discriminator}";
        _logger.LogInformation("Connected as {username}", username);
        return Task.CompletedTask;
    }

    private static void RegisterCommands(ulong guildId, SlashCommandsExtension slash)
    {
        slash.RegisterCommands<ChannelSize>(guildId);
    }
    
    public OmniBotService(ILogger<OmniBotService> logger, IServiceProvider services, IConfiguration config)
    {
        _logger = logger;
        _discord = new DiscordClient(new DiscordConfiguration
        {
            Token = config["DiscordToken"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog()
        });

        _discord.Ready += OnReady;

        var slash = _discord.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = services
        });

        RegisterCommands(ulong.Parse(config["GuildId"]!), slash);
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _discord.ConnectAsync();
    }
    
    public async Task StopAsync(CancellationToken token)
    {
        await _discord.DisconnectAsync();
    }
}