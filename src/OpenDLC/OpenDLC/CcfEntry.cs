using System.Diagnostics;

namespace OpenDLC
{
    public class CcfEntry : DownloadEntry
    {
        public ulong FileSize { get; set; }
        public string FileName { get; set; }

        public CcfEntry(string url, ulong fileSize, string fileName)
            : base(url)
        {
            FileSize = fileSize;
            FileName = fileName;
        }

        internal CcfEntry(CcfDownload ccfDownload)
            : base(ccfDownload.Url)
        {
            Debug.Assert(ccfDownload != null);
            // ccfDownload = ccfDownload;

            ulong fsize;
            if (ulong.TryParse(ccfDownload.FileSize, out fsize))
                FileSize = fsize;
            FileName = ccfDownload.FileName;
        }

        public static implicit operator string (CcfEntry value)
        {
            // TODO: Remove this implicit conversion?
            return value == null ? null : value.Url;
        }
    }
}
