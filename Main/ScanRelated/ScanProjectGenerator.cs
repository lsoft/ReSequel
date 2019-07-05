using System;
using System.Linq;
using System.Xml.Serialization;

namespace Main.ScanRelated
{
    public sealed class ScanProjectGenerator
    {
        [XmlElement(ElementName = "Method")]
        public ScanProjectGeneratorMethod[] Methods
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

        internal bool IsItSqlBodyMethod(string methodName)
        {
            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return
                Methods.Any(j => j.ContainsSql && j.Name == methodName);
        }

        internal bool IsItOptionMethod(string methodName)
        {
            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return
                Methods.Any(j => j.ContainsOptions && j.Name == methodName);
        }
    }
}