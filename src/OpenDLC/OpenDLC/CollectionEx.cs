using System.Collections.Generic;

namespace OpenDLC.Collections.Generic
{
    internal static class CollectionEx
    {
        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            if (collection == null)
                return true;
            return collection.Count == 0;
        }
    }
}
