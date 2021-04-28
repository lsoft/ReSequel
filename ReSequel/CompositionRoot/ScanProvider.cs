using Main.Helper;
using Main.Other;
using Main.ScanRelated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReSequel.CompositionRoot
{
    public class ScanProvider : IScanProvider
    {
        private readonly string _pathToXmlScanSchema;

        public ScanProvider(
            string pathToXmlScanSchema
            )
        {
            if (pathToXmlScanSchema is null)
            {
                throw new ArgumentNullException(nameof(pathToXmlScanSchema));
            }

            _pathToXmlScanSchema = pathToXmlScanSchema;
        }

        public Scan CreateScan()
        {
            var filePath = _pathToXmlScanSchema.GetFullPathToFile();

            var scan = filePath.ReadXml<Scan>();

            return scan;
        }
    }
}
