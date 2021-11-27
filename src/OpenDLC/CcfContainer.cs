using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OpenDLC
{
    public partial class CcfContainer : DownloadContainer<CcfPackage>
    {
        public Version Version { get; private set; }

        #region from*

        public static CcfContainer FromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var buffer = File.ReadAllBytes(fileName);
            return FromBuffer(buffer);
        }

        public static CcfContainer FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream is MemoryStream ms)
            {
                return FromBuffer(ms.ToArray());
            }
            using var newMs = new MemoryStream();
            stream.CopyTo(newMs);
            return FromBuffer(newMs.ToArray());
        }

        #endregion

        public static CcfContainer FromBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length == 0)
                throw new ArgumentException("Empty " + nameof(buffer));

            var xml = DecryptXml(buffer, out Version ccfVersion);
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


        public override void SaveToStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var buffer = SaveToBuffer();
            stream.Write(buffer, 0, buffer.Length);
        }


        private byte[] SaveToBuffer()
        {
            const int version10KeyIndex = 0;

            var xmlDataBytes = SerializeToXml();
            using var rij = CcfFormat.CreateSymmetricAlgorithm();
            rij.IV = CcfFormat.IVs[version10KeyIndex];
            rij.Key = CcfFormat.Keys[version10KeyIndex];

            using var enc = rij.CreateEncryptor();
            var outputResized = enc.TransformFinalBlock(xmlDataBytes, 0, xmlDataBytes.Length);

            Debug.Assert(outputResized.Length != 0);

            return outputResized;
        }

        private static string DecryptXml(byte[] data, out Version usedVersion)
        {
            Debug.Assert(data != null);
            Debug.Assert(data.Length > 0);

            using (var rij = CcfFormat.CreateSymmetricAlgorithm())
            {
                var output = new byte[4096];
                // There is more than one key. Just try them out.
                for (int i = 0; i < CcfFormat.Keys.Count; ++i)
                {
                    rij.IV = CcfFormat.IVs[i];
                    rij.Key = CcfFormat.Keys[i];

                    using var dec = rij.CreateDecryptor();
                    var written = dec.TransformBlock(data, 0, data.Length, output, 0);

                    var outputResized = new byte[written];
                    Buffer.BlockCopy(output, 0, outputResized, 0, written);

                    var xmlData = Encoding.UTF8.GetString(outputResized);
                    if (xmlData[^1] == '\0')
                    {
                        // Fallback for \0 padding
                        xmlData = xmlData.Remove(xmlData.IndexOf('\0'));
                        Debug.Assert(xmlData.Length > 0);
                    }

                    Debug.Assert(xmlData != null);
                    Debug.Assert(xmlData[xmlData.Length - 1] != '\0');

                    if (IsValidContainerXml(xmlData))
                    {
                        usedVersion = CcfFormat.Versions[i];
                        return xmlData;
                    }
                }
            }
            usedVersion = null;
            return null; // Failed to decrypt
        }

        private static bool IsValidContainerXml(string data)
        {
            Debug.Assert(data != null);
            const string Identifier = "<CryptLoad";
            return data.Contains(Identifier, StringComparison.InvariantCultureIgnoreCase);
        }

        #region serialization

        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(CryptLoadContainer));

        private static CryptLoadContainer SerializeFromXml(string xmlData)
        {
            Debug.Assert(xmlData != null);
            xmlData = xmlData.Trim();
            using var reader = new StringReader(xmlData);
            using var rr = XmlReader.Create(reader);
            return _serializer.Deserialize(rr) as CryptLoadContainer;
        }

        private byte[] SerializeToXml()
        {
            var serCon = CryptLoadContainer.FromCcfContainer(this);
            using var ms = new MemoryStream();

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty); // No namespaces because the others don't do it as well

            using (var ww = XmlWriter.Create(ms, CcfFormat.WriterSettings))
                _serializer.Serialize(ww, serCon, namespaces);

            return ms.ToArray();
        }

        #endregion
    }
}
