using System;
using Main.ScanRelated;

namespace Main.Inclusion.Scanner
{
    public sealed class InclusionScannerFactory : IInclusionScannerFactory
    {
        private readonly Func<Scan> _scanFunc;

        public InclusionScannerFactory(
            Func<Scan> scanFunc
            )
        {
            if (scanFunc == null)
            {
                throw new ArgumentNullException(nameof(scanFunc));
            }

            _scanFunc = scanFunc;
        }

        public IInclusionScanner Create()
        {
            var scan = _scanFunc();

            var result = new InclusionScanner(
                scan
                );

            return
                result;
        }
    }

}
