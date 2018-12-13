using System.Xml.Serialization;

namespace TestConsole.TaskRelated
{
    [XmlRoot(ElementName = "Task")]
    public sealed class WorkingTask
    {
        [XmlElement]
        public string ScanScheme
        {
            get;
            set;
        }

        [XmlElement]
        public string TargetSolution
        {
            get;
            set;
        }

        //[XmlElementAttribute("SqlExecutor")]
        public WorkingTaskSqlExecutor SqlExecutor
        {
            get;
            set;
        }
    }

    //public sealed class TaskContainer
    //{
    //    [XmlElementAttribute("Task")]
    //    public Task[] Tasks
    //    {
    //        get;
    //        set;
    //    }
    //}
}
