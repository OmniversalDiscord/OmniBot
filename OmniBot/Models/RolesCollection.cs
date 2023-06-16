using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;

namespace OmniBot.Models;

public class RoleSectionNotFoundException : Exception
{
    public RoleSectionNotFoundException(string sectionName) : base(
        $"Role section {sectionName} not found")
    {
        SectionName = sectionName;
    }

    public string SectionName { get; }
}

public class RolesCollection
{
    private readonly DiscordClient _client;
    private readonly ulong _guildId;
    private readonly ILogger<RolesCollection> _logger;
    private readonly Regex _sectionPattern;
    private Dictionary<string, IList<DiscordRole>>? _roleSections;

    public RolesCollection(IOptions<GeneralOptions> generalOptions, IOptions<RolesOptions> roleOptions,
        DiscordClient client, ILogger<RolesCollection> logger)
    {
        _client = client;
        _guildId = generalOptions.Value.GuildId;
        _sectionPattern = new Regex(roleOptions.Value.SectionPattern);
        _logger = logger;
        // We don't fire on created because the role updated event fires when a new event has its name changed from default
        _client.GuildRoleUpdated += (_, _) => new Task(UpdateRoleSections);
        _client.GuildRoleDeleted += (_, _) => new Task(UpdateRoleSections);
    }

    private void UpdateRoleSections()
    {
        _logger.LogDebug("Updating role sections");
        _roleSections = new Dictionary<string, IList<DiscordRole>>();
        var roles = _client.Guilds[_guildId].Roles.Values.OrderByDescending(role => role.Position);
        var currentSection = "other";
        var currentSectionRoles = new List<DiscordRole>();
        foreach (var role in roles)
            if (_sectionPattern.IsMatch(role.Name))
            {
                _roleSections.Add(currentSection, currentSectionRoles);
                currentSection = _sectionPattern.Match(role.Name).Groups[1].Value.ToLower();
                currentSectionRoles = new List<DiscordRole>();
            }
            else
            {
                currentSectionRoles.Add(role);
            }

        _roleSections.Add(currentSection, currentSectionRoles);
    }

    public IList<DiscordRole> GetRoleSection(string sectionName)
    {
        if (_roleSections == default)
            UpdateRoleSections();

        try
        {
            return _roleSections![sectionName.ToLower()];
        }
        catch (KeyNotFoundException)
        {
            throw new RoleSectionNotFoundException(sectionName);
        }
    }
}