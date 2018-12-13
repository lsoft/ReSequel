using System;
using System.Data.SqlClient;

namespace Tests
{
    public static class SqlHelper
    {
        public static void ExecuteBatch(
            this SqlConnection connection,
            string batchesBody
            )
        {
            var batches = batchesBody.Split(new string[] { Environment.NewLine + "GO" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var batch in batches)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = batch;
                    command.ExecuteNonQuery();
                }
            }

        }

    }
}
