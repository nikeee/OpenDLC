
namespace OpenDLC
{
    public record DlcAppSettings(string ApplicationId, string Secret, string Revision)
    {
        private byte[] _secretBytes = null;

        internal byte[] GetSecretBuffer() => _secretBytes ??= ConvertEx.FromHexString(Secret);
    }
}
