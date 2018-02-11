using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace OpenDLC
{
    // TODO: Make all this internal since it's only used for serialization

    [XmlRoot(ElementName = "Options", Namespace = "")]
    [DataContract(Name = "Options", Namespace = "")]
    public class CcfOptions
    {
        [XmlElement(ElementName = "Kommentar", Namespace = "")]
        [DataMember]
        public string Kommentar { get; set; }
        [XmlElement(ElementName = "Passwort", Namespace = "")]
        [DataMember]
        public string Passwort { get; set; }
    }

    [XmlRoot(ElementName = "Download", Namespace = "")]
    [DataContract(Name = "Download", Namespace = "")]
    public class CcfDownload
    {
        [XmlElement(ElementName = "FileSize", Namespace = "")]
        [DataMember]
        public string FileSize { get; set; }

        [XmlElement(ElementName = "Url", Namespace = "")]
        [DataMember]
        public string Url { get; set; }

        [XmlAttribute(AttributeName = "Url", Namespace = "")]
        public string UrlAttribute { get; set; }

        [XmlElement(ElementName = "FileName", Namespace = "")]
        [DataMember]
        public string FileName { get; set; }

        internal static CcfDownload FromCcfEntry(CcfEntry entry) => new CcfDownload
        {
            Url = entry.Url,
            UrlAttribute = entry.Url,
            FileName = entry.FileName,
            FileSize = entry.FileSize.ToString()
        };
    }

    [XmlRoot(ElementName = "Package", Namespace = "")]
    [DataContract(Name = "Package", Namespace = "")]
    public class CcfPackageItem
    {
        [XmlElement(ElementName = "Options", Namespace = "")]
        public CcfOptions Options { get; set; }

        [XmlElement(ElementName = "Download", Namespace = "")]
        public List<CcfDownload> Downloads { get; set; }

        [XmlAttribute(AttributeName = "service", Namespace = "")]
        public string Service { get; set; }

        [XmlAttribute(AttributeName = "name", Namespace = "")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "url", Namespace = "")]
        public string Url { get; set; }

        internal static CcfPackageItem FromCcfPackage(CcfPackage package)
        {
            Debug.Assert(package != null);

            var res = new CcfPackageItem
            {
                Name = package.Name,
                Options = new CcfOptions
                {
                    Kommentar = package.Comment,
                    Passwort = package.Password
                },
                Service = package.Service,
                Url = package.Url
            };

            res.Downloads = new List<CcfDownload>();
            for (int i = 0; i < package.Count; ++i)
                res.Downloads.Add(CcfDownload.FromCcfEntry(package[i]));

            return res;
        }
    }

    [XmlRoot(ElementName = "CryptLoad", Namespace = "")]
    [DataContract(Name = "CryptLoad", Namespace = "")]
    public class CryptLoadContainer
    {
        [XmlElement(ElementName = "Package", Namespace = "")]
        [DataMember]
        public List<CcfPackageItem> Packages { get; set; }

        internal static CryptLoadContainer FromCcfContainer(CcfContainer container)
        {
            Debug.Assert(container != null);

            var c = new CryptLoadContainer();
            c.Packages = new List<CcfPackageItem>();
            for (int i = 0; i < container.Count; ++i)
                c.Packages.Add(CcfPackageItem.FromCcfPackage(container[i]));
            return c;
        }
    }
}
