﻿using System.Collections.Generic;
using System.IO;

namespace Blueprint.Compiler.Tests;

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
}