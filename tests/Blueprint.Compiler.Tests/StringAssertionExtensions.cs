using Shouldly;

namespace Blueprint.Compiler.Tests.Compilation
{
    public static class StringAssertionExtensions
    {
        public static void ShouldContainIgnoringNewlines(this string actual, string expected)
        {
            RemoveNewLines(actual).ShouldContain(RemoveNewLines(expected));
        }

        private static string RemoveNewLines(string actual)
        {
            return actual.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }
    }
}
