using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

namespace SharpDepartmentBot.Utils;

public static class RoleUtils
{
    public static DiscordRole GetRole(CommandContext ctx)
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

    public static async Task ApplyRoleChanges(CommandContext ctx, DiscordRole role)
    {
        var roles = new List<DiscordRole>();
        roles.AddRange(ctx.Member.Roles.ToArray());
        for (int i = 0; i < roles.Count; i++)
            if (roles[i].Name != "Студент" && roles[i].Name != "@everyone")
                await ctx.Member.RevokeRoleAsync(roles[i]);
        await ctx.Member.GrantRoleAsync(role);
        await ctx.RespondAsync($"Теперь ты в группе {role.Name}!");
    }

    public static bool CheckGraduate(CommandContext ctx)
    {
        var gradGroups = new List<string>() { "6511", "6512", "6513", "6514" };
        var roles = ctx.Member.Roles;
        return roles.Select(x => x.Name).Intersect(gradGroups).Any();
    }

    public static async Task ApplyGraduateChanges(CommandContext ctx)
    {
        var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Выпускник").Value;
        var roles = new List<DiscordRole>();
        roles.AddRange(ctx.Member.Roles.ToArray());
        for (int i = 0; i < roles.Count; i++)
            if (roles[i].Name != "@everyone")
                await ctx.Member.RevokeRoleAsync(roles[i]);
        await ctx.Member.GrantRoleAsync(role);
        await ctx.RespondAsync($"Теперь ты {role.Name}!");
    }
}
