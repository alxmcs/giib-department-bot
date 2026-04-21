using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpDepartmentBot.Utils;

/// <summary>
/// Role-management helpers. Instance-scoped so graduate groups can be supplied
/// from configuration (matching the Python implementation's single source of
/// truth) and can be easily replaced in tests.
/// </summary>
public class RoleUtils
{
    public const string NicknameError = "Назови себя нормально! Никнейм должен быть вида *ФИО НомерГруппы*";

    private readonly HashSet<string> _graduateGroups;

    public RoleUtils(IEnumerable<string> graduateGroups)
    {
        _graduateGroups = new HashSet<string>(graduateGroups ?? BotConstants.DefaultGraduateGroups);
    }

    /// <summary>
    /// Resolves the guild role matching the last whitespace-separated token of
    /// the member's nickname. Returns <c>null</c> when the member has no
    /// nickname or when no matching role exists.
    /// </summary>
    public static DiscordRole GetRole(CommandContext ctx)
    {
        if (ctx?.Member == null || string.IsNullOrEmpty(ctx.Member.Nickname))
            return null;

        var groupToken = ctx.Member.Nickname.Split(' ').LastOrDefault();
        if (string.IsNullOrEmpty(groupToken))
            return null;

        // Use explicit null-aware lookup instead of FirstOrDefault().Value which
        // would NRE when the role doesn't exist (KeyValuePair default has a
        // null Value).
        return ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == groupToken);
    }

    public bool CheckGraduate(CommandContext ctx) =>
        ctx?.Member != null &&
        ctx.Member.Roles.Select(x => x.Name).Intersect(_graduateGroups).Any();

    public static async Task ApplyRoleChanges(CommandContext ctx, DiscordRole role)
    {
        // Atomic role replacement — avoids partial state if one of the
        // individual revoke calls fails mid-loop.
        var preserved = ctx.Member.Roles
            .Where(r => r.Name == BotConstants.StudentRole || r.Name == BotConstants.EveryoneRole)
            .ToList();
        preserved.Add(role);
        await ctx.Member.ReplaceRolesAsync(preserved);
        await ctx.RespondAsync($"Теперь ты в группе {role.Name}!");
    }

    public static async Task ApplyGraduateChanges(CommandContext ctx)
    {
        var role = ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == BotConstants.GraduateRole);
        if (role == null)
        {
            await ctx.RespondAsync($"На сервере отсутствует роль \"{BotConstants.GraduateRole}\", обратитесь к администратору.");
            return;
        }

        // @everyone cannot be removed, so we simply replace all roles with
        // only the graduate role; Discord preserves @everyone implicitly.
        await ctx.Member.ReplaceRolesAsync(new[] { role });
        await ctx.RespondAsync($"Теперь ты {role.Name}!");
    }
}
