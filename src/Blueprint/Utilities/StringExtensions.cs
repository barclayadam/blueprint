using System;
using System.Text;

namespace Blueprint.Utilities;

/// <summary>
/// Provides a number of extension methods to the built-in <see cref="string"/> class.
/// </summary>
public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    public static TEnumType AsEnum<TEnumType>(this string value) where TEnumType : struct
    {
        Guard.NotNullOrEmpty(nameof(value), value);

        return (TEnumType)Enum.Parse(typeof(TEnumType), value);
    }

    public static TEnumType? AsNullableEnum<TEnumType>(this string value) where TEnumType : struct
    {
        return value?.AsEnum<TEnumType>();
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