using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;

namespace TestReporter.Reqnroll.Tool.Models.Console
{
    public class ConsoleArguments
    {
        [Option('p', "project", Required = true, HelpText = "Path to the Reqnroll project folder")]
        public string ProjectFolder { get; set; }

        [Option('o', "output", Required = false, HelpText = "Path to directory, where test report file will be saved")]
        public string TestReportDirectory { get; set; }

        [Usage(ApplicationAlias = "reqnroll-report")]
        public static IEnumerable<Example> Examples => new List<Example>
        {
            new Example("Generate step definition usage report for project and save HTML in current folder",
                new ConsoleArguments
                {
                    ProjectFolder = "Test project folder"
                }),
            new Example("Generate step definition usage report for project and save HTML in output folder",
                new ConsoleArguments
                {
                    ProjectFolder = "Test project folder",
                    TestReportDirectory = "Report Output folder"
                })
        };
    }
}