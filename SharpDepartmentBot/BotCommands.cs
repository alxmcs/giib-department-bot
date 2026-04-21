using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SharpDepartmentBot.Utils;
using System.Threading.Tasks;

namespace SharpDepartmentBot.Commands;

public class BotCommands : BaseCommandModule
{
    private readonly IDataStore _dataStore;
    private readonly RoleUtils _roleUtils;

    public BotCommands(IDataStore dataStore, RoleUtils roleUtils)
    {
        _dataStore = dataStore;
        _roleUtils = roleUtils;
    }

    [Command("role"), Description("Присваивает роль студенту в соответствии с его никнеймом")]
    public async Task GrantRole(CommandContext ctx)
    {
        var role = RoleUtils.GetRole(ctx);
        if (role != null)
            await RoleUtils.ApplyRoleChanges(ctx, role);
        else
            await ctx.RespondAsync(RoleUtils.NicknameError);
    }

    [Command("graduate"), Description("Присваивает студенту последнего курса роль выпускника")]
    public async Task GrantGraduate(CommandContext ctx)
    {
        if (_roleUtils.CheckGraduate(ctx))
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
            var result = _dataStore.FindSchedule(role.Name);
            if (!string.IsNullOrEmpty(result))
                await ctx.RespondAsync(result);
            else
                await ctx.RespondAsync($"Для группы {role.Name} расписания не нашлось");
        }
        else
            await ctx.RespondAsync(RoleUtils.NicknameError);
    }

    [Command("links"), Description("Выдает ссылки на информационные ресурсы кафедры")]
    public async Task ShowLinks(CommandContext ctx) => await ctx.RespondAsync(_dataStore.FindLinks());
}
