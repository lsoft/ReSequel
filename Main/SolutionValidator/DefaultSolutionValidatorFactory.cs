using System;
using Main.WorkspaceWrapper;
using Main.Inclusion.Scanner;
using Main.Validator;
using Main.Logger;
using Main.Progress;

namespace Main.SolutionValidator
{
    public class DefaultSolutionValidatorFactory : ISolutionValidatorFactory
    {
        private readonly IWorkspaceFactory _workspaceFactory;
        private readonly IInclusionScannerFactory _scannerFactory;
        private readonly IValidatorFactory _validationFactory;
        private readonly ValidationProgressFactory _progressFactory;
        private readonly IProcessLogger _logger;


        public DefaultSolutionValidatorFactory(
            IWorkspaceFactory workspaceFactory,
            IInclusionScannerFactory scannerFactory,
            IValidatorFactory validationFactory,
            ValidationProgressFactory progressFactory,
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

            if (progressFactory == null)
            {
                throw new ArgumentNullException(nameof(progressFactory));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _workspaceFactory = workspaceFactory;
            _scannerFactory = scannerFactory;
            _validationFactory = validationFactory;
            _progressFactory = progressFactory;
            _logger = logger;
        }

        public ISolutionValidator Create()
        {
            var result = new DefaultSolutionValidator(
                _workspaceFactory,
                _scannerFactory,
                _validationFactory,
                _progressFactory.Create(),
                _logger
                );

            return
                result;
        }
    }

}
