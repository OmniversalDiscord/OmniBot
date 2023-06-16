using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;
using Serilog.Core;
using Serilog.Events;

namespace OmniBot.Sinks;

public class DiscordSink : ILogEventSink
{
    private readonly DiscordClient _client;
    private readonly ulong _logChannelId;
    private readonly string _ping;
    private DiscordChannel? _logChannel;

    public DiscordSink(DiscordClient client, IOptions<DiscordLoggingOptions> options)
    {
        _client = client;
        _logChannelId = options.Value.Channel;
        _ping = $"<@{options.Value.PingUser}>";
    }

    public void Emit(LogEvent logEvent)
    {
        Task.Run(() => SendLog(logEvent));
    }

    private async Task SendLog(LogEvent logEvent)
    {
        if (_client.Guilds.Count == 0) return;

        // Discord logs min level is Warning
        if (logEvent.Level < LogEventLevel.Warning) return;

        // Filter out reconnecting messages as they're too frequent and spam me
        if (logEvent.MessageTemplate.Text.Contains("reconnecting")) return;

        if (_logChannel == null) _logChannel = await _client.GetChannelAsync(_logChannelId);

        var color = logEvent.Level switch
        {
            LogEventLevel.Warning => DiscordColor.Yellow,
            LogEventLevel.Error => DiscordColor.Red,
            LogEventLevel.Fatal => DiscordColor.Red,
            _ => DiscordColor.White
        };

        var message = logEvent.RenderMessage();
        var exception = logEvent.Exception;
        if (exception != null)
            message += $"\n```{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}```";

        var embed = new DiscordEmbedBuilder()
            .WithTitle(logEvent.Level.ToString())
            .WithDescription(message)
            .WithColor(color)
            .WithTimestamp(DateTimeOffset.Now);

        await _logChannel.SendMessageAsync(_ping, embed);
    }
}