﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace OpenDLC
{
    public class DlcContainer : DownloadContainer<DlcPackage>
    {
        public DlcGenerator Generator { get; set; }
        public DlcTribute Tribute { get; set; }
        public string XmlVersion { get; set; }

        public static Task<DlcContainer> FromFileAsync(string fileName, DlcAppSettings applicationSettings)
        {
            if (applicationSettings == null)
                throw new ArgumentNullException(nameof(applicationSettings));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var str = File.ReadAllText(fileName);
            return FromStringAsync(str, applicationSettings);
        }
        private static async Task<DlcContainer> FromStringAsync(string fileContent, DlcAppSettings applicationSettings)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentException("Invalid file contents");
            Debug.Assert(applicationSettings != null);

            var key = fileContent.Substring(fileContent.Length - DlcFormat.TempKeyLength);
            fileContent = fileContent.Remove(fileContent.Length - DlcFormat.TempKeyLength);

            var fileContentBuffer = Convert.FromBase64String(fileContent.Trim());
            var keyBuffer = Convert.FromBase64String(key.Trim());

            var tempKey = await CallJDService(applicationSettings.ApplicationId, applicationSettings.Revision, key);

            var secretBuffer = applicationSettings.GetSecretBuffer();

            var decryptionKey = CreateDecryptionKey(tempKey, secretBuffer);

            var encodedXml = DecryptContainer(fileContentBuffer, decryptionKey);

            var decodedDocument = DecodeXmlData(encodedXml);

            return ContainerFromDocument(decodedDocument);
        }

        private static DlcContainer ContainerFromDocument(XDocument doc)
        {
            var container = new DlcContainer();
            var dlc = doc.Element("dlc");
            if (dlc != null)
            {
                var header = dlc.Element("header");
                if (header != null)
                {
                    var generator = header.Element("generator");
                    if (generator != null)
                    {

                        var application = generator.Element("app")?.Value;
                        var url = generator.Element("url")?.Value;

                        Version? v = null;
                        var version = generator.Element("version");
                        if (version != null)
                        {
                            if (Version.TryParse(version.Value, out Version ver))
                                v = ver;
                        }

                        container.Generator = new DlcGenerator(application, v, url);
                    }

                    var tribute = header.Element("tribute");
                    if (tribute != null)
                    {
                        container.Tribute = new DlcTribute(tribute.Element("name")?.Value);
                    }

                    container.XmlVersion = header.Element("dlcxmlversion")?.Value;
                }

                var content = dlc.Element("content");
                if (content != null)
                {
                    var packages = content.Elements("package");
                    foreach (var p in packages)
                    {
                        if (p == null)
                            continue;

                        var packageObj = new DlcPackage
                        {
                            Name = p.Attribute("name")?.Value,
                            Comment = p.Attribute("comment")?.Value,
                            Category = p.Attribute("category")?.Value
                        };

                        var files = p.Elements("file");
                        foreach (var f in files)
                        {
                            if (f == null)
                                continue;

                            var url = f.Element("url");
                            if (url != null)
                                packageObj.Add(new DlcEntry(url.Value));
                        }

                        container.Add(packageObj);
                    }
                }
            }
            return container;
        }

        private static async Task<byte[]> CallJDService(string appId, string revision, string data)
        {
            var ub = new UriBuilder(DlcFormat.JdServiceUrl);
            var query = HttpUtility.ParseQueryString(ub.Query);
            query["destType"] = "jdtc6";
            query["b"] = appId;
            query["srcType"] = "dlc";
            query["data"] = data;
            query["v"] = revision;
            ub.Query = query.ToString();

            var msg = new HttpRequestMessage(HttpMethod.Post, ub.Uri);

            using var cl = new HttpClient();
            using var res = await cl.SendAsync(msg).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
                throw new DlcDecryptionException("Server responded with unsuccessful HTTP status code.");

            var resContent = await res.Content.ReadAsStringAsync().ConfigureAwait(false) ?? string.Empty;

            var match = Regex.Match(resContent, "<rc>(.*)</rc>");
            if (!match.Success)
                throw new DlcDecryptionException($"Server responded with non-XML content:{Environment.NewLine}{resContent}");

            var tempKey = match.Groups[1]?.Value ?? string.Empty;
            tempKey = tempKey.Trim();
            if (tempKey == string.Empty)
                throw new DlcDecryptionException("Server responded with empty decryption key.");

            if (tempKey == DlcFormat.RateLimitExceededKey)
                throw new DlcLimitExceededException();

            return Convert.FromBase64String(tempKey);
        }

        private static byte[] CreateDecryptionKey(byte[] tempKey, byte[] appSecret)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = appSecret;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                using (var dec = aes.CreateDecryptor())
                {
                    var res = dec.TransformFinalBlock(tempKey, 0, tempKey.Length);
                    var b64Str = Encoding.UTF8.GetString(res);
                    b64Str = b64Str.TrimEnd('\0');
                    return Convert.FromBase64String(b64Str); ;
                }
            }
        }

        private static string DecryptContainer(byte[] containerContent, byte[] key)
        {
            byte[] decContainer;
            try
            {
                decContainer = DecryptWithPadding(key, PaddingMode.PKCS7, containerContent);
            }
            catch (CryptographicException)
            {
                decContainer = DecryptWithPadding(key, PaddingMode.None, containerContent);
            }
            var str = Encoding.UTF8.GetString(decContainer).TrimEnd('\0');

            return DlcFormat.DecodeDataString(str);
        }

        private static byte[] DecryptWithPadding(byte[] ivPlusKey, PaddingMode paddingMode, byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = aes.IV = ivPlusKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = paddingMode;

            using var dec = aes.CreateDecryptor();
            return dec.TransformFinalBlock(data, 0, data.Length);
        }

        private static XDocument DecodeXmlData(string encodedXml)
        {
            Debug.Assert(encodedXml != null);
            var doc = XDocument.Parse(encodedXml);
            DecodeXmlDataForElement(doc.Element("dlc"));
            return doc;
        }

        private static void DecodeXmlDataForElement(XElement element)
        {
            if (element == null)
                return;

            foreach (var attr in element.Attributes())
                attr.Value = DlcFormat.DecodeDataString(attr.Value);

            foreach (var sub in element.Elements())
            {
                if (!sub.HasElements && !sub.IsEmpty)
                    sub.Value = DlcFormat.DecodeDataString(sub.Value);
                DecodeXmlDataForElement(sub);
            }
        }

        public override void SaveToStream(Stream stream) => throw new NotSupportedException();
        public override Task SaveToStreamAsync(Stream stream) => throw new NotSupportedException();
    }
}
