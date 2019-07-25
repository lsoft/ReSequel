using Main.Inclusion.Scanner;

namespace Extension.ExtensionStatus
{
    public interface IExtensionStatus : ISolutionNameProvider
    {
        bool IsSolutionExists
        {
            get;
        }

        bool IsEnabled
        {
            get;
        }
    }

}
