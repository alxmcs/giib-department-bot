using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpDepartmentBot.Utils;

public static class RoleUtils
{
    private static readonly List<string> _GradGroups = new List<string>() { "6511", "6512", "6513", "6514" };
    private const string _GradRole = "Выпускник";
    private const string _StudentRole = "Студент";
    private const string _BaseRole = "@everyone";
    public static DiscordRole GetRole(CommandContext ctx) =>
        string.IsNullOrEmpty(ctx.Member.Nickname) ?
            null :
            ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == ctx.Member.Nickname.Split(" ").LastOrDefault()).Value;
    public static bool CheckGraduate(CommandContext ctx) => ctx.Member.Roles.Select(x => x.Name).Intersect(_GradGroups).Any();
    public static async Task ApplyRoleChanges(CommandContext ctx, DiscordRole role)
    {
        var roles = new List<DiscordRole>();
        roles.AddRange(ctx.Member.Roles.ToArray());
        for (int i = 0; i < roles.Count; i++)
            if (roles[i].Name != _StudentRole && roles[i].Name != _BaseRole)
                await ctx.Member.RevokeRoleAsync(roles[i]);
        await ctx.Member.GrantRoleAsync(role);
        await ctx.RespondAsync($"Теперь ты в группе {role.Name}!");
    }
    public static async Task ApplyGraduateChanges(CommandContext ctx)
    {
        var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == _GradRole).Value;
        var roles = new List<DiscordRole>();
        roles.AddRange(ctx.Member.Roles.ToArray());
        for (int i = 0; i < roles.Count; i++)
            if (roles[i].Name != _BaseRole)
                await ctx.Member.RevokeRoleAsync(roles[i]);
        await ctx.Member.GrantRoleAsync(role);
        await ctx.RespondAsync($"Теперь ты {role.Name}!");
    }
}
