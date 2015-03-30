
namespace OpenDLC
{
    public class CcfEntry : DlcEntry
    {
        public ulong FileSize { get; set; }
        public string FileName { get; set; }

        public CcfEntry(string url, ulong fileSize, string fileName)
            : base(url)
        {
            FileSize = fileSize;
            FileName = fileName;
        }

        public static implicit operator string(CcfEntry value)
        {
            // TODO: Remove this implicit conversion?
            return value == null ? null : value.Url;
        }
    }
}
