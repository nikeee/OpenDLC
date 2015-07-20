using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDLC
{
    public class DlcAppSettings
    {
        public string ApplicationId { get; }
        public string Secret { get; }
        public string Revision { get; }

        private byte[] _secretBytes = null;

        public DlcAppSettings(string appId, string appSecret, string revision)
        {
            ApplicationId = appId;
            Secret = appSecret;
            Revision = revision;
        }

        internal byte[] GetSecretBuffer()
        {
            if (_secretBytes == null)
                _secretBytes = ConvertEx.FromHexString(Secret);
            return _secretBytes;
        }
    }
}
