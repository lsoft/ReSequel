using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProjectContainerMethod
    {
        [XmlElement(ElementName = "Argument")]
        public ScanProjectContainerMethodArgument[] Arguments
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
