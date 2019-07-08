using Main.Sql;
using Main.Progress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Main.Inclusion.Validated;
using Main.Validator.UnitProvider;

namespace Main.Validator
{
    public class ComplexValidator : IValidator
    {
        private readonly ValidationProgress _status;
        private readonly ISqlExecutorFactory _executorFactory;

        public ComplexValidator(
            ValidationProgress status,
            ISqlExecutorFactory executorFactory
            )
        {
            if (status == null)
            {
                throw new ArgumentNullException(nameof(status));
            }

            if (executorFactory == null)
            {
                throw new ArgumentNullException(nameof(executorFactory));
            }

            _status = status;
            _executorFactory = executorFactory;
        }

        /// <summary>
        /// No order guarantee!
        /// </summary>
        public void Validate(
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

            var unitProvider = new UnitProvider.UnitProvider(
                inclusions,
                shouldBreak
                );

            if (unitProvider.TotalVariantCount < 20) //hand made constant
            {
                ProcessSequentially(unitProvider);
            }
            else
            {
                ProcessInParallel(unitProvider);
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

        private void ProcessSequentially(IUnitProvider unitProvider)
        {
            using (var executor = _executorFactory.Create())
            {
                executor.Execute(unitProvider);
            }
        }


        private void ProcessInParallel(IUnitProvider unitProvider)
        {
            var executorCreated = 0;
            var executorDisposed = 0;
            var unitProcessedCount = 0;

            var before = DateTime.Now;

            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;

            Parallel.ForEach(
                unitProvider.RequestNextUnitSync(),
                options,
                () =>
                {
                    Debug.WriteLine("SqlExecutor works in thread {0}", Thread.CurrentThread.ManagedThreadId);
                    Interlocked.Increment(ref executorCreated);

                    var executor = _executorFactory.Create();
                    return executor;
                },
                (unit, state, executor) =>
                {
                    Interlocked.Increment(ref unitProcessedCount);
                    executor.Execute(unit);
                    return executor;
                },
                executor =>
                {
                    Debug.WriteLine("SqlExecutor (thread {0}) processed {1} variants", Thread.CurrentThread.ManagedThreadId, executor.ProcessedUnits);
                    Interlocked.Increment(ref executorDisposed);

                    executor.Dispose();
                });

            var after = DateTime.Now;
            Debug.WriteLine("Inclusion validation checks {0} variants, takes {1}", unitProvider.TotalVariantCount, (after - before));
            Debug.WriteLine("Executors created {0}, disposed {1}", executorCreated, executorDisposed);
        }
    }
}
