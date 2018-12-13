using System;

namespace Main.Logger
{
    public class DebugProcessLogger : IProcessLogger
    {
        private readonly object _locker = new object();

        public DebugProcessLogger()
        {

        }

        public void ShowProcessMessage(
            string message,
            params object[] args
            )
        {
            this.ShowProcessMessage(
                string.Format(message, args)
                );
        }

        public void ShowProcessMessage(
            string message
            )
        {
            message = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff") + "    " + message + "... ";

            Console.WriteLine(message);
        }


    }



}
