using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Options;
using OmniBot.Commands.Attributes;
using OmniBot.Models;

namespace OmniBot.Commands;

public class Commands
{
    private static bool _isConfigured;
    private readonly ILogger<Commands> _logger;

    public Commands(DiscordClient client, IServiceProvider services, IOptions<GeneralOptions> options,
        ILogger<Commands> logger)
    {
        _logger = logger;
        var guildId = options.Value.GuildId;

        if (_isConfigured)
        {
            logger.LogWarning("Attempted to register commands multiple times");
            return;
        }

        var slash = client.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = services
        });

        client.UseInteractivity(new InteractivityConfiguration
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromSeconds(60)
        });

        var registeredModules = RegisterCommands(slash, guildId);

        client.Ready += (_, _) =>
        {
            _logger.LogInformation("Registered command modules: {Modules}",
                string.Join(", ", registeredModules));
            return Task.CompletedTask;
        };

        slash.SlashCommandErrored += CommandErrored;

        _isConfigured = true;
    }

    private static IEnumerable<string> RegisterCommands(SlashCommandsExtension slash, ulong guildId)
    {
        // Get all root commands modules in the assembly
        var rootCommandsModules = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.IsAssignableTo(typeof(ApplicationCommandModule)) && !type.IsNested)
            .ToList();

        // Register each module
        foreach (var module in rootCommandsModules) slash.RegisterCommands(module, guildId);

        return rootCommandsModules.Select(module => module.Name);
    }

    private static Task SendErrorMessage(BaseContext ctx, string message, bool ephemeral)
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
                        SendErrorMessage(ctx,
                            $"You must be in {ctx.Guild.GetChannel(requireUserInChannel.ChannelId).Name} to use this command",
                            false),
                    _ => Task.CompletedTask
                };
            case RoleSectionNotFoundException sectionNotFound:
                return SendErrorMessage(ctx,
                    $"A command tried to access the {sectionNotFound.SectionName} role section, but no section was found",
                    false);
            default:
                _logger.LogError(exception, "Error executing command {Command}", ctx.CommandName);
                return SendErrorMessage(ctx, "An unknown error occurred", false);
        }
    }
}