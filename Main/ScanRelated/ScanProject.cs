using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProject
    {
        [XmlElement(ElementName = "Container")]
        public ScanProjectContainer[] Containers
        {
            get;
            set;
        }

        [XmlElement(ElementName = "Generator")]
        public ScanProjectGenerator[] Generators
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
