using Discord;
using Discord.Net;
using Discord.WebSocket;
using EyeBot.commands;
using Newtonsoft.Json;

namespace EyeBot;

internal class EyeBot
{
    private readonly DiscordSocketClient _client = new();

    private readonly Dictionary<string, ICommand> _commands = [];

    private event ButtonHandlerFuncSignature ButtonEventHandler = delegate { };

    private event ModalHandlerFuncSignature ModalEventHandler = delegate { };

    private event SelectMenuFuncSignature SelectMenuEventHandler = delegate { };

    private static void Main()
    {
        var token = Environment.GetEnvironmentVariable("EYEBOT_TOKEN") ?? throw new NullReferenceException("EYEBOT_TOKEN environment variable is not set");
        new EyeBot().MainAsync(token).GetAwaiter().GetResult();
    }

    private async Task MainAsync(string token)
    {
        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.SelectMenuExecuted += SelectMenuHandler;
        _client.ButtonExecuted += ButtonHandler;
        _client.ModalSubmitted += ModalHandler;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task AddCommand(ICommand command)
    {
        try
        {
            Console.WriteLine($"Adding command: {command.GetType().Name}");
            await _client.CreateGlobalApplicationCommandAsync(command.Properties);
            _commands.Add(command.Properties.Name.Value, command);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("ERROR - Could not add ICommand {0}", command.GetType().Name);
            throw new Exception();
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private void InitCommands()
    {
        AddCommand(new RoleSelection()).Wait();
        Console.WriteLine("Initializing commands...");

        foreach (var cmd in _commands.Values)
        {

            if (cmd is IModalCommand modalCmd)
                ModalEventHandler += modalCmd.ModalHandler;
            if (cmd is ISelectMenuCommand selectmenu)
                SelectMenuEventHandler += selectmenu.SelectMenuHandle;
            if (cmd is IButtonCommand button)
                ButtonEventHandler += button.ButtonHandle;
        }
    }

    private async Task ModalHandler(SocketModal cmp)
    {
        try
        {
            ModalEventHandler.Invoke(_client, cmp);
        }
        catch (KeyNotFoundException)
        {
            // "This interaction failed"
            // Not very ideal but idc
            await cmp.RespondAsync("This interaction failed", ephemeral: true);
        }
    }

    private async Task SelectMenuHandler(SocketMessageComponent cmp)
    {
        try
        {
            SelectMenuEventHandler.Invoke(_client, cmp);
        }
        catch (KeyNotFoundException)
        {
            // "This interaction failed"
            // Not very ideal but idc
            await cmp.RespondAsync("This interaction failed", ephemeral: true);
        }
    }

    private async Task ButtonHandler(SocketMessageComponent cmp)
    {
        try
        {
            ButtonEventHandler.Invoke(_client, cmp);
        }
        catch (KeyNotFoundException)
        {
            // "This interaction failed"
            // Not very ideal but idc
            await cmp.RespondAsync("This interaction failed", ephemeral: true);
        }
    }

    private Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            if (!_commands.TryGetValue(command.CommandName, out var cmd))
            {
                return command.RespondAsync("Unknown command.", ephemeral: true);
            }

            return cmd.Execute(_client, command);
        }
        catch (Exception error)
        {
            Console.WriteLine(error);
            return Task.CompletedTask;
        }
    }

    private Task Ready()
    {
        InitCommands();
        return Task.CompletedTask;
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private delegate void ButtonHandlerFuncSignature(DiscordSocketClient client, SocketMessageComponent cmp);
    private delegate void ModalHandlerFuncSignature(DiscordSocketClient client, SocketModal modal);

    private delegate void SelectMenuFuncSignature(DiscordSocketClient client, SocketMessageComponent cmp);

}