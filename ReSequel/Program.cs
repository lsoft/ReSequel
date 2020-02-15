using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Main.Helper;
using Main.Other;
using Main.SolutionValidator;
using ReSequel.CompositionRoot;
using ReSequel.TaskRelated;

namespace ReSequel
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("1st argument: path to SLN file");
                Console.WriteLine("2nd argument: path scan description XML file");
                Console.WriteLine("3rd argument: RDBMS type (SqlServer, Sqlite)");
                Console.WriteLine("4rd argument: connection string");

                return 1;
            }

            var strFullFileName = typeof(Program).Module.FullyQualifiedName;
            var strShortFileName = typeof(Program).Module.Name;
            var strDir = strFullFileName.Substring(0, strFullFileName.Length - strShortFileName.Length - 1);
            Directory.SetCurrentDirectory(strDir);

            var slnPath = Path.GetFullPath(args[0]);
            var scanDescriptionXmlPath = Path.GetFullPath(args[1]);
            var sqlType = args[2];
            var connectionString = args[3];

            var task = new WorkingTask
            {
                TargetSolution = slnPath,
                ScanScheme = scanDescriptionXmlPath,
                SqlExecutor = new WorkingTaskSqlExecutor
                {
                    Type = sqlType,
                    ConnectionString = connectionString
                }
            };

            using (var root = new Root(task))
            {
                root.BindAll();

                var solutionValidator = root.GetInstance<ISolutionValidator>();

                MSBuildRegisterer.RegisterDefaultOnce();

                var processedInclusions = solutionValidator.Execute(
                    task.TargetSolution
                    );

                solutionValidator.Progress.UpdateMessage();

                //make results: checking reports
                var reports = processedInclusions.ConvertAll(j => j.GenerateReport());

                //take only succeeded and NOT muted
                var succeededReports = reports.FindAll(j => !j.IsFailed && !j.IsMuted);

                //take only failed and NOT muted
                var failedReports = reports.FindAll(j => j.IsFailed && !j.IsMuted);

                //take only NOT muted
                var mutedReports = reports.FindAll(j => j.IsMuted);

                foreach (var failedReport in failedReports)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    Console.WriteLine("{0} : [{1}]", failedReport.FilePath, failedReport.LineNumber);
                    Console.WriteLine("    " + failedReport.SqlQuery);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(failedReport.FailMessage);

                    Console.WriteLine();

                    Console.ResetColor();
                }

                Console.ResetColor();
                Console.WriteLine($"Total count: {reports.Count}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Succeeded count: {succeededReports.Count}");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed count: {failedReports.Count}" );

                Console.ResetColor();
                Console.WriteLine($"Muted count: {mutedReports.Count}");


                Console.WriteLine();
                Console.ResetColor();

                return failedReports.Count > 0 ? 1 : 0;
            }
        }
    }
}
