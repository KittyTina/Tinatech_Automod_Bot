using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Text;
public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient _client;
    private IConfigurationRoot _config;
    private ulong guildId;

    public async Task MainAsync()
    {
        _config = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "json/config.json"))
        .Build();

        _client = new DiscordSocketClient();

        _client.Log += Log;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;

        await _client.LoginAsync(TokenType.Bot, _config["BOT_TOKEN"]);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "ping":
                await command.RespondAsync("Pong!");
                break;
            case "clearchat":
                if (!((SocketGuildUser)command.User).GuildPermissions.Administrator)
                {
                    await command.Channel.SendMessageAsync(null, false, CreateEmbed("Insufficient Permissions", "You don't have permissions to use this command!", Discord.Color.Red));
                    return;
                }
                double count = (double)command.Data.Options.First().Value;
                int counter = ((int)count);

                if (count <= 0)
                {
                    await command.Channel.SendMessageAsync(null, false, CreateEmbed("Logic Error", $"How am I supposed to remove {count} messages you donkey?????!!!", Discord.Color.Red));
                    return;
                }
                try
                {
                    await ClearChatAsync(command, counter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                break;
        }
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
    public async Task Client_Ready()
    {
        var guild_cmd = new SlashCommandBuilder();
        var rmvcmd = new SlashCommandBuilder();

        _client.GuildAvailable += async (guild) =>
        {
            if (guildId == default) 
            { 
                guildId = guild.Id;
                await Console.Out.WriteLineAsync($"Guild ID set to {guildId}");
            }
        };
        var guild = _client.GetGuild(guildId);
        guild_cmd.WithName("ping").WithDescription("Answers with pong");


        rmvcmd.WithName("clearchat").WithDescription("Clears the chat with the according number").AddOption("count", ApplicationCommandOptionType.Number, "e.g 3", true)
            .WithDefaultPermission(false);

        try
        {
            await guild.CreateApplicationCommandAsync(guild_cmd.Build());
            await guild.CreateApplicationCommandAsync(rmvcmd.Build());
        }
        catch (HttpException ex)
        {
            Console.WriteLine(JsonConvert.SerializeObject(ex.Errors, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public async Task ClearChatAsync(SocketSlashCommand cmdctx, int count)
    {
        var channel = cmdctx.Channel as SocketTextChannel;
        var messages = await channel.GetMessagesAsync(count + 1).FlattenAsync(); // +1 for last message that the bot sends
        await channel.DeleteMessagesAsync(messages);
    }

    private Embed CreateEmbed(string title, string description, Discord.Color color)
    {
        var builder = new EmbedBuilder();
        builder
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(color);
        return builder.Build();
    }
}