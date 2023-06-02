using DSharpPlus.Entities;

namespace OmniBot.Commands.Extensions;

public static class MemberExtensions
{
    public static bool HasRole(this DiscordMember member, string roleName)
    {
        return member.Roles.Any(role => role.Name == roleName);
    }
    
    public static bool IsCoffeeCrew(this DiscordMember member)
    {
        return member.HasRole("Coffee Crew");
    }
}