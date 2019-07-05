using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProjectContainer
    {
        [XmlElement(ElementName = "Property")]
        public ScanProjectContainerProperty[] Properties
        {
            get;
            set;
        }

        [XmlElement(ElementName = "Method")]
        public ScanProjectContainerMethod[] Methods
        {
            get;
            set;
        }

        [XmlAttribute]
        public string Name
        {
            get;
            set;
        }
    }
}
