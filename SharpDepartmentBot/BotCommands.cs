using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SharpDepartmentBot
{
    public class BotCommands : BaseCommandModule
    {
        #region !giiib role
        [Command("role"), Description("Присваивает роль студенту в соответствии с его никнеймом")]
        public async Task GrantRole(CommandContext ctx)
        {
            var role = RoleUtils.GetRole(ctx);
            if (role != null)
                await RoleUtils.ApplyRoleChanges(ctx, role);
            else
                await ctx.RespondAsync("Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*");
        }
        #endregion

        #region !giiib graduate
        [Command("graduate"), Description("Присваивает студенту последнего курса роль выпускника")]
        public async Task GrantGraduate(CommandContext ctx)
        {
            if (RoleUtils.CheckGraduate(ctx))
                await RoleUtils.ApplyGraduateChanges(ctx);
            else
                await ctx.RespondAsync("Ты не на последнем курсе!");
        }

        #endregion

        #region !giiib schedule
        [Command("schedule"), Description("Выдает ссылку на расписание группы студента в соответствии с его группой")]
        public async Task ShowSchedule(CommandContext ctx)
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

        #endregion

        #region !giiib links
        [Command("links"), Description("Выдает ссылки на информационные ресурсы кафедры")]
        public async Task ShowLinks(CommandContext ctx) => await ctx.RespondAsync(DataUtils.FindLinks());
        #endregion
    }
}
