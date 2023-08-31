using Microsoft.Extensions.Configuration;
using System.Data.SQLite;

namespace SharpDepartmentBot.Utils;

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
