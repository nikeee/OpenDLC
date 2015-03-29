using System.Collections.ObjectModel;

namespace OpenDLC
{
    public abstract class DlcContainer<T> : Collection<T> where T : DlcEntry
    { }
}
