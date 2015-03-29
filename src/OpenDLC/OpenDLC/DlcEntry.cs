namespace OpenDLC
{
    public abstract class DlcEntry
    {
        public string Url { get; set; }

        protected DlcEntry(string url)
        {
            Url = url;
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
