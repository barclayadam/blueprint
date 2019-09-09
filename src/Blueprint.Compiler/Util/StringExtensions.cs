using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Blueprint.Compiler.Util
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Reads text and returns an enumerable of strings for each line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLines(this string text)
        {
            var reader = new StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Reads text and calls back for each line of text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static void ReadLines(this string text, Action<string> callback)
        {
            var reader = new StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                callback(line);
            }
        }

        /// <summary>
        /// Splits a camel cased string into seperate words delimitted by a space
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        /// <summary>
        /// Splits a pascal cased string into seperate words delimitted by a space
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitPascalCase(this string str)
        {
            return SplitCamelCase(str);
        }
    }
}
