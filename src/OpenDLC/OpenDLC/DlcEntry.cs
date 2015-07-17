namespace OpenDLC
{
    public class DlcEntry : DownloadEntry
    {
        public DlcEntry(string url)
            : base(url)
        { }

        public static implicit operator string (DlcEntry value)
        {
            return value == null ? null : value.Url;
        }
    }
}
