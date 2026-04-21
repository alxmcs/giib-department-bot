using System;
using System.Data.SQLite;
using System.Text;

namespace SharpDepartmentBot.Utils;

/// <summary>
/// SQLite-backed implementation of <see cref="IDataStore"/>. Caches the links
/// query result in-memory since it is effectively static after startup.
/// </summary>
public class DataUtils : IDataStore
{
    private const string GetScheduleSql = "SELECT \"Url\" FROM \"Schedule\" WHERE \"Group\"=@group LIMIT 1";
    private const string GetLinksSql = "SELECT \"Name\", \"Url\" FROM \"Resources\"";

    private readonly string _connectionString;
    private readonly object _linksLock = new();
    private string _cachedLinks;

    public DataUtils(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must be provided", nameof(connectionString));
        _connectionString = connectionString;
    }

    public string FindSchedule(string roleName)
    {
        if (string.IsNullOrEmpty(roleName) || !int.TryParse(roleName, out var groupNumber))
            return string.Empty;

        var schedule = string.Empty;
        using var con = new SQLiteConnection(_connectionString);
        con.Open();
        using var cmd = new SQLiteCommand(GetScheduleSql, con);
        cmd.Parameters.AddWithValue("@group", groupNumber);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            schedule = rd.GetString(0);
        return schedule;
    }

    public string FindLinks()
    {
        if (_cachedLinks is not null)
            return _cachedLinks;

        lock (_linksLock)
        {
            if (_cachedLinks is not null)
                return _cachedLinks;

            var builder = new StringBuilder();
            using var con = new SQLiteConnection(_connectionString);
            con.Open();
            using var cmd = new SQLiteCommand(GetLinksSql, con);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                builder.Append('[').Append(rd.GetString(0))
                       .Append("](<").Append(rd.GetString(1)).Append(">)\n");
            _cachedLinks = builder.ToString();
            return _cachedLinks;
        }
    }

    /// <summary>Test helper to discard the cached links result.</summary>
    public void InvalidateCache() => _cachedLinks = null;
}
