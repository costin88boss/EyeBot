using Discord;
using Discord.WebSocket;

namespace EyeBot.commands;

public interface ICommand
{
    public SlashCommandProperties Properties { get; }

    public Task Execute(DiscordSocketClient client, SocketSlashCommand cmd);
}

public interface ISelectMenuCommand
{
    public void SelectMenuHandle(DiscordSocketClient client, SocketMessageComponent cmp);
}

public interface IButtonCommand
{
    public void ButtonHandle(DiscordSocketClient client, SocketMessageComponent button);
}

public interface IModalCommand
{
    public void ModalHandler(DiscordSocketClient client, SocketModal modal);
}