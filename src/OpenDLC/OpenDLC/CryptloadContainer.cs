using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace OpenDLC
{
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
    }

    [XmlRoot(ElementName = "Package", Namespace = "")]
    [DataContract(Name = "Package", Namespace = "")]
    public class CcfPackage
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
    }

    [XmlRoot(ElementName = "CryptLoad", Namespace = "")]
    [DataContract(Name = "CryptLoad", Namespace = "")]
    public class CryptLoadContainer
    {
        [XmlElement(ElementName = "Package", Namespace = "")]
        [DataMember]
        public CcfPackage Package { get; set; }
    }
}
