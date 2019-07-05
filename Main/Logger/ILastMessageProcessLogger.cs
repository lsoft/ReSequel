namespace Main.Logger
{
    public interface ILastMessageProcessLogger : IProcessLogger
    {
        string LastMessage
        {
            get;
        }

        event NewProcessLoggerMessageDelegate NewProcessLoggerMessageEvent;

        void ResetCounter();
    }
}