namespace Extension.ExtensionStatus.FullyLoaded
{
    public interface IFullyLoadedStatusProvider
    {
        bool IsSolutionFullyLoaded
        {
            get;
        }
    }
}
