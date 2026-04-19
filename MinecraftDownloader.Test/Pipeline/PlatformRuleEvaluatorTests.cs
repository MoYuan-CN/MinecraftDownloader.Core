using MinecraftDownloader.Core.Models;
using MinecraftDownloader.Core.Pipeline;

namespace MinecraftDownloader.Test.Pipeline;

public class PlatformRuleEvaluatorTests
{
    [Fact]
    public void IsAllowed_WithNullRules_ReturnsTrue()
    {
        var result = PlatformRuleEvaluator.IsAllowed(null);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithEmptyRulesList_ReturnsTrue()
    {
        var rules = new List<LibraryRule>();

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithAllowRuleForCurrentOs_ReturnsTrue()
    {
        var currentOs = GetCurrentOsName();
        var rules = new List<LibraryRule>
        {
            new("allow", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithDisallowRuleForCurrentOs_ReturnsFalse()
    {
        var currentOs = GetCurrentOsName();
        var rules = new List<LibraryRule>
        {
            new("disallow", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithRuleForDifferentOs_DoesNotApplyThatRule()
    {
        var differentOs = GetDifferentOsName();
        var rules = new List<LibraryRule>
        {
            new("allow", new OsCondition(differentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithAllowRuleWithoutOsCondition_ReturnsTrue()
    {
        var rules = new List<LibraryRule>
        {
            new("allow", null)
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithDisallowRuleWithoutOsCondition_ReturnsFalse()
    {
        var rules = new List<LibraryRule>
        {
            new("disallow", null)
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithMultipleRules_LastMatchingRuleWins()
    {
        var currentOs = GetCurrentOsName();
        var rules = new List<LibraryRule>
        {
            new("allow", new OsCondition(currentOs)),
            new("disallow", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithMultipleRules_LastMatchingRuleWins_Reverse()
    {
        var currentOs = GetCurrentOsName();
        var rules = new List<LibraryRule>
        {
            new("disallow", new OsCondition(currentOs)),
            new("allow", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithMixedOsRules_OnlyCurrentOsRulesApply()
    {
        var currentOs = GetCurrentOsName();
        var differentOs = GetDifferentOsName();
        var rules = new List<LibraryRule>
        {
            new("allow", new OsCondition(differentOs)),
            new("disallow", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_DefaultIsDisallow_WhenExplicitRulesPresentButNoneMatch()
    {
        var differentOs = GetDifferentOsName();
        var rules = new List<LibraryRule>
        {
            new("allow", new OsCondition(differentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_ActionIsCaseInsensitive()
    {
        var currentOs = GetCurrentOsName();
        var rules = new List<LibraryRule>
        {
            new("ALLOW", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_OsNameIsCaseInsensitive()
    {
        var currentOs = GetCurrentOsName().ToUpperInvariant();
        var rules = new List<LibraryRule>
        {
            new("allow", new OsCondition(currentOs))
        };

        var result = PlatformRuleEvaluator.IsAllowed(rules);

        Assert.True(result);
    }

    private static string GetCurrentOsName()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsMacOS()) return "osx";
        return "linux";
    }

    private static string GetDifferentOsName()
    {
        if (OperatingSystem.IsWindows()) return "osx";
        return "windows";
    }
}
