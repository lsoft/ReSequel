using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TestConsole.TaskRelated
{
    public sealed class WorkingTaskSqlExecutor
    {
        [XmlElement]
        public string Type
        {
            get;
            set;
        }

        [XmlElement]
        public string ConnectionString
        {
            get;
            set;
        }
    }
}
