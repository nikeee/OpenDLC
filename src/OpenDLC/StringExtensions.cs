using System;

namespace OpenDLC
{
    internal static class StringExtensions
    {
        public static bool Contains(this string value, string needle, StringComparison comparisonType)
        {
            return value.IndexOf(needle, comparisonType) > -1;
        }
    }
}
