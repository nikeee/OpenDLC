namespace OpenDLC;
public record DlcEntry : DownloadEntry
{
    public DlcEntry(string url) : base(url) { }

    public static implicit operator string(DlcEntry value) => value?.Url;
}
