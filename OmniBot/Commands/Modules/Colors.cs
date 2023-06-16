using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using OmniBot.Services;

namespace OmniBot.Commands.Modules;

[PublicAPI]
public class Colors : ApplicationCommandModule
{
    private readonly string _componentId;
    private ColorService _colorService;
    private ILogger<Colors> _logger;

    public Colors(ColorService colorService, ILogger<Colors> logger)
    {
        _colorService = colorService;
        _logger = logger;
        _componentId = Guid.NewGuid().ToString();
    }

    private async Task<DiscordMessage> SendColorPickerMessage(InteractionContext ctx)
    {
        var allColors = _colorService.GetAllColors();
        var roleMentionList = string.Join("\n", allColors.Select(role => role.Mention).ToList());
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Available colors")
            .WithDescription($"{roleMentionList}")
            .WithFooter("Use the select menu below to choose your color in chat")
            .WithColor(DiscordColor.Azure);

        var currentColor = _colorService.GetColorForMember(ctx.Member);
        var selectOptions = allColors.Select(role =>
            new DiscordSelectComponentOption(role.Name, role.Id.ToString(), isDefault: currentColor?.Id == role.Id));

        var selectMenu = new DiscordSelectComponent($"color-{_componentId}", "Color", selectOptions);

        var buttons = new List<DiscordButtonComponent>
            { new(ButtonStyle.Secondary, $"cancel-{_componentId}", "Cancel") };

        if (currentColor != null)
            buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, $"clear-{_componentId}", "Clear"));

        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AddComponents(selectMenu)
            .AddComponents(buttons));

        return await ctx.GetOriginalResponseAsync();
    }

    [SlashCommand("color", "Set or clear your color role")]
    public async Task SetColor(InteractionContext ctx)
    {
        var message = await SendColorPickerMessage(ctx);

        // The second await is needed because Task.WhenAny returns a Task<Task<>>, thanks Microsoft
        // It's instant though - I could just read TResult but then I would have to wrap the await in brackets and ugh
        var response =
            await await Task.WhenAny(
                message.WaitForSelectAsync(ctx.User, $"color-{_componentId}"),
                message.WaitForButtonAsync(ctx.User));

        var result = response.Result;

        if (response.TimedOut || result.Id == $"cancel-{_componentId}")
        {
            await message.DeleteAsync();
        }
        else if (result.Id == $"clear-{_componentId}")
        {
            await _colorService.ClearColorForMember(ctx.Member);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You no longer have a color"));
        }
        else
        {
            // Has to be a select menu
            var newColorId = ulong.Parse(result.Values.First());
            var newColor = ctx.Guild.GetRole(newColorId);

            if (newColor == null)
            {
                _logger.LogError("Attempted to set color role to {ColorId} but the role was not found", newColorId);
                throw new CommandException("An error occurred while setting your color role");
            }

            await _colorService.SetColorForMember(ctx.Member, newColor);
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Your color is now **{newColor.Name}**"));
        }
    }
}