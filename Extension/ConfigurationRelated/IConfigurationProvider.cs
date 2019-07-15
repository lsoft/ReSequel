namespace Extension.ConfigurationRelated
{
    public interface IConfigurationProvider
    {
        bool TryRead(
            out Configuration configuration
        );

        void Save(
            Configuration configuration
        );

        event ConfigurationFileChangedDelegate ConfigurationFileChangedEvent;
    }
}