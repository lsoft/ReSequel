using System;

namespace Main.Logger
{
    public class ConsoleProcessLogger : IProcessLogger
    {
        private readonly object _locker = new object();

        public ConsoleProcessLogger()
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
            var cwidth = Console.WindowWidth;
            var ccwidth = cwidth - 4;

            if (message.Length > ccwidth)
            {
                message = message.Substring(0, ccwidth);
            }

            message = message + "... ";

            lock (_locker)
            {
                Console.Write(new string(' ', ccwidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(message);
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }


    }



}
