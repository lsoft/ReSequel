using System;
using Main.ScanRelated;

namespace Main.Inclusion.Scanner
{
    public sealed class InclusionScannerFactory : IInclusionScannerFactory
    {
        private readonly ISolutionNameProvider _solutionNameProvider;
        private readonly Func<Scan> _scanFunc;

        public InclusionScannerFactory(
            ISolutionNameProvider solutionNameProvider,
            Func<Scan> scanFunc
            )
        {
            if (solutionNameProvider == null)
            {
                throw new ArgumentNullException(nameof(solutionNameProvider));
            }

            if (scanFunc == null)
            {
                throw new ArgumentNullException(nameof(scanFunc));
            }

            _solutionNameProvider = solutionNameProvider;
            _scanFunc = scanFunc;
        }

        public IInclusionScanner Create()
        {
            var scan = _scanFunc();

            var result = new InclusionScanner(
                _solutionNameProvider,
                scan
                );

            return
                result;
        }
    }

}
