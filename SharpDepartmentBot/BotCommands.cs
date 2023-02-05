using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SharpDepartmentBot.Utils;

namespace SharpDepartmentBot.Commands
{
    public class BotCommands : BaseCommandModule
    {
        [Command("role"), Description("Присваивает роль студенту в соответствии с его никнеймом")]
        public static async Task GrantRole(CommandContext ctx)
        {
            var role = RoleUtils.GetRole(ctx);
            if (role != null)
                await RoleUtils.ApplyRoleChanges(ctx, role);
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }

        [Command("graduate"), Description("Присваивает студенту последнего курса роль выпускника")]
        public static async Task GrantGraduate(CommandContext ctx)
        {
            if (RoleUtils.CheckGraduate(ctx))
                await RoleUtils.ApplyGraduateChanges(ctx);
            else
                await ctx.RespondAsync("Ты не на последнем курсе!");
        }

        [Command("schedule"), Description("Выдает ссылку на расписание группы студента в соответствии с его группой")]
        public static async Task ShowSchedule(CommandContext ctx)
        {
            var role = RoleUtils.GetRole(ctx);
            if (role != null)
            {
                var result = DataUtils.FindSchedule(role.Name);
                if(!string.IsNullOrEmpty(result))
                    await ctx.RespondAsync(result);
                else
                    await ctx.RespondAsync($"Для группы {role.Name} расписания не нашлось");
            }
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }

        [Command("links"), Description("Выдает ссылки на информационные ресурсы кафедры")]
        public static async Task ShowLinks(CommandContext ctx) => await ctx.RespondAsync(DataUtils.FindLinks());
    }
}
