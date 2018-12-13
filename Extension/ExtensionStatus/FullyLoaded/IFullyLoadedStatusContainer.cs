namespace Extension.ExtensionStatus.FullyLoaded
{
    public interface IFullyLoadedStatusContainer : IFullyLoadedStatusProvider
    {
        void AsyncStart();

        void SyncStop();
    }
}
