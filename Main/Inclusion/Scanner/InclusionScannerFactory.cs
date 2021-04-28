using System;
using Main.ScanRelated;

namespace Main.Inclusion.Scanner
{
    public sealed class InclusionScannerFactory : IInclusionScannerFactory
    {
        private readonly ISolutionNameProvider _solutionNameProvider;
        private readonly IScanProvider _scanProvider;

        public InclusionScannerFactory(
            ISolutionNameProvider solutionNameProvider,
            IScanProvider scanProvider
            )
        {
            if (solutionNameProvider == null)
            {
                throw new ArgumentNullException(nameof(solutionNameProvider));
            }

            if (scanProvider == null)
            {
                throw new ArgumentNullException(nameof(scanProvider));
            }

            _solutionNameProvider = solutionNameProvider;
            _scanProvider = scanProvider;
        }

        public IInclusionScanner Create()
        {
            var scan = _scanProvider.CreateScan();

            var result = new InclusionScanner(
                _solutionNameProvider,
                scan
                );

            return
                result;
        }
    }

}
