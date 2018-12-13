namespace Extension.ExtensionStatus
{
    public interface IExtensionStatus
    {
        bool IsSolutionExists
        {
            get;
        }

        string SolutionName
        {
            get;
        }

        bool IsEnabled
        {
            get;
        }
    }

}
