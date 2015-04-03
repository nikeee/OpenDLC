using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OpenDLC
{
    public class CcfContainer : DownloadContainer<CcfPackage>
    {
        public Version Version { get; private set; }

        #region crypto

        private static readonly ReadOnlyCollection<byte[]> Keys = new ReadOnlyCollection<byte[]>(new[]
        {
            new byte[] {0x5F, 0x67, 0x9C, 0x00, 0x54, 0x87, 0x37, 0xE1, 0x20, 0xE6, 0x51, 0x8A, 0x98, 0x1B, 0xD0, 0xBA, 0x11, 0xAF, 0x5C, 0x71, 0x9E, 0x97, 0x50, 0x29, 0x83, 0xAD, 0x6A, 0xA3, 0x8E, 0xD7, 0x21, 0xC3},
            new byte[] {0x17, 0x1B, 0xF8, 0xE3, 0x4C, 0x3D, 0x0C, 0x0C, 0x26, 0x93, 0xFD, 0xD2, 0xB0, 0x80, 0x42, 0x3A, 0x5B, 0x98, 0xF4, 0xD0, 0x28, 0xA0, 0xAF, 0x4D, 0x82, 0xA3, 0x85, 0xD8, 0x37, 0xA8, 0xF9, 0x5F},
            new byte[] {0x02, 0x69, 0x00, 0xE9, 0x77, 0xC6, 0x40, 0x24, 0x42, 0xB6, 0x61, 0x32, 0x9C, 0xFE, 0x62, 0xD6, 0xED, 0x21, 0xBD, 0xEB, 0x0C, 0xD6, 0x32, 0x13, 0x18, 0xA8, 0xED, 0xC7, 0xBC, 0x5A, 0x6C, 0x86}
        });
        private static readonly ReadOnlyCollection<byte[]> IVs = new ReadOnlyCollection<byte[]>(new[]
        {
            new byte[] {0xE3, 0xD1, 0x53, 0xAD, 0x60, 0x9E, 0xF7, 0x35, 0x8D, 0x66, 0x68, 0x41, 0x80, 0xC7, 0x33, 0x1A},
            new byte[] {0x9F, 0xE9, 0x5F, 0xFF, 0x7C, 0xA4, 0xFC, 0x0F, 0xCE, 0xF2, 0x5E, 0x4F, 0x74, 0x44, 0xAE, 0x67},
            new byte[] {0x8C, 0xE1, 0x17, 0x3E, 0xBA, 0xD7, 0x6E, 0x08, 0x58, 0x4B, 0x94, 0x57, 0x39, 0x26, 0x23, 0x1E}
        });
        private static readonly ReadOnlyCollection<Version> Versions = new ReadOnlyCollection<Version>(new[]
        {
            new Version(1, 0),
            new Version(0, 8),
            new Version(0, 7)
        });

        #endregion

        public static CcfContainer FromFile(string fileName)
        {
            var buffer = File.ReadAllBytes(fileName);
            return FromBuffer(buffer);
        }

#if FEATURE_TAP
        public static async Task<CcfContainer> FromFileAsync(string fileName)
        {
            using (var f = File.OpenRead(fileName))
                return await FromStreamAsync(f).ConfigureAwait(false);
        }
#endif
        public static CcfContainer FromStream(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return FromBuffer(ms.ToArray());
            }
        }

#if FEATURE_TAP
        public static async Task<CcfContainer> FromStreamAsync(Stream stream)
        {
            var alreadyMs = stream as MemoryStream;
            if (alreadyMs != null)
            {
                var res = FromBuffer(alreadyMs.ToArray());
                return res;
            }
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                return FromBuffer(ms.ToArray());
            }
        }
#endif
        public static CcfContainer FromBuffer(byte[] buffer)
        {
            Version ccfVersion;
            var xml = DecryptXml(buffer, out ccfVersion);
            if (xml == null)
                throw new NotSupportedException("The CCF version is not supported.");

            var contents = SerializeFromXml(xml);
            if (contents == null || contents.Packages == null)
                throw new NotSupportedException("The CCF version is not supported.");

            var resContainer = new CcfContainer();

            var ps = contents.Packages;
            for (int i = 0; i < ps.Count; ++i)
            {
                var currentPackage = ps[i];
                if (currentPackage != null)
                    resContainer.Add(new CcfPackage(currentPackage));
            }

            resContainer.Version = ccfVersion;

            return resContainer;
        }


        private static RijndaelManaged CreateRijndael()
        {
            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros
            };
        }

        public override void SaveToStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream"); // TODO: nameof(stream)

            var buffer = SaveToBuffer();
            stream.Write(buffer, 0, buffer.Length);
        }

#if FEATURE_TAP
        public override async Task SaveToStreamAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream"); // TODO: nameof(stream)

            var buffer = SaveToBuffer();
            await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        }
#endif

        private byte[] SaveToBuffer()
        {
            const int version10KeyIndex = 0;

            var xmlData = SerializeToXml();
            var xmlDataBytes = Encoding.UTF8.GetBytes(xmlData);

            Debug.Assert(!string.IsNullOrEmpty(xmlData));
            using (var rij = CreateRijndael())
            {
                rij.IV = IVs[version10KeyIndex];
                rij.Key = Keys[version10KeyIndex];
                using (var enc = rij.CreateEncryptor())
                {
                    var output = new byte[xmlData.Length];
                    var written = enc.TransformBlock(xmlDataBytes, 0, xmlDataBytes.Length, output, 0);

                    var outputResized = new byte[written];
                    Buffer.BlockCopy(output, 0, outputResized, 0, written);

                    Debug.Assert(outputResized.Length != 0);

                    return outputResized;
                }
            }
        }

        private static string DecryptXml(byte[] data, out Version usedVersion)
        {
            using (var rij = CreateRijndael())
            {
                // There are more than one key. Just try them out.
                for (int i = 0; i < Keys.Count; ++i)
                {
                    rij.IV = IVs[i];
                    rij.Key = Keys[i];
                    using (var dec = rij.CreateDecryptor())
                    {
                        var output = new byte[data.Length];
                        var written = dec.TransformBlock(data, 0, data.Length, output, 0);

                        var outputResized = new byte[written];
                        Buffer.BlockCopy(output, 0, outputResized, 0, written);

                        var xmlData = Encoding.UTF8.GetString(outputResized);

                        Debug.Assert(xmlData != null);
                        Debug.Assert(xmlData[xmlData.Length - 1] != '\0');

                        if (IsValidContainerXml(xmlData))
                        {
                            usedVersion = Versions[i];
                            return xmlData;
                        }
                    }
                }
            }
            usedVersion = null;
            return null; // Failed to decrypt
        }

        private static bool IsValidContainerXml(string data)
        {
            const string Identifier = "<CryptLoad>";
            return data.Contains(Identifier, StringComparison.InvariantCultureIgnoreCase);
        }

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(CryptLoadContainer));
        //private static readonly XmlReaderSettings _settings = new XmlReaderSettings
        //{
        //    ConformanceLevel = ConformanceLevel.Fragment,
        //    IgnoreWhitespace = true,
        //    IgnoreComments = true
        //};

        private static CryptLoadContainer SerializeFromXml(string xmlData)
        {
            Debug.Assert(xmlData != null);
            xmlData = xmlData.Trim();
            using (var reader = new StringReader(xmlData))
            using (var rr = XmlReader.Create(reader))
            {
                return _serializer.Deserialize(rr) as CryptLoadContainer;
            }
        }

        private string SerializeToXml()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var ww = XmlWriter.Create(writer))
                _serializer.Serialize(ww, this);
            return sb.ToString();
        }
    }
}
