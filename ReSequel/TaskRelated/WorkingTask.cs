using System.Xml.Serialization;

namespace ReSequel.TaskRelated
{
    [XmlRoot(ElementName = "Task")]
    public sealed class WorkingTask
    {
        public string ScanScheme
        {
            get;
            set;
        }

        public string TargetSolution
        {
            get;
            set;
        }

        public WorkingTaskSqlExecutor SqlExecutor
        {
            get;
            set;
        }
    }
}
