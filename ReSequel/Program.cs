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

                //make results: checking reports
                var results = processedInclusions.ConvertAll(j => j.GenerateReport());

                solutionValidator.Progress.UpdateMessage();

                var faileds = results.FindAll(j => !j.IsSuccess);

                foreach (var failed in faileds)
                {
                    if (failed.IsSuccess)
                    {
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    Console.WriteLine("{0} : [{1}]", failed.FilePath, failed.LineNumber);
                    Console.WriteLine("    " + failed.SqlQuery);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(failed.FailMessage);

                    Console.WriteLine();

                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed count: {faileds.Count}" );
                Console.WriteLine();
                Console.ResetColor();

                return faileds.Count() > 0 ? 1 : 0;
            }
        }
    }
}
