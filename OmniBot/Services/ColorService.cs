using DSharpPlus;
using DSharpPlus.Entities;
using OmniBot.Models;

namespace OmniBot.Services;

public class ColorService
{
    private readonly DiscordClient _client;
    private readonly ISet<DiscordRole> _colors;
    private readonly IList<DiscordRole> _sortedColors;

    public ColorService(DiscordClient client, RolesCollection rolesCollection)
    {
        _client = client;
        _sortedColors = rolesCollection.GetRoleSection("colors");
        _colors = _sortedColors.ToHashSet();
    }

    public DiscordRole? GetColorForMember(DiscordMember member)
    {
        // This search is cheap enough to run multiple times as most users only have a few roles
        // If roles were actually sorted it would be even cheaper!
        return member.Roles.FirstOrDefault(role => _colors.Contains(role));
    }

    public IList<DiscordRole> GetAllColors()
    {
        return _sortedColors;
    }

    public async Task ClearColorForMember(DiscordMember ctxMember)
    {
        var color = GetColorForMember(ctxMember);
        if (color is null)
            return;

        await ctxMember.RevokeRoleAsync(color);
    }

    public async Task SetColorForMember(DiscordMember ctxMember, DiscordRole newColor)
    {
        if (!_colors.Contains(newColor))
            throw new ArgumentException("Role is not a color", nameof(newColor));

        await ClearColorForMember(ctxMember);
        await ctxMember.GrantRoleAsync(newColor);
    }
}