using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpDepartmentBot.Commands;
using SharpDepartmentBot.Utils;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// в основном все украдено отсюда https://github.com/DSharpPlus/Example-Bots.git
/// </summary>
namespace SharpDepartmentBot;

public class Bot
{
    public readonly EventId BotEventId = new(359, "GISandITSecDepartmentBot");
    public DiscordClient Client { get; set; }
    public CommandsNextExtension Commands { get; set; }

    public static async Task Main() => await new Bot().RunBotAsync();

    public async Task RunBotAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("config.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Prefer the DISCORD_TOKEN environment variable; fall back to the
        // config file so existing deployments keep working.
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrEmpty(token))
            token = configuration["Token"];
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException(
                "Discord token is not configured. Set the DISCORD_TOKEN environment variable or provide \"Token\" in config.json.");

        var cfg = new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            // Only request the intents the bot actually uses. MessageContent
            // is a privileged intent but is required for prefix commands.
            Intents = DiscordIntents.Guilds
                      | DiscordIntents.GuildMembers
                      | DiscordIntents.GuildMessages
                      | DiscordIntents.MessageContents,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Debug
        };

        Client = new DiscordClient(cfg);
        Client.Ready += Client_Ready;
        Client.GuildAvailable += Client_GuildAvailable;
        Client.ClientErrored += Client_ClientError;
        Client.GuildMemberAdded += Client_GuildMemberAdded;

        var graduateGroups = configuration.GetSection("GraduateGroups").Get<string[]>()
            ?? BotConstants.DefaultGraduateGroups;

        var services = new ServiceCollection()
            .AddSingleton<IDataStore>(new DataUtils(configuration["Database"]))
            .AddSingleton(new RoleUtils(graduateGroups))
            .BuildServiceProvider();

        var commandsConfig = new CommandsNextConfiguration
        {
            StringPrefixes = new[] { configuration["Prefix"] },
            EnableDms = false,
            EnableMentionPrefix = true,
            Services = services
        };
        Commands = Client.UseCommandsNext(commandsConfig);
        Commands.CommandExecuted += Commands_CommandExecuted;
        Commands.CommandErrored += Commands_CommandErrored;
        Commands.RegisterCommands<BotCommands>();

        await Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
    {
        sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");
        return Task.CompletedTask;
    }

    private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");
        return Task.CompletedTask;
    }

    private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");
        return Task.CompletedTask;
    }

    private async Task Client_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        // Explicit null-safe lookup: default(KeyValuePair).Value would NRE
        // on GrantRoleAsync when the student role is missing on the guild.
        var role = e.Guild.Roles.Values.FirstOrDefault(r => r.Name == BotConstants.StudentRole);
        if (role == null)
        {
            sender.Logger.LogWarning(BotEventId,
                "Role \"{Role}\" not found on guild \"{Guild}\"; new member not auto-assigned.",
                BotConstants.StudentRole, e.Guild.Name);
            return;
        }
        await e.Member.GrantRoleAsync(role);
    }

    private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        var message = $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'";
        e.Context.Client.Logger.LogInformation(BotEventId, message);
        return Task.CompletedTask;
    }

    private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        var message = $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}";
        e.Context.Client.Logger.LogError(BotEventId, message, DateTime.Now);

        if (e.Exception is ChecksFailedException)
        {
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed);
        }
    }
}
