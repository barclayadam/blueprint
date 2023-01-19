using System.Text;

namespace Blueprint.Utilities;

/// <summary>
/// Provides a number of extension methods to the built-in <see cref="string"/> class.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether two strings are equal or not, ignoring any new line characters.
    /// </summary>
    /// <param name="a">First string to compare.</param>
    /// <param name="b">Seconds string to compare.</param>
    /// <returns>Whether the two strings can be considered equal.</returns>
    public static bool EqualsIgnoringNewlines(string a, string b)
    {
        if (a.Length == 0 && b.Length == 0)
        {
            return true;
        }

        var i = 0;
        var j = 0;

        while (true)
        {
            // Skip any new line characters in both a and b strings
            while (i < a.Length && (a[i] == '\n' || a[i] == '\r'))
            {
                i++;
            }

            while (j < b.Length && (b[j] == '\n' || b[j] == '\r'))
            {
                j++;
            }

            // We have reached the end of a string, break out and check for length after while loop
            if (i >= a.Length || j >= b.Length)
            {
                break;
            }

            // We have found a difference, immediately return false
            if (a[i] != b[j])
            {
                return false;
            }

            i++;
            j++;
        }

        // We must have reached the end of BOTH strings. If that is not the case it means, ignoring new lines
        // one of the strings is larger and therefore not equal
        return i == a.Length && j == b.Length;
    }

    public static string ToPascalCase(this string s)
    {
        var sb = new StringBuilder(s.Length);
        var toUpper = true;

        for (var i = 0; i < s.Length; i++)
        {
            var curChar = s[i];

            switch (curChar)
            {
                // Next should be uppercase, do NOT append " ", "-", "_"
                case ' ':
                case '-':
                case '_':
                    toUpper = true;
                    continue;

                // A digit will always be appended, next character should be uppercase (i.e. 8ball -> 8Ball)
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    toUpper = true;
                    sb.Append(curChar);
                    continue;

                default:
                    if (toUpper)
                    {
                        sb.Append(char.ToUpper(curChar));
                        toUpper = false;
                    }
                    else
                    {
                        var nextIsUpper = i < s.Length - 1 && char.IsLower(s[i + 1]);
                        var prevIsLower = i > 0 && char.IsLower(s[i - 1]);

                        if (nextIsUpper)
                        {
                            sb.Append(curChar);
                        }
                        else if (prevIsLower && (curChar == 'A' || curChar == 'I'))
                        {
                            sb.Append(curChar);
                        }
                        else
                        {
                            sb.Append(char.ToLower(curChar));
                        }
                    }

                    break;
            }
        }

        return sb.ToString();
    }
}
