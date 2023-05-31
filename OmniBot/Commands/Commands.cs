using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using OmniBot.Commands.Attributes;
using OmniBot.Commands.Modules;
using Serilog;

namespace OmniBot.Commands;

public class Commands
{
    private IServiceProvider _services;
    private ILogger<Commands> _logger;
    private ulong _guildId;
    private bool _isConfigured;

    public Commands(IServiceProvider services, IConfiguration configuration, ILogger<Commands> logger)
    {
        _services = services;
        _logger = logger;
        _guildId = ulong.Parse(configuration["GuildId"]!);
    }

    private void RegisterCommands(SlashCommandsExtension slash)
    {
        slash.RegisterCommands<ChannelSize>(_guildId);
    }

    public void Register(DiscordClient client)
    {
        if (_isConfigured)
        {
            _logger.LogWarning("Attempted to configure commands when already configured");
            return;
        }

        var slash = client.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = _services
        });

        RegisterCommands(slash);
        slash.SlashCommandErrored += CommandErrored;

        _isConfigured = true;
    }

    private static Task SendErrorMessage(InteractionContext ctx, string message, bool ephemeral)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Error")
            .WithDescription(message)
            .WithColor(DiscordColor.Red)
            .WithTimestamp(DateTimeOffset.Now);

        return ctx.CreateResponseAsync(embed, ephemeral);
    }
    
    private Task CommandErrored(SlashCommandsExtension slash, SlashCommandErrorEventArgs args)
    {
        var ctx = args.Context;
        var exception = args.Exception;
        switch (exception)
        {
            case CommandException:
                return SendErrorMessage(ctx, exception.Message, false);
            case EphemeralCommandException:
                return SendErrorMessage(ctx, exception.Message, true);
            case SlashExecutionChecksFailedException checksFailed:
                // Get first fail
                return checksFailed.FailedChecks[0] switch
                {
                    RequireUserInChannel requireUserInChannel =>
                        SendErrorMessage(ctx, $"You must be in {ctx.Guild.GetChannel(requireUserInChannel.ChannelId).Name} to use this command", false),
                    _ => Task.CompletedTask
                };
            default:
                _logger.LogError(exception, "Error executing command {command}", ctx.CommandName);
                return SendErrorMessage(ctx, "An unknown error occurred", false);
        }
    }
}