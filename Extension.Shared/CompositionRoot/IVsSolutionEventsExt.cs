using Microsoft.VisualStudio.Shell.Interop;

namespace Extension.CompositionRoot
{
    public interface IVsSolutionEventsExt : IVsSolutionEvents
    {
        uint Cookie
        {
            get;
            set;
        }
    }
}
