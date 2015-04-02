using System.Collections.ObjectModel;

namespace OpenDLC
{
    public abstract class DownloadContainer<T> : Collection<T> where T : DownloadEntry
    { }
}
