using System;
using System.Text;

namespace Main.Helper
{
    public static class ExceptionHelper
    {
        public static string AggregateMessages(this Exception excp)
        {
            var sb = new StringBuilder();

            AggregateMessages(excp, 0, sb);

            return
                sb.ToString();
        }

        private static void AggregateMessages(Exception excp, int prefix, StringBuilder sb)
        {
            sb.AppendLine(new string(' ', prefix) + excp.Message);

            if (excp.InnerException != null)
            {
                AggregateMessages(excp.InnerException, prefix + 2, sb);
            }

            if (excp is AggregateException)
            {
                foreach (var ie in (excp as AggregateException).InnerExceptions)
                {
                    if (ie != null)
                    {
                        AggregateMessages(ie, prefix + 2, sb);
                    }
                }
            }
        }
    }
}
