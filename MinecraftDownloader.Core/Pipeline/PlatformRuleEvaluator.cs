using MinecraftDownloader.Core.Models;

namespace MinecraftDownloader.Core.Pipeline;

/// <summary>Evaluates Mojang library rules against the current OS.</summary>
internal static class PlatformRuleEvaluator
{
    private static readonly string CurrentOs = DetectOs();

    /// <returns><c>true</c> when the library should be downloaded on this machine.</returns>
    public static bool IsAllowed(IReadOnlyList<LibraryRule>? rules)
    {
        if (rules is null || rules.Count == 0)
            return true;

        // Rules are evaluated in order; last matching rule wins.
        // Default is "disallow" when explicit rules are present.
        var allowed = false;

        foreach (var rule in rules)
        {
            var applies = rule.Os is null || rule.Os.Name.Equals(CurrentOs, StringComparison.OrdinalIgnoreCase);

            if (applies)
                allowed = rule.Action.Equals("allow", StringComparison.OrdinalIgnoreCase);
        }

        return allowed;
    }

    private static string DetectOs()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsMacOS()) return "osx";
        return "linux";
    }
}