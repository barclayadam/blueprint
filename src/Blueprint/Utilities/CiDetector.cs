using System;

namespace Blueprint.Utilities;

/// <summary>
/// Simple class to detect whether we are running in a CI environment.
/// </summary>
internal static class CiDetector
{
    /// <summary>
    /// Are we running in a CI environment?
    /// </summary>
    /// <returns>Whether the environment is a CI system.</returns>
    internal static readonly bool IsRunningOnCiServer =
            Environment.GetEnvironmentVariable("APPVEYOR") != null ||
            Environment.GetEnvironmentVariable("BITRISE_IO") != null ||
            Environment.GetEnvironmentVariable("BUILD_CI") != null ||
            Environment.GetEnvironmentVariable("BUILD_ID") != null ||
            Environment.GetEnvironmentVariable("BUILDKITE") == "true" ||
            Environment.GetEnvironmentVariable("CI") != null ||
            Environment.GetEnvironmentVariable("CIRCLECI") == "true" ||
            Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" ||
            Environment.GetEnvironmentVariable("GITLAB_CI") != null ||
            Environment.GetEnvironmentVariable("HEROKU_TEST_RUN_ID ") != null ||
            Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null ||
            Environment.GetEnvironmentVariable("TF_BUILD") == "true" ||
            Environment.GetEnvironmentVariable("TRAVIS") == "true";
}
