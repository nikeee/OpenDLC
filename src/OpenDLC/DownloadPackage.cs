using System.Collections.ObjectModel;

namespace OpenDLC
{
    public abstract class DownloadPackage<T> : Collection<T> where T : DownloadEntry
    {
        // May be used for later abstraction
    }
}
