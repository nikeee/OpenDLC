﻿using System;
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
            var lines = this.Select(entry => entry.Url).ToArray();

            var sb = new StringBuilder();
            var outputBuffer = new byte[4096];

            var nextIv = RsdfFormat.IV;
            for (int i = 0; i < lines.Length; ++i)
            {
                var currentLink = lines[i];
                if (string.IsNullOrEmpty(currentLink))
                    continue;

                var linkBytes = Encoding.UTF8.GetBytes(currentLink);
                var output = RsdfFormat.Algorithm.EncryptCfb(linkBytes, nextIv);
                var b64 = Convert.ToBase64String(output);

                sb.Append(b64).Append("\r\n");

                nextIv = AppendShiftToLeft(nextIv, output);
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


            byte[] nextIv = RsdfFormat.IV;
            foreach (var paddedLine in lines)
            {
                Debug.Assert(nextIv.Length == 16);

                var line = paddedLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var linkData = Convert.FromBase64String(line);

                var outputBuffer = RsdfFormat.Algorithm.DecryptCfb(linkData, nextIv);

                var link = Encoding.ASCII.GetString(outputBuffer);

                const string ccfPrefix = "CCF:";
                if (link.StartsWith(ccfPrefix))
                    link = link[ccfPrefix.Length..].Trim();

                container.Add(new RsdfEntry(link));

                nextIv = AppendShiftToLeft(nextIv , linkData);
            }

            return container;
        }

        private static byte[] AppendShiftToLeft(ReadOnlySpan<byte> a, byte[] b)
        {
            var res = new byte[a.Length];
            var start = a.Length - b.Length;
            a.CopyTo(res);

            if (start >= 0)
                Buffer.BlockCopy(b, 0, res, start, b.Length);
            else
                Buffer.BlockCopy(b, start * -1, res, 0, res.Length);

            return res;
        }
    }
}
