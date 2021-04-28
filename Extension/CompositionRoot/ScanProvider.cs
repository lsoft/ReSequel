using Extension.ConfigurationRelated;
using Extension.Helper;
using Main.Helper;
using Main.Inclusion.Scanner;
using Main.ScanRelated;
using System;
using System.IO;

namespace Extension.CompositionRoot
{
    public class ScanProvider : IScanProvider
    {
        public const string ConfigurationFileName = "Configuration.xml";
        public const string ScanSchemeFileName = "ScanDescription.xml";

        private readonly ISolutionNameProvider _solutionNameProvider;
        private readonly IConfigurationProvider _configurationProvider;

        public ScanProvider(
            ISolutionNameProvider solutionNameProvider,
            IConfigurationProvider configurationProvider
            )
        {
            if (solutionNameProvider is null)
            {
                throw new ArgumentNullException(nameof(solutionNameProvider));
            }

            if (configurationProvider is null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            _solutionNameProvider = solutionNameProvider;
            _configurationProvider = configurationProvider;
        }

        public Scan CreateScan()
        {
            if (string.IsNullOrWhiteSpace(_solutionNameProvider.SolutionName))
            {
                //no open solution found, so return empty Scan
                return new Scan();
            }

            if (!_configurationProvider.TryRead(out _))
            {
                throw new InvalidOperationException("Cannot read configuration file");
            }

            var solutionNamePath = new FileInfo(_solutionNameProvider.SolutionName);
            var solutionFileName = solutionNamePath.Name;
            var solutionFileNameWithoutExtension =
                solutionNamePath.Extension.Length > 0
                    ? solutionFileName.Substring(0, solutionFileName.Length - solutionNamePath.Extension.Length)
                    : solutionFileName;
            var solutionFolder = solutionNamePath.Directory.FullName;
            var specificScanFilePath = Path.Combine(solutionFolder, $"{solutionFileNameWithoutExtension}.{ScanSchemeFileName}");
            var generalScanFilePath = Path.Combine(solutionFolder, ScanSchemeFileName);

            if (File.Exists(specificScanFilePath))
            {
                return specificScanFilePath.ReadXml<Scan>();
            }

            if (!File.Exists(generalScanFilePath))
            {
                //if scan file does not exists for this solution, we create it with default one
                ReflectionHelper.ExtractEmbeddedResource(generalScanFilePath, "Extension." + ScanSchemeFileName);
            }

            var scan = generalScanFilePath.ReadXml<Scan>();
            return scan;
        }
    }
}
