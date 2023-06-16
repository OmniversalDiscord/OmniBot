namespace OmniBot;

public class GeneralOptions
{
    public const string Section = "General";
    public ulong GuildId { get; set; }
}

public class RolesOptions
{
    public const string Section = "Roles";
    public string SectionPattern { get; init; } = null!;
}

public class DiscordLoggingOptions
{
    public const string Section = "Logging:Discord";
    public ulong Channel { get; set; }
    public ulong PingUser { get; set; }
}