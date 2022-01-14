using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// в основном все украдено отсюда https://github.com/DSharpPlus/Example-Bots.git
/// </summary>
namespace SharpDepartmentBot
{
    public struct Configuration
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
    }

    public class Bot
    {
        public readonly EventId BotEventId = new EventId(359, "GISandITSecDepartmentBot");
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public static void Main(string[] args)
        {
            var prog = new Bot();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }
        public async Task RunBotAsync()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            #region Loading configuration
            var json = "";
            using var fs = File.OpenRead("config.json");
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            json = await sr.ReadToEndAsync();
            var cfgjson = JsonConvert.DeserializeObject<Configuration>(json);
            var cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.Guilds | DiscordIntents.GuildMessages,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            #endregion

            #region Setting up a client
            this.Client = new DiscordClient(cfg);
            #region Setting up event handlers
            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientErrored += this.Client_ClientError;
            this.Client.GuildMemberAdded += this.Client_GuildMemberAdded;
            #endregion

            #region Setting up commands
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { cfgjson.CommandPrefix },
                EnableDms = false,
                EnableMentionPrefix = true
            };
            this.Commands = this.Client.UseCommandsNext(commandsConfig);
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;
            this.Commands.RegisterCommands<BotCommands>();
            #endregion
            #endregion
            await this.Client.ConnectAsync();
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
            e.Context.Client.Logger.LogInformation(BotEventId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }
        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(BotEventId, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            if (e.Exception is ChecksFailedException ex)
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
