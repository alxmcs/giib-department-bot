﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SharpDepartmentBot.Utils;
using System.Threading.Tasks;

namespace SharpDepartmentBot.Commands
{
    public class BotCommands : BaseCommandModule
    {
        [Command("role"), Description("Присваивает роль студенту в соответствии с его никнеймом")]
        public async Task GrantRole(CommandContext ctx)
        {
            var role = RoleUtils.GetRole(ctx);
            if (role != null)
                await RoleUtils.ApplyRoleChanges(ctx, role);
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }

        [Command("graduate"), Description("Присваивает студенту последнего курса роль выпускника")]
        public async Task GrantGraduate(CommandContext ctx)
        {
            if (RoleUtils.CheckGraduate(ctx))
                await RoleUtils.ApplyGraduateChanges(ctx);
            else
                await ctx.RespondAsync("Ты не на последнем курсе!");
        }

        [Command("schedule"), Description("Выдает ссылку на расписание группы студента в соответствии с его группой")]
        public async Task ShowSchedule(CommandContext ctx)
        {
            var role = RoleUtils.GetRole(ctx);
            if (role != null)
            {
                var result = DataUtils.FindSchedule(role.Name);
                if (!string.IsNullOrEmpty(result))
                    await ctx.RespondAsync(result);
                else
                    await ctx.RespondAsync($"Для группы {role.Name} расписания не нашлось");
            }
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }

        [Command("links"), Description("Выдает ссылки на информационные ресурсы кафедры")]
        public async Task ShowLinks(CommandContext ctx) => await ctx.RespondAsync(DataUtils.FindLinks());
    }
}
