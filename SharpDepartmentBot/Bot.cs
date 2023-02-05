using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Configuration;

/// <summary>
/// в основном все украдено отсюда https://github.com/DSharpPlus/Example-Bots.git
/// </summary>
namespace SharpDepartmentBot
{
    public class Bot
    {
        public readonly EventId BotEventId = new(359, "GISandITSecDepartmentBot");
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public static void Main()
        {
            var prog = new Bot();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }
        public async Task RunBotAsync()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var cfg = new DiscordConfiguration
            {
                Token = configuration["Token"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.Guilds | DiscordIntents.GuildMessages,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };

            Client = new DiscordClient(cfg);
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;
            Client.GuildMemberAdded += Client_GuildMemberAdded;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configuration["Prefix"] },
                EnableDms = false,
                EnableMentionPrefix = true
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
            var role = e.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Студент").Value;
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
}
