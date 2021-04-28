namespace Main.Logger
{
    public interface IProcessLogger
    {
        void ShowProcessMessage(
            string message,
            params object[] args
            );

        void ShowProcessMessage(
            string message
            );
    }
}
