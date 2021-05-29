using System;
using System.Data.Common;

namespace SqlServerValidator.UndeclaredDeterminer
{
    public class UndeclaredParameterDeterminerFactory : IUndeclaredParameterDeterminerFactory
    {
        public IUndeclaredParameterDeterminer Create(string connectionString)
        {
            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var dd = new DescribeUndeclaredParameterDeterminer(
                connectionString
                );
            var sqlxmld = new SqlXmlUndeclaredParameterDeterminer(
                connectionString
                );
            var ad = new AdapterUndeclaredParameterDeterminer(
                dd,
                sqlxmld
                );

            return ad;
        }

        public IUndeclaredParameterDeterminer Create(DbConnection connection)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var dd = new DescribeUndeclaredParameterDeterminer(
                connection
                );
            var sqlxmld = new SqlXmlUndeclaredParameterDeterminer(
                connection
                );
            var ad = new AdapterUndeclaredParameterDeterminer(
                dd,
                sqlxmld
                );

            return ad;
        }
    }
}
