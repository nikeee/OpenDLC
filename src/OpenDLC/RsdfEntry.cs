namespace OpenDLC
{
    public class RsdfEntry : DownloadEntry
    {
        public RsdfEntry(string url)
            : base(url)
        { }

        public static implicit operator string (RsdfEntry value) => value?.Url;
    }
}
