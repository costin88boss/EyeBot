using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace EyeBot.commands;

public class RoleSelection : ICommand
{
    public RoleSelection()
    {
        var cmd = new SlashCommandBuilder();
        cmd.WithName("addroleselection");
        cmd.WithDescription("Add role selection menu");
        cmd.WithContextTypes(InteractionContextType.Guild);
        cmd.WithIntegrationTypes(ApplicationIntegrationType.GuildInstall);

        cmd.AddOption("rolesamount", ApplicationCommandOptionType.Integer, "Amount of roles to add", true, minValue: 1, maxValue: 10);
        Properties = cmd.Build();
    }

    public SlashCommandProperties Properties { get; }

    public async Task Execute(DiscordSocketClient client, SocketSlashCommand cmd)
    {
        if (!cmd.GuildId.HasValue)
        {
            return;
        }
        if (!cmd.Permissions.Has(GuildPermission.ManageRoles | GuildPermission.ManageGuild))
        {
            await cmd.RespondAsync("You do not have permission to run this command.", ephemeral: true);
            return;
        }
        
        Int64 roleAmount = (Int64)cmd.Data.Options.ElementAt(0).Value;
        
    }

    private void addRoleMenu()
    {
        var compBuilder = new ComponentBuilder();

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Test")
            .WithCustomId("cmd_roleselectcreate_menu");


        IReadOnlyCollection<IRole> roles = client.GetGuild(cmd.GuildId.Value).Roles;
        IEnumerable<IRole> rolesOrdered = roles.OrderBy(role => role.Position).Reverse();
        
        foreach (var role in rolesOrdered)
        {
            menuBuilder.AddOption(new SelectMenuOptionBuilder
            {
                Label = role.Name,
                Value = role.Id.ToString()
            });

        }
        
        
        
        compBuilder.WithSelectMenu(menuBuilder);
        

        await cmd.RespondAsync("Role 1", ephemeral: true, components: compBuilder.Build());
    }
    
    public bool ComponentHandle(DiscordSocketClient client, SocketMessageComponent cmp)
    {
        switch (cmp.Data.CustomId)
        {
            case "cmd_roleselectcreate_menu":
                cmp.RespondAsync("ASDasd");
                break;
        }

        return false;
    }
}