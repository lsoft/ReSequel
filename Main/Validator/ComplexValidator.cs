
using Main.Helper;
using Main.Inclusion;
using Main.Sql;
using Main.Inclusion.Validated.Result;
using Main.Progress;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Inclusion.Validated;

namespace Main.Validator
{
    public class ComplexValidator : IValidator
    {
        private readonly ValidationProgress _status;
        private readonly ISqlExecutorFactory _executorFactory;
        private readonly int _batchSize;

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
            _batchSize = 0;
        }


        public ComplexValidator(
            ValidationProgress status,
            ISqlExecutorFactory executorFactory,
            int batchSize
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
            _batchSize = batchSize;
        }

        /// <summary>
        /// No order guarrantee!
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

            if (_batchSize == 0 || inclusions.Count < _batchSize)
            {
                //sequence verify
                DoValidation(_status, inclusions, shouldBreak);
            }
            else
            {
                //parallel batch verify
                Parallel.ForEach(inclusions.Split(_batchSize), inclusionBatch =>
                {
                    DoValidation(_status, inclusionBatch, shouldBreak);
                });
            }
        }

        private void DoValidation(
            ValidationProgress status,
            List<IValidatedSqlInclusion> inclusionBatch,
            Func<bool> shouldBreak
            )
        {
            var executor = _executorFactory.Create();

            foreach (var inclusion in inclusionBatch)
            {
                try
                {
                    IComplexValidationResult successExecuteResult = null;
                    foreach (var executeResult in executor.Execute(inclusion.Inclusion))
                    {
                        if (!executeResult.IsSuccess)
                        {
                            inclusion.SetResult(
                                executeResult
                            );

                            break;
                        }

                        successExecuteResult = executeResult;

                        if (shouldBreak())
                        {
                            //we should stop prematurely

                            successExecuteResult = null;

                            var cvr = new ComplexValidationResult();
                            cvr.Append(ValidationResult.Error(inclusion.Inclusion.SqlBody, inclusion.Inclusion.SqlBody, "Process stopped prematurely"));

                            inclusion.SetResult(
                                cvr
                                );
                            break;
                        }
                    }

                    if (successExecuteResult != null)
                    {
                        inclusion.SetResult(
                            successExecuteResult
                        );
                    }
                }
                catch (Exception excp)
                {
                    inclusion.SetResult(
                        new ExceptionValidationResult(
                            inclusion.Inclusion.SqlBody,
                            excp
                            )
                        );
                }

                status.AddProcessedInclusionCount(1);
            }
        }



    }
}
