using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenDLC
{
    public partial class RsdfContainer : DownloadContainer<RsdfEntry>
    {
        public static RsdfContainer FromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            using var f = File.OpenRead(fileName);
            return FromStream(f);
        }

        public static RsdfContainer FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream is MemoryStream ms)
            {
                var str = Encoding.UTF8.GetString(ms.ToArray());
                return FromString(str);
            }

            using var reader = new StreamReader(stream);
            return FromString(reader.ReadToEnd());
        }

        public static RsdfContainer FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            value = value.Trim();
            if ((value.Length & 1) != 0)
                throw new ArgumentException("Invalid RSDF data");

            var data = ConvertEx.FromHexString(value);
            return DecryptData(data);
        }

        public override void SaveToStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var str = SaveAsString();
            Debug.Assert(!string.IsNullOrWhiteSpace(str));
            var stringBuffer = Encoding.UTF8.GetBytes(str);
            stream.Write(stringBuffer, 0, stringBuffer.Length);
        }

        public string SaveAsString()
        {
            using var enc = RsdfFormat.Algorithm.CreateEncryptor(RsdfFormat.Key, RsdfFormat.IV);
            var lines = this.Select(entry => entry.Url).ToArray();

            var sb = new StringBuilder();
            var outputBuffer = new byte[4096];

            for (int i = 0; i < lines.Length; ++i)
            {
                var currentLink = lines[i];
                if (string.IsNullOrEmpty(currentLink))
                    continue;

                var linkBytes = Encoding.UTF8.GetBytes(currentLink);
                var written = enc.TransformBlock(linkBytes, 0, linkBytes.Length, outputBuffer, 0);
                var sizedOutput = new byte[written];
                Buffer.BlockCopy(outputBuffer, 0, sizedOutput, 0, written);

                var b64 = Convert.ToBase64String(sizedOutput);

                sb.Append(b64).Append("\r\n");
            }

            var unencodedData = Encoding.UTF8.GetBytes(sb.ToString());
            Debug.Assert(unencodedData != null);
            Debug.Assert(unencodedData.Length > 0);

            var res = ConvertEx.ToHexString(unencodedData);

            Debug.Assert(!string.IsNullOrWhiteSpace(res));
            Debug.Assert(res.Length == unencodedData.Length * 2);

            return res;
        }

        private static RsdfContainer DecryptData(byte[] data)
        {
            string dataStr = Encoding.UTF8.GetString(data);

            string[] lines = dataStr.Contains('�')
                    ? dataStr.Split('�')
                    : dataStr.Split('\n');

            var container = new RsdfContainer();

            using (var dec = RsdfFormat.Algorithm.CreateDecryptor(RsdfFormat.Key, RsdfFormat.IV))
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
