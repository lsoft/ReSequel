using Main.Sql;
using Main.Progress;

using System;
using System.Linq;
using Main.Sql.ConnectionString;

namespace Main.Validator
{
    public class ComplexValidatorFactory : IValidatorFactory
    {
        private readonly ISqlExecutorFactory _executorFactory;

        public ComplexValidatorFactory(
            ISqlExecutorFactory executorFactory
            )
        {
            if (executorFactory == null)
            {
                throw new ArgumentNullException(nameof(executorFactory));
            }

            _executorFactory = executorFactory;
        }

        public IValidator Create(
            ValidationProgress progress
            )
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            return
                new ComplexValidator(progress, _executorFactory);
        }
    }
}
