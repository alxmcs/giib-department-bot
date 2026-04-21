namespace SharpDepartmentBot;

/// <summary>
/// Shared role-name constants used by both event handlers and commands.
/// Keeping them in a single place avoids magic strings scattered across the codebase
/// and keeps behaviour aligned with the Python implementation.
/// </summary>
public static class BotConstants
{
    public const string StudentRole = "Студент";
    public const string GraduateRole = "Выпускник";
    public const string EveryoneRole = "@everyone";

    /// <summary>
    /// Default list of graduate group names; can be overridden via the
    /// <c>GraduateGroups</c> array in <c>config.json</c>.
    /// </summary>
    public static readonly string[] DefaultGraduateGroups = { "6511", "6512", "6513", "6514" };
}
