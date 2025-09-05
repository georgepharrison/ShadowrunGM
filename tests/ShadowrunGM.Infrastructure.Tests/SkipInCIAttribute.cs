using System;
using Xunit;

namespace ShadowrunGM.Infrastructure.Tests;

/// <summary>
/// Marks tests that should be skipped in CI/CD environments or when database is not available.
/// </summary>
public sealed class SkipInCIAttribute : FactAttribute
{
    public SkipInCIAttribute()
    {
        // Check for CI environment variables or database availability
        if (IsRunningInCI() || !IsDatabaseAvailable())
        {
            Skip = "Test requires database infrastructure not available in current environment";
        }
    }

    private static bool IsRunningInCI()
    {
        // Common CI environment variables
        return Environment.GetEnvironmentVariable("CI") == "true" ||
               Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ||
               Environment.GetEnvironmentVariable("TF_BUILD") == "true" ||
               Environment.GetEnvironmentVariable("JENKINS_URL") != null;
    }

    private static bool IsDatabaseAvailable()
    {
        // For now, assume database is not available unless explicitly set
        return Environment.GetEnvironmentVariable("SHADOWRUN_TEST_DB") == "available";
    }
}