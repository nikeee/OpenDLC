using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenDLC
{
    public class RsdfContainer : DownloadContainer<RsdfEntry>
    {
        private static readonly byte[] Key = { 0x8C, 0x35, 0x19, 0x2D, 0x96, 0x4D, 0xC3, 0x18, 0x2C, 0x6F, 0x84, 0xF3, 0x25, 0x22, 0x39, 0xEB, 0x4A, 0x32, 0x0D, 0x25, 0x00, 0x00, 0x00, 0x00 };

        private static readonly byte[] IV = { 0xA3, 0xD5, 0xA3, 0x3C, 0xB9, 0x5A, 0xC1, 0xF5, 0xCB, 0xDB, 0x1A, 0xD2, 0x5C, 0xB0, 0xA7, 0xAA };

        private static readonly Rijndael Rijndael = new RijndaelManaged
        {
            Padding = PaddingMode.None,
            FeedbackSize = 8,
            Mode = CipherMode.CFB,
            BlockSize = 128,
            KeySize = 128
        };

        public static RsdfContainer FromFile(string fileName)
        {
            using (var f = File.OpenRead(fileName))
                return FromStream(f);
        }

#if FEATURE_TAP
        public static async Task<RsdfContainer> FromFileAsync(string fileName)
        {
            using (var f = File.OpenRead(fileName))
                return await FromStreamAsync(f).ConfigureAwait(false);
        }
#endif
        public static RsdfContainer FromStream(Stream stream)
        {
            var ms = stream as MemoryStream;
            if (ms != null)
            {
                var str = Encoding.UTF8.GetString(ms.ToArray());
                return FromString(str);
            }
            using (var reader = new StreamReader(stream))
                return FromString(reader.ReadToEnd());
        }

#if FEATURE_TAP
        public static async Task<RsdfContainer> FromStreamAsync(Stream stream)
        {
            var ms = stream as MemoryStream;
            if (ms != null)
            {
                var str = Encoding.UTF8.GetString(ms.ToArray());
                return FromString(str);
            }
            using (var reader = new StreamReader(stream))
            {
                var stringValue = await reader.ReadToEndAsync().ConfigureAwait(false);
                return FromString(stringValue);
            }
        }
#endif

        public static RsdfContainer FromString(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            value = value.Trim();
            if ((value.Length & 1) != 0)
                throw new ArgumentException("Invalid RSDF data");


            var data = ConvertEx.FromHexString(value);
            return DecryptData(data);
        }

        private static RsdfContainer DecryptData(byte[] data)
        {
            string dataStr = Encoding.UTF8.GetString(data);

            string[] lines = dataStr.Contains('�') ? dataStr.Split('�') : dataStr.Split('\n');

            var container = new RsdfContainer();

            using (var dec = Rijndael.CreateDecryptor(Key, IV))
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var linkData = Convert.FromBase64String(line);

                    var outputBuffer = new byte[linkData.Length];
                    dec.TransformBlock(linkData, 0, linkData.Length, outputBuffer, 0);

                    var link = Encoding.UTF8.GetString(outputBuffer);

                    const string ccfPrefix = "CCF:";
                    if (link.StartsWith(ccfPrefix))
                        link = link.Substring(ccfPrefix.Length).Trim();

                    container.Add(new RsdfEntry(link));
                }
            }

            return container;
        }
    }
}
