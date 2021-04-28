using System.Data.Common;
using Main.Helper;

namespace Tests
{
    public static class SqlHelper
    {
        public static void ExecuteBatch(
            this DbConnection connection,
            string batchesBody
            )
        {
            var batches = batchesBody.SplitBatch();
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
