using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProjectContainerProperty
    {
        [XmlAttribute]
        public string Name
        {
            get;
            set;
        }
    }
}
