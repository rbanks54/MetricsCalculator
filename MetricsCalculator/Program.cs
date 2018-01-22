using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace MetricsCalculator
{
    // todo: add all accumulating statements to lines of code strategy, need to go
    // through syntax kind class and look for statements.
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string fileName = "";
                if (args.Length > 0)
                {
                    fileName = args[0];
                }
                while (string.IsNullOrWhiteSpace(fileName))
                {
                    Console.Write("Path of file to analyse: ");
                    fileName = Console.ReadLine();
                    if (!File.Exists(fileName))
                    {
                        Console.WriteLine("File not found.");
                        fileName = string.Empty;
                    }
                }

                MetricsTreeWalker analyzer = new MetricsTreeWalker();
                analyzer.Initialize();
                analyzer.GetMetricsForFile(fileName);

                MetricsAccumulator metrics = analyzer.GetMetrics(fileName);
                LinesAndCyclomaticReport<string> report = new LinesAndCyclomaticReport<string>(new LinesAndCyclomaticReportConsoleFormatter());
                Console.WriteLine(report.GenerateReport(metrics));

                ABCMetricsReport<string> abcReport = new ABCMetricsReport<string>(new ABCMetricsReportConsoleFormatter());
                Console.WriteLine(abcReport.GenerateReport(metrics));
            }
            catch (Exception e)
            {
                Console.WriteLine("The metrics analyzer threw an exception.");
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
