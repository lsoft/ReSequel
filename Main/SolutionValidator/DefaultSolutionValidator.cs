using System;
using Main.WorkspaceWrapper;
using System.Collections.Generic;
using Main.Inclusion.Scanner;
using Main.Validator;
using Main.Logger;
using Main.Progress;
using Main.Inclusion.Found;
using Main.Inclusion.Validated;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.CodeAnalysis;

namespace Main.SolutionValidator
{
    public sealed class DefaultSolutionValidator : ISolutionValidator
    {
        private readonly IWorkspaceFactory _workspaceFactory;
        private readonly IInclusionScannerFactory _scannerFactory;
        private readonly IValidatorFactory _validationFactory;
        private readonly IProcessLogger _logger;

        public ValidationProgress Progress
        {
            get;
        }

        public DefaultSolutionValidator(
            IWorkspaceFactory workspaceFactory,
            IInclusionScannerFactory scannerFactory,
            IValidatorFactory validationFactory,
            ValidationProgress progress,
            IProcessLogger logger
            )
        {
            if (workspaceFactory == null)
            {
                throw new ArgumentNullException(nameof(workspaceFactory));
            }

            if (scannerFactory == null)
            {
                throw new ArgumentNullException(nameof(scannerFactory));
            }

            if (validationFactory == null)
            {
                throw new ArgumentNullException(nameof(validationFactory));
            }

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _workspaceFactory = workspaceFactory;
            _scannerFactory = scannerFactory;
            _validationFactory = validationFactory;
            Progress = progress;
            _logger = logger;
        }

        public async Task<List<IValidatedSqlInclusion>> ExecuteAsync(
            Workspace subjectWorkspace
            )
        {
            if (subjectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(subjectWorkspace));
            }

            Progress.Start();

            _logger.ShowProcessMessage("Scanning for SQL inclusions");

            try
            {
                var scanner = _scannerFactory.Create();

                var foundInclusionList = await scanner.ScanAsync(
                    subjectWorkspace,
                    _logger
                    )/*.ConfigureAwait(false)*/;

                var validationInclusionList = foundInclusionList.ConvertAll(j => (IValidatedSqlInclusion) new ValidatedSqlInclusion(j))
                    ;

                Progress.SetInclusionCount(validationInclusionList.Count);

                var validator = _validationFactory.Create(Progress);

                await validator.ValidateAsync(
                    validationInclusionList,
                    () => false
                    );

                Progress.Finish();

                return
                    validationInclusionList;
            }
            catch (Exception excp)
            {
                throw;
            }
        }
    }

}
