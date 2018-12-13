namespace Main.Logger
{
    public class NullProcessLogger : IProcessLogger
    {
        public void ShowProcessMessage(string message, params object[] args)
        {
            //nothing to do
        }

        public void ShowProcessMessage(string message)
        {
            //nothing to do
        }

        public static NullProcessLogger Instance = new NullProcessLogger();
    }
}
