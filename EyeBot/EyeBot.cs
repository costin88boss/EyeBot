using Discord;
using Discord.WebSocket;
using EyeBot.commands;

namespace EyeBot;

internal class EyeBot
{
    private readonly DiscordSocketClient _client = new();

    private readonly Dictionary<string, ICommand> _commands = new();

    private static void Main(string[] args)
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
    }

    private void InitCommands()
    {
        AddCommand(new RoleSelection()).Wait();
    }

    private Task SlashCommandHandler(SocketSlashCommand command)
    {
        _commands[command.CommandName].Execute(command.Data);
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
}