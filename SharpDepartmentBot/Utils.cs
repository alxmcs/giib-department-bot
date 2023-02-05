using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Data.SQLite;

namespace SharpDepartmentBot.Utils
{
    public static class DataUtils
    {
        private static readonly string _ConnectionString = new ConfigurationBuilder().AddJsonFile("config.json").Build()["Database"];
        private static readonly string _GetSchedule = "SELECT \"Url\" FROM \"Schedule\" WHERE \"Group\"=@group LIMIT 1";
        private static readonly string _GetLinks = "SELECT \"Name\", \"Url\" FROM \"Resources\"";

        public static string FindSchedule(string roleName)
        {
            var schedule = string.Empty;
            if (roleName != null && !string.IsNullOrEmpty(roleName))
            {
                using var con = new SQLiteConnection(_ConnectionString);
                con.Open();
                using var cmd = new SQLiteCommand(_GetSchedule, con);
                cmd.Parameters.AddWithValue("@group", int.Parse(roleName));
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                    schedule = rd.GetString(0);
            }
            return schedule;
        }
        public static string FindLinks()
        {
            var links = string.Empty;
            using var con = new SQLiteConnection(_ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(_GetLinks, con);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                links += $"{rd.GetString(0)}\n<{rd.GetString(1)}>\n";
            return links;
        }
    }
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
}
