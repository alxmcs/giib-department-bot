namespace SharpDepartmentBot.Utils;

/// <summary>
/// Abstraction over the persistent storage used by the bot. Implemented by
/// <see cref="DataUtils"/> in production and can be swapped out in tests.
/// </summary>
public interface IDataStore
{
    /// <summary>
    /// Returns the schedule URL for the given group name, or an empty string
    /// when nothing matches (including non-numeric role names).
    /// </summary>
    string FindSchedule(string roleName);

    /// <summary>
    /// Returns a newline-joined, Discord-flavoured markdown list of the
    /// department's informational resources.
    /// </summary>
    string FindLinks();
}
