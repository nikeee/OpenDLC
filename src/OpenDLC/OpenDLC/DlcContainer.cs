using System;
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
        private const string JdServiceUrl = "http://service.jdownloader.org/dlcrypt/service.php";
        private const string RateLimitExceededKey = "2YVhzRFdjR2dDQy9JL25aVXFjQ1RPZ";

        public DlcGenerator Generator { get; set; }
        public DlcTribute Tribute { get; set; }
        public string XmlVersion { get; set; }

        public override void SaveToStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public static Task<DlcContainer> FromFileAsync(string fileName, string appId, string revision, byte[] appSecret)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName"); // TODO: nameof(fileName)

            var str = File.ReadAllText(fileName);
            return FromStringAsync(str, appId, revision, appSecret);
        }
        private static async Task<DlcContainer> FromStringAsync(string fileContent, string appId, string revision, byte[] appSecret)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentException("Invalid file contents");

            var key = fileContent.Substring(fileContent.Length - 88);
            fileContent = fileContent.Remove(fileContent.Length - 88);

            var fileContentBuffer = Convert.FromBase64String((fileContent ?? string.Empty).Trim());
            var keyBuffer = Convert.FromBase64String((key ?? string.Empty).Trim());

            var tempKey = await CallJDService(appId, revision, key);

            var decryptionKey = CreateDecryptionKey(tempKey, appSecret);

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
                        var app = generator.Element("app");
                        var url = generator.Element("url");

                        var generatorObj = new DlcGenerator
                        {
                            Application = app == null ? null : app.Value,
                            Url = url == null ? null : url.Value
                        };

                        var version = generator.Element("version");
                        if (version != null)
                        {
                            Version ver;
                            if (Version.TryParse(version.Value, out ver))
                                generatorObj.Version = ver;
                        }

                        container.Generator = generatorObj;
                    }


                    var tribute = header.Element("tribute");
                    if (tribute != null)
                    {
                        var name = tribute.Element("name");

                        container.Tribute = new DlcTribute
                        {
                            Name = name == null ? null : name.Value
                        };
                    }

                    var dlcxmlversion = header.Element("dlcxmlversion");
                    container.XmlVersion = dlcxmlversion == null ? null : dlcxmlversion.Value;
                }

                var content = dlc.Element("content");
                if (content != null)
                {
                    var packages = content.Elements("package");
                    foreach (var p in packages)
                    {
                        if (p == null)
                            continue;

                        var name = p.Attribute("name");
                        var comment = p.Attribute("comment");
                        var category = p.Attribute("category");

                        var packageObj = new DlcPackage
                        {
                            Name = name == null ? null : name.Value,
                            Comment = comment == null ? null : comment.Value,
                            Category = category == null ? null : category.Value
                        };

                        var files = p.Elements("file");
                        foreach (var f in files)
                        {
                            if (f == null)
                                continue;

                            var url = f.Element("url");
                            if (url == null)
                                continue;

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
            var ub = new UriBuilder(JdServiceUrl);
            var query = HttpUtility.ParseQueryString(ub.Query);
            query["destType"] = "jdtc6";
            query["b"] = appId;
            query["srcType"] = "dlc";
            query["data"] = data;
            query["v"] = revision;
            ub.Query = query.ToString();

            var msg = new HttpRequestMessage(HttpMethod.Post, ub.Uri);

            using (var cl = new HttpClient())
            using (var res = await cl.SendAsync(msg))
            {
                if (!res.IsSuccessStatusCode)
                    throw new DlcDecryptionException(); // TODO

                var resContent = await res.Content.ReadAsStringAsync();

                var match = Regex.Match(resContent, "<rc>(.*)</rc>");
                if (!match.Success)
                    throw new DlcDecryptionException(); // TODO

                var tempKey = match.Groups[1].Value ?? string.Empty;
                tempKey = tempKey.Trim();
                if (tempKey == string.Empty)
                    throw new DlcDecryptionException(); // TODO

                if (tempKey == RateLimitExceededKey)
                    throw new DlcLimitExceededException();

                return Convert.FromBase64String(tempKey);
            }
        }

        private static byte[] CreateDecryptionKey(byte[] tempKey, byte[] appSecret)
        {
            using (var aes = new AesManaged())
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
            byte[] decContainer = null;
            try
            {
                decContainer = DecryptWithPadding(key, PaddingMode.PKCS7, containerContent);
            }
            catch (CryptographicException)
            {
                decContainer = DecryptWithPadding(key, PaddingMode.None, containerContent);
            }
            var str = Encoding.UTF8.GetString(decContainer) ?? string.Empty;
            str = str.TrimEnd('\0');

            var pln = Encoding.UTF8.GetString(Convert.FromBase64String(str));
            return pln;
        }

        private static byte[] DecryptWithPadding(byte[] ivPlusKey, PaddingMode paddingMode, byte[] data)
        {
            using (var aes = new AesManaged())
            {
                aes.Key = aes.IV = ivPlusKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = paddingMode;

                using (var dec = aes.CreateDecryptor())
                    return dec.TransformFinalBlock(data, 0, data.Length);
            }
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
            {
                attr.Value = DecodeDataString(attr.Value);
            }
            foreach (var sub in element.Elements())
            {
                if (!sub.HasElements && !sub.IsEmpty)
                    sub.Value = DecodeDataString(sub.Value);
                DecodeXmlDataForElement(sub);
            }
        }

        private static string DecodeDataString(string encodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }

        public override Task SaveToStreamAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
