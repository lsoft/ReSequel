using System.Xml.Serialization;

namespace Extension.ConfigurationRelated
{
    public partial class ConfigurationSolutions
    {
        [XmlElement]
        public string[] Solution
        {
            get;
            set;
        }

    }
}
