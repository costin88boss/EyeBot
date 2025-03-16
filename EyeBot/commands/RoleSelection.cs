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
        cmd.WithContextTypes(InteractionContextType.Guild);
        cmd.WithIntegrationTypes(ApplicationIntegrationType.GuildInstall);
        Properties = cmd.Build();
    }

    public SlashCommandProperties Properties { get; }

    public Task Execute(SocketSlashCommandData data)
    {
        Console.WriteLine("COMMAND EXECUTED");
        
        return Task.CompletedTask;
    }
}