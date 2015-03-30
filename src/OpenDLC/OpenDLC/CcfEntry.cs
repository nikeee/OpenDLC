namespace OpenDLC
{
    public class CcfEntry : DlcEntry
    {
        public CcfEntry(string url)
            : base(url)
        { }

        public static implicit operator string(CcfEntry value)
        {
            return value == null ? null : value.Url;
        }
    }
}
