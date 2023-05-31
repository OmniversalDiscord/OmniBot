using DSharpPlus.SlashCommands;

namespace OmniBot.Commands;

public class CommandException : Exception
{
    public CommandException(string message) : base(message)
    {
    }
}

public class EphemeralCommandException : Exception 
{
    public EphemeralCommandException(string message) : base(message)
    {
    }
}