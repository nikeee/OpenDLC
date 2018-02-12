namespace OpenDLC
{
    public class DlcPackage : DownloadPackage<DlcEntry>
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Category { get; set; }
    }
}
