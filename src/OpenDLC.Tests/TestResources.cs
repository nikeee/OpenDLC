using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenDLC.Tests
{
    internal static class TestResources
    {
        private const string ResourceFolderName = "Resources";

        private static readonly char[] InvalidChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray();
        public static string GetResourcePath(string itemName)
        {
            if (InvalidChars.Any(itemName.Contains))
                throw new ArgumentException("Invalid item path");

            return System.IO.Path.Combine(ResourceFolderName, itemName);
        }
    }
}
