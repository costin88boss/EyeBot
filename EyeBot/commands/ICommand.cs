using Discord;
using Discord.WebSocket;

namespace EyeBot.commands;

public interface ICommand
{
    public SlashCommandProperties Properties { get; }

    public Task Execute(DiscordSocketClient client,  SocketSlashCommand cmd);
    public bool ComponentHandle(DiscordSocketClient client, SocketMessageComponent cmp);
}