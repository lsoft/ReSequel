using System;
using System.Data.SqlClient;

namespace Main.Sql.SqlServer.Validator.Factory
{
    public class DetectSqlValidatorFactory :  ISqlValidatorFactory
    {
        private readonly DescribeSqlValidatorFactory _describeSqlValidatorFactory;
        private readonly FmtOnlySqlValidatorFactory _fmtOnlySqlValidatorFactory;

        public DetectSqlValidatorFactory(
            DescribeSqlValidatorFactory describeSqlValidatorFactory,
            FmtOnlySqlValidatorFactory fmtOnlySqlValidatorFactory
            )
        {
            if (describeSqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(describeSqlValidatorFactory));
            }

            if (fmtOnlySqlValidatorFactory == null)
            {
                throw new ArgumentNullException(nameof(fmtOnlySqlValidatorFactory));
            }

            _describeSqlValidatorFactory = describeSqlValidatorFactory;
            _fmtOnlySqlValidatorFactory = fmtOnlySqlValidatorFactory;
        }


        public ISqlValidator Create(SqlConnection connection)
        {
            ISqlValidator result;

            var version = new Version(connection.ServerVersion);
            if (version.Major >= 12)
            {
                //only SQL SERVER 2012 (or above) supports this
                result = _describeSqlValidatorFactory.Create(connection);
            }
            else
            {
                result = _fmtOnlySqlValidatorFactory.Create(connection);
            }

            return result;
        }
    }

}
