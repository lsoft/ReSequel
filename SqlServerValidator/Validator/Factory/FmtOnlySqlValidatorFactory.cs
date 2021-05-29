using System;
using System.Data.Common;
using Main.Sql;
using SqlServerValidator.UndeclaredDeterminer;

namespace SqlServerValidator.Validator.Factory
{
    public class FmtOnlySqlValidatorFactory :  ISqlValidatorFactory
    {
        private readonly IUndeclaredParameterDeterminerFactory _undeclaredParameterDeterminerFactory;

        public FmtOnlySqlValidatorFactory(
            IUndeclaredParameterDeterminerFactory undeclaredParameterDeterminerFactory
            )
        {
            if (undeclaredParameterDeterminerFactory is null)
            {
                throw new ArgumentNullException(nameof(undeclaredParameterDeterminerFactory));
            }
            _undeclaredParameterDeterminerFactory = undeclaredParameterDeterminerFactory;
        }

        public ISqlValidator Create(DbConnection connection)
        {
            return
                new FmtOnlySqlValidator(
                    _undeclaredParameterDeterminerFactory,
                    connection
                    );
        }
    }

}
