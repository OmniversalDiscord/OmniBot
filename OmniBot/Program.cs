using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using OmniBot;
using OmniBot.Commands;
using OmniBot.Commands.Modules;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console()
    .CreateLogger();

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(logging => logging.ClearProviders().AddSerilog());
        services.AddSingleton<Commands>();
        services.AddHostedService<OmniBotService>();
    })
    .ConfigureHostConfiguration(hostConfig =>
        hostConfig.Add(new SecretsConfigurationSource("OmniBot_Secrets")))
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
        _logger.LogInformation("Connected as {Username}", username);
        return Task.CompletedTask;
    }

    public OmniBotService(ILogger<OmniBotService> logger, Commands commands, IConfiguration config)
    {
        _logger = logger;
        _discord = new DiscordClient(new DiscordConfiguration
        {
            Token = config["DiscordToken"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LogUnknownEvents = false,
            LoggerFactory = new LoggerFactory().AddSerilog()
        });

        _discord.Ready += OnReady;
        commands.Register(_discord);
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
