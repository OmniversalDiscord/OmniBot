using DSharpPlus.Entities;

namespace OmniBot.Commands.Extensions;

public static class MemberExtensions
{
    public static bool IsCoffeeCrew(this DiscordMember member)
    {
        return member.Roles.Any(role => role.Name == "Coffee Crew");
    }
}