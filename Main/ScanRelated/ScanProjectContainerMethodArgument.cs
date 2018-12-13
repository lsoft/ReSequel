using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProjectContainerMethodArgument
    {
        [XmlAttribute]
        public string Type
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
    }
}
