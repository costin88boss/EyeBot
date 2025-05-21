using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;

namespace EyeBot.commands;

public class RoleSelection : ICommand, IModalCommand, ISelectMenuCommand, IButtonCommand
{

    public SlashCommandProperties Properties { get; }


    public RoleSelection()
    {
        var cmd = new SlashCommandBuilder();
        cmd.WithName("addroleselection");
        cmd.WithDescription("Add role selection menu");
        cmd.WithContextTypes(InteractionContextType.Guild);
        cmd.WithIntegrationTypes(ApplicationIntegrationType.GuildInstall);
        cmd.WithDefaultMemberPermissions(GuildPermission.ManageRoles | GuildPermission.ManageGuild);

        Properties = cmd.Build();
    }

    public async Task Execute(DiscordSocketClient client, SocketSlashCommand cmd)
    {
        if (!cmd.GuildId.HasValue)
        {
            await cmd.RespondAsync("You can only run this command in a guild", ephemeral: true);
            return;
        }

        var compBuilder = new ComponentBuilder();
        var menuBuilder = new SelectMenuBuilder()
            .WithCustomId("roleSelectMenu")
            .WithPlaceholder("Select a role")
            .AddOption("Select a role", "none");
        var guild = client.GetGuild(cmd.GuildId!.Value);
        if (guild == null)
        {
            cmd.RespondAsync("Guild not found.", ephemeral: true).Wait();
            return;
        }
        foreach (var role in guild.Roles.OrderBy(r => r.Position).Reverse())
        {
            menuBuilder.AddOption(role.Name, role.Id.ToString());
        }
        compBuilder.WithSelectMenu(menuBuilder);
        cmd.RespondAsync($"Please select a role for the message", ephemeral: true, components: compBuilder.Build()).Wait();
    }

    public void SelectMenuHandle(DiscordSocketClient client, SocketMessageComponent cmp)
    {
        if (cmp.Data.CustomId == "roleSelectMenu")
        {
            if (cmp.User is not SocketGuildUser user || !user.GuildPermissions.ManageRoles)
            {
                cmp.RespondAsync("You do not have permission on this command.", ephemeral: true).Wait();
                return;
            }

            var selectedRoleId = cmp.Data.Values.First();
            if (selectedRoleId == "none") return;
            var guild = client.GetGuild(cmp.GuildId!.Value);
            var role = guild.GetRole(ulong.Parse(selectedRoleId));

            if (role == null)
            {
                cmp.RespondAsync("Role not found.", ephemeral: true).Wait();
                return;
            }

            var modal = new ModalBuilder()
            .WithTitle("Role Selection Content")
            .WithCustomId($"rolemodal-{role.Id}")
            .AddTextInput("Message content", "message", TextInputStyle.Paragraph, "Enter content for this role message.");

            cmp.RespondWithModalAsync(modal.Build()).Wait();
        }
    }

    public void ModalHandler(DiscordSocketClient client, SocketModal modal)
    {
        if (modal.Data.CustomId.StartsWith("rolemodal"))
        {
            if (modal.User is not SocketGuildUser user || !user.GuildPermissions.ManageRoles)
            {
                modal.RespondAsync("You do not have permission on this command.", ephemeral: true).Wait();
                return;
            }
            var content = modal.Data.Components.First().Value.ToString();

            var roleId = modal.Data.CustomId.Split('-')[1];
            var guild = client.GetGuild(modal.GuildId!.Value);
            var role = guild.GetRole(ulong.Parse(roleId));

            var button = new ComponentBuilder()
                    .WithButton("Give/Takes Role", $"role-{role.Id}");

            modal.Channel.SendMessageAsync(content, components: button.Build());
            modal.RespondAsync("Done", ephemeral: true);
        }
    }

    public async void ButtonHandle(DiscordSocketClient client, SocketMessageComponent button)
    {
        if (button.Data.CustomId.StartsWith("role"))
        {
            if (button.User is not SocketGuildUser _user || !_user.GuildPermissions.ManageRoles)
            {
                button.RespondAsync("You do not have permission on this command.", ephemeral: true).Wait();
                return;
            }
            var roleId = button.Data.CustomId.Split('-')[1];

            var guild = client.GetGuild(button.GuildId!.Value);
            var role = guild.GetRole(ulong.Parse(roleId));

            if (role == null)
            {
                await button.RespondAsync("Role not found.", ephemeral: true);
                return;
            }

            if (button.User is not SocketGuildUser user)
            {
                await button.RespondAsync("User not found.", ephemeral: true);
                return;
            }

            var bot = guild.GetUser(client.CurrentUser.Id);
            if (bot == null)
            {
                await button.RespondAsync("Bot not found.", ephemeral: true);
                return;
            }

            if (bot.Hierarchy <= role.Position)
            {
                await button.RespondAsync("I cannot assign this role because it is higher than my role in the hierarchy. Please contact an admin or moderator.", ephemeral: true);
                return;
            }

            try
            {
                if (user.Roles.Contains(role))
                {
                    await user.RemoveRoleAsync(role);
                    await button.RespondAsync($"Role {role.Name} has been removed from you.", ephemeral: true);
                }
                else
                {
                    await user.AddRoleAsync(role);
                    await button.RespondAsync($"Role {role.Name} has been added to you.", ephemeral: true);
                }
            }
            catch (Discord.Net.HttpException ex) when (ex.HttpCode == System.Net.HttpStatusCode.Forbidden)
            {
                await button.RespondAsync("I don't have permission to assign this role. Please contact an admin or moderator.", ephemeral: true);
            }
        }
    }
}