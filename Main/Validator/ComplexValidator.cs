using Main.Sql;
using Main.Progress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Main.Inclusion.Validated;
using Main.Inclusion.Validated.Result;
using Main.Validator.UnitProvider;
using Main.Helper;

namespace Main.Validator
{
    public class ComplexValidator : IValidator
    {
        private readonly ValidationProgress _progress;
        private readonly ISqlExecutorFactory _executorFactory;

        public ComplexValidator(
            ValidationProgress progress,
            ISqlExecutorFactory executorFactory
            )
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (executorFactory == null)
            {
                throw new ArgumentNullException(nameof(executorFactory));
            }

            _progress = progress;
            _executorFactory = executorFactory;
        }

        /// <summary>
        /// No order guarantee!
        /// </summary>
        public async Task ValidateAsync(
            List<IValidatedSqlInclusion> inclusions,
            Func<bool> shouldBreak
            )
        {
            if (inclusions == null)
            {
                throw new ArgumentNullException(nameof(inclusions));
            }

            if (inclusions.Count <= 0)
            {
                return;
            }

            //check for sql validator can connect to RDBMS
            var (checkResult, errorMessage) = await _executorFactory.CheckForConnectionExistsAsync();
            if (!checkResult)
            {
                foreach (var inclusion in inclusions)
                {
                    inclusion.SetStatusProcessed(ValidationResult.Error(inclusion.Inclusion.SqlBody, inclusion.Inclusion.SqlBody, errorMessage));
                }

                return;
            }

            var unitProvider = new UnitProvider.UnitProvider(
                inclusions,
                shouldBreak,
                _progress
                );

            if (unitProvider.TotalVariantCount < 20) //hand made constant
            {
                await ProcessSequentiallyAsync(unitProvider);
            }
            else
            {
                await ProcessInParallelAsync(unitProvider);
            }

            var prematurelyStopped = shouldBreak();

            foreach (var inclusion in inclusions)
            {
                if (!unitProvider.Artifacts.TryGetValue(inclusion, out var bag))
                {
                    throw new InvalidOperationException("Bag must exists, unknown problem");
                }

                //bag is exists, fix the results
                bag.FixResultIntoInclusion(prematurelyStopped);
            }
        }

        private async Task ProcessSequentiallyAsync(IUnitProvider unitProvider)
        {
            using (var executor = _executorFactory.Create())
            {
                await executor.ExecuteAsync(unitProvider);
            }
        }


        private async Task ProcessInParallelAsync(IUnitProvider unitProvider)
        {
            await unitProvider.RequestNextUnitSync()
                .ParallelForEachAsync(
                unit =>
                {
                    var executor = _executorFactory.Create();
                    return executor.ExecuteAsync(unit);
                },
                Environment.ProcessorCount
                );
        }
    }
}
