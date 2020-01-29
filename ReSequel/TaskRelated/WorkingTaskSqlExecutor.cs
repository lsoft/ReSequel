using System.Xml.Serialization;

namespace ReSequel.TaskRelated
{
    public sealed class WorkingTaskSqlExecutor
    {
        public string Type
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }
    }
}
