﻿using System;

namespace Blueprint.Compiler.Util;

internal static class StringExtensions
{
    /// <summary>
    /// Splits the given string in a non-allocating manner in to lines. See https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <returns>An enumerator that contains all lines from the given string.</returns>
    public static LineSplitEnumerator SplitLines(this string str)
    {
        // LineSplitEnumerator is a struct so there is no allocation here
        return new LineSplitEnumerator(str.AsSpan());
    }

    // Must be a ref struct as it contains a ReadOnlySpan<char>
    public ref struct LineSplitEnumerator
    {
        private ReadOnlySpan<char> _str;

        public LineSplitEnumerator(ReadOnlySpan<char> str)
        {
            this._str = str;
            this.Current = default;
        }

        public LineSplitEntry Current { get; private set; }

        // Needed to be compatible with the foreach operator
        public LineSplitEnumerator GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            var span = this._str;

            // Reach the end of the string
            if (span.Length == 0)
            {
                return false;
            }

            var index = span.IndexOfAny('\r', '\n');

            // The string is composed of only one line
            if (index == -1)
            {
                this._str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                this.Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
                return true;
            }

            if (index < span.Length - 1 && span[index] == '\r')
            {
                // Try to consume the '\n' associated to the '\r'
                var next = span[index + 1];
                if (next == '\n')
                {
                    this.Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 2));
                    this._str = span.Slice(index + 2);
                    return true;
                }
            }

            this.Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 1));
            this._str = span.Slice(index + 1);
            return true;
        }
    }

    public readonly ref struct LineSplitEntry
    {
        public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
        {
            this.Line = line;
            this.Separator = separator;
        }

        public ReadOnlySpan<char> Line { get; }

        public ReadOnlySpan<char> Separator { get; }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry)
        {
            return entry.Line;
        }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
        // https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
        {
            line = this.Line;
            separator = this.Separator;
        }
    }
}