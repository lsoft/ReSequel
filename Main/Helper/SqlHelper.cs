using System;
using System.Globalization;

namespace Main.Helper
{
    public static class SqlHelper
    {
        private static string StartGo = "GO" + Environment.NewLine;
        private static string MiddleGo = Environment.NewLine +  "GO" + Environment.NewLine;
        private static string EndGo = Environment.NewLine + "GO";

        public static string[] SplitBatch(
            this string sqlBatch
            )
        {
            while (sqlBatch.StartsWith(StartGo, true, CultureInfo.InvariantCulture))
            {
                sqlBatch = sqlBatch.Substring(StartGo.Length);
            }

            while (sqlBatch.EndsWith(EndGo, true, CultureInfo.InvariantCulture))
            {
                sqlBatch = sqlBatch.Substring(0, sqlBatch.Length - EndGo.Length);
            }

            var batches = sqlBatch.Split(new string[] { MiddleGo }, StringSplitOptions.RemoveEmptyEntries);

            return
                batches;
        }
    }
}
