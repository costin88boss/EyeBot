using Discord;
using Discord.Net;
using Discord.WebSocket;
using EyeBot.commands;
using Newtonsoft.Json;

namespace EyeBot;

internal class EyeBot
{
    private readonly DiscordSocketClient _client = new();

    private readonly Dictionary<string, ICommand> _commands = new();

    private event ButtonHandlerFuncSignature ButtonHandlerEventHandler = delegate { };

    private static void Main()
    {
        var token = Environment.GetEnvironmentVariable("EYEBOT_TOKEN");
        if (token == null) throw new NullReferenceException("EYEBOT_TOKEN environment variable is not set");
        new EyeBot().MainAsync(token).GetAwaiter().GetResult();
    }

    private async Task MainAsync(string token)
    {
        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.SelectMenuExecuted += SelectMenuHandler;
        _client.ButtonExecuted += ButtonHandler;

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

        foreach (var cmd in _commands.Values) ButtonHandlerEventHandler += cmd.ComponentHandle;
    }

    private Task SelectMenuHandler(SocketMessageComponent cmp)
    {
        try
        {
            ButtonHandlerEventHandler.Invoke(_client, cmp);
        }
        catch (KeyNotFoundException)
        {
            // "This interaction failed"
            // Not very ideal but idc
            cmp.RespondAsync("", ephemeral: true);
        }

        return Task.CompletedTask;
    }


    private Task ButtonHandler(SocketMessageComponent cmp)
    {
        try
        {
            ButtonHandlerEventHandler.Invoke(_client, cmp);
        }
        catch (KeyNotFoundException)
        {
            // "This interaction failed"
            // Not very ideal but idc
            cmp.RespondAsync("", ephemeral: true);
        }

        return Task.CompletedTask;
    }

    private Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            _commands[command.CommandName].Execute(_client, command).Wait();
        }
        catch (KeyNotFoundException)
        {
            // "This interaction failed"
            // Not very ideal but idc
            command.RespondAsync("", ephemeral: true);
        }

        return Task.CompletedTask;
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
}