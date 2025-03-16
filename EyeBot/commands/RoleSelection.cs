using Discord;
using Discord.WebSocket;

namespace EyeBot.commands;

public class RoleSelection : ICommand
{
    public RoleSelection()
    {
        var cmd = new SlashCommandBuilder();
        cmd.WithName("test");
        cmd.WithDescription("Test");
        Properties = cmd.Build();
    }

    public SlashCommandProperties Properties { get; }

    public async Task Execute(SocketSlashCommandData data)
    {
        Console.WriteLine("COMMAND EXECUTED");
    }
}