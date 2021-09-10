using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace GIiIBDepartmentBot
{
    public class BotCommands : BaseCommandModule
    {
        #region !giiib role
        [Command("role"), Description("Присваивает роль студенту в соответствии с его никнеймом")]
        public async Task GrantRole(CommandContext ctx)
        {
            var role = GetRole(ctx);
            if (role != null)
                await ApplyRoleChanges(ctx, role);
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }
        private DiscordRole GetRole(CommandContext ctx)
        {

            var roleViaNick = string.IsNullOrEmpty(ctx.Member.Nickname) ? null : ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == ctx.Member.Nickname.Split(" ").LastOrDefault()).Value;
            var roleViaDisp = string.IsNullOrEmpty(ctx.Member.DisplayName) ? null : ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == ctx.Member.DisplayName.Split(" ").LastOrDefault()).Value;
            if (roleViaNick != null)
                return roleViaNick;
            else if (roleViaDisp != null)
                return roleViaDisp;
            else
                return null;
        }
        private async Task ApplyRoleChanges(CommandContext ctx, DiscordRole role)
        {
            var roles = new List<DiscordRole>();
            roles.AddRange(ctx.Member.Roles.ToArray());
            for (int i=0; i< roles.Count; i++)
                if(roles[i].Name !="Студент")
                    await ctx.Member.RevokeRoleAsync(roles[i]);
            await ctx.Member.GrantRoleAsync(role);
            await ctx.RespondAsync($"Теперь ты в группе {role.Name}!");
        }
        #endregion

        #region !giiib schedule
        [Command("schedule"), Description("Выдает ссылку на расписание группы студента в соответствии с его группой")]
        public async Task ShowSchedule(CommandContext ctx)
        {
            var role = GetRole(ctx);
            if (role != null)
                await FindSchedule(ctx, role.Name);
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }
        /// <summary>
        /// возвращает ссылки на расписание из захардкоженного json-а, что довольно паршиво - его (скорее всего) придется менять раз в год
        /// </summary>
        /// <remarks>to do: выяснить, если ли у помойки под названием ssau.ru какое-нибудь api для получения ссылки на расписание запросом</remarks>
        private async Task FindSchedule(CommandContext ctx, string roleName)
        {
            using var fs = File.OpenRead("schedule.json");
            using var sr = new StreamReader(fs, new UTF8Encoding(false));
            var json = await sr.ReadToEndAsync();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (dict.ContainsKey(roleName))
                await ctx.RespondAsync(dict[roleName]);
            else
                await ctx.RespondAsync($"Для группы {roleName} расписания не нашлось");
        }
        #endregion

        #region !giiib links
        [Command("links"), Description("Выдает ссылки на информационные ресурсы кафедры")]
        public async Task ShowLinks(CommandContext ctx)
        {
            using var fs = File.OpenRead("links.json");
            using var sr = new StreamReader(fs, Encoding.GetEncoding(1251));
            var json = await sr.ReadToEndAsync();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            var message = "Информационные ресурсы кафедры ГИиИБ:\n";
            foreach (var k in dict.Keys)
            {
                message +=$"{k}\n<{dict[k]}>\n";
            }
            await ctx.RespondAsync(message);
        }
        #endregion
    }
}
