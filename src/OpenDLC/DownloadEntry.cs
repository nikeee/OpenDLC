namespace OpenDLC
{
    public abstract class DownloadEntry
    {
        public string Url { get; set; }

        protected DownloadEntry(string url)
        {
            Url = url;
        }

        public override string ToString() => Url;
    }
}
