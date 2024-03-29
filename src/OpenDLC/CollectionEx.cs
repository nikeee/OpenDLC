﻿using System.Collections.Generic;

namespace OpenDLC.Collections.Generic
{
    internal static class CollectionEx
    {
        public static bool IsNullOrEmpty<T>(ICollection<T> collection) => collection == null || collection.Count == 0;
    }
}
