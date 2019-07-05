using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProjectGeneratorMethod
    {
        [XmlAttribute]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute]
        public bool ContainsSql
        {
            get;
            set;
        }

        [XmlAttribute]
        public bool ContainsOptions
        {
            get;
            set;
        }
    }
}