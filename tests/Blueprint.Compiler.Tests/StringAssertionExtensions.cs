using FluentAssertions;

namespace Blueprint.Compiler.Tests
{
    public static class StringAssertionExtensions
    {
        public static void ShouldContainIgnoringNewlines(this string actual, string expected)
        {
            RemoveNewLines(actual).Should().Contain(RemoveNewLines(expected));
        }

        private static string RemoveNewLines(string actual)
        {
            return actual.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }
    }
}
