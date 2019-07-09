using System;
using Main.WorkspaceWrapper;
using System.Collections.Generic;
using Main.Inclusion.Scanner;
using Main.Validator;
using Main.Logger;
using Main.Progress;
using Main.Inclusion;
using Main.Other;
using Main.Inclusion.Found;
using Main.Inclusion.Validated;
using System.Threading.Tasks;

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

        public List<IValidatedSqlInclusion> Execute(
            string pathToSubjectSolution
            )
        {
            if (pathToSubjectSolution == null)
            {
                throw new ArgumentNullException(nameof(pathToSubjectSolution));
            }

            Progress.Start();

            _logger.ShowProcessMessage("Scanning for SQL inclusions");

            try
            {
                List<IFoundSqlInclusion> foundInclusionList;
                using (IWorkspaceWrapper subjectWorkspace = _workspaceFactory.Open(pathToSubjectSolution))
                {
                    //no need this:
                    //subjectWorkspace.Compile(false);
                    //this solution will be complied document by document

                    var scanner = _scannerFactory.Create();

                    foundInclusionList = scanner.Scan(subjectWorkspace,
                        _logger);
                }

                var validationInclusionList = foundInclusionList.ConvertAll(j => (IValidatedSqlInclusion) new ValidatedSqlInclusion(j))
                    ;

                Progress.SetInclusionCount(validationInclusionList.Count);

                var validator = _validationFactory.Create(Progress);

                validator.Validate(validationInclusionList,
                    () => false);

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
