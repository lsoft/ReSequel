using Main;
using Main.Helper;
using Main.Other;
using Main.SolutionValidator;
using System;
using System.Linq;
using System.Threading;
using TestConsole.CompositionRoot;
using TestConsole.TaskRelated;

namespace TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var taskXml = args[0];

            var task = taskXml.ReadXml<WorkingTask>();

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

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(Environment.NewLine);

                foreach (var result in results)
                {
                    if (result.IsSuccess)
                    {
                        continue;
                    }
                    
                    Console.ForegroundColor = result.IsSuccess ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;

                    Console.WriteLine("{0} : [{1}]", result.FilePath, result.LineNumber);
                    Console.WriteLine("    " + result.SqlQuery);

                    if (!result.IsSuccess)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(result.FailMessage);
                    }

                    Console.WriteLine();

                    Console.ResetColor();
                }

                Console.WriteLine(
                    "Total: {0}, Correct: {1}, Fail: {2}",
                    results.Count,
                    results.Count(j => j.IsSuccess),
                    results.Count(j => !j.IsSuccess)
                    );

                Thread.Sleep(1000);
                //Console.ReadLine();
            }
        }
    }


}
