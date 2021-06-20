using System.Xml.Serialization;

namespace Extension.ConfigurationRelated
{
    public partial class ConfigurationSqlExecutors
    {
        [XmlElement]
        public ConfigurationSqlExecutorsSqlExecutor[] SqlExecutor
        {
            get;
            set;
        }

    }
}
