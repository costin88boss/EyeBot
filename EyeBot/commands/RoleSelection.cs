using System.Collections;
using System.Diagnostics;
using Discord;
using Discord.WebSocket;

namespace EyeBot.commands;

public class RoleSelection : ICommand
{
    private readonly ArrayList _makerMenus = new();


    public RoleSelection()
    {
        var cmd = new SlashCommandBuilder();
        cmd.WithName("addroleselection");
        cmd.WithDescription("Add role selection menu");
        cmd.WithContextTypes(InteractionContextType.Guild);
        cmd.WithIntegrationTypes(ApplicationIntegrationType.GuildInstall);

        cmd.AddOption("rolesamount", ApplicationCommandOptionType.Integer, "Amount of roles to add", true, minValue: 1,
            maxValue: 10);
        Properties = cmd.Build();
    }

    public SlashCommandProperties Properties { get; }

    public async Task Execute(DiscordSocketClient client, SocketSlashCommand cmd)
    {
        foreach (MakerMenu makerMenu in _makerMenus)
            if (cmd.ChannelId == makerMenu.ChannelId)
            {
                await cmd.RespondAsync("A role selection menu is in progress in this channel.", ephemeral: true);
                return;
            }

        if (!cmd.GuildId.HasValue) return;
        if (!cmd.Permissions.Has(GuildPermission.ManageRoles | GuildPermission.ManageGuild))
        {
            await cmd.RespondAsync("You do not have permission to run this command.", ephemeral: true);
            return;
        }

        long roleAmount = (long) cmd.Data.Options.ElementAt(0).Value;

        Debug.Assert(cmd.ChannelId.HasValue);
        MakerMenu menu = new()
        {
            MaxRoleCount = roleAmount,
            AtRole = 0, // hacky
            RolesIds = new ulong[roleAmount],
            ChannelId = cmd.ChannelId.Value,
        };
        
        _makerMenus.Add(menu);

        AddRoleMenu(client, cmd, menu);
    }

    public void ComponentHandle(DiscordSocketClient client, SocketMessageComponent cmp)
    {
        switch (cmp.Data.CustomId)
        {
            case "cmd_roleselectcreate_menu":
                AddRoleMenu(client, cmp, _makerMenus.Cast<MakerMenu>().First(menu => menu.ChannelId == cmp.ChannelId),
                    cmp.Data.Values.ElementAt(0));
                break;
        }
    }

    private void AddRoleMenu(DiscordSocketClient client, SocketInteraction msg, MakerMenu menu, string? roleVal = null)
    {
        if (roleVal != null) {
            menu.RolesIds[menu.AtRole - 1] = ulong.Parse(roleVal);
        }
        
        if (menu.AtRole == menu.MaxRoleCount)
        {
            msg.DeleteOriginalResponseAsync();
            
            
            
            foreach (var role in menu.RolesIds)
            {
                Debug.Assert(role != 0);

                ButtonBuilder button = new();
                    //button.WithCustomId("cmd_roleselectcreate_menu_addrole");
                    // TODO
            }
            
            // TODO: buttons to gain roles

            //msg.Channel

            return;
        }

        var compBuilder = new ComponentBuilder();

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder(" ")
            .WithCustomId("cmd_roleselectcreate_menu");


        Debug.Assert(msg.GuildId.HasValue);
        IReadOnlyCollection<IRole> roles = client.GetGuild(msg.GuildId.Value).Roles;
        var rolesOrdered = roles.OrderBy(role => role.Position).Reverse();

        foreach (var role in rolesOrdered)
            menuBuilder.AddOption(new SelectMenuOptionBuilder
            {
                Label = role.Name,
                Value = role.Id.ToString()
            });

        compBuilder.WithSelectMenu(menuBuilder);
        
        msg.RespondAsync("Role #" + (menu.AtRole + 1), ephemeral: true, components: compBuilder.Build()).Wait();
        menu.AtRole++;
    }

    private record MakerMenu
    {
        public ulong ChannelId;
        public required ulong[] RolesIds;
        public long AtRole;
        public long MaxRoleCount;
    }
}