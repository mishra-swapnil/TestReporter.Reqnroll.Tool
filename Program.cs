using System;
using Serilog;
using System.IO;
using System.Linq;
using CommandLine;
using System.Diagnostics;
using System.Threading.Tasks;
using TestReporter.Reqnroll.Tool.Constants;
using TestReporter.Reqnroll.Tool.Models.Report;
using TestReporter.Reqnroll.Tool.Helpers.Calls;
using TestReporter.Reqnroll.Tool.Models.Console;
using TestReporter.Reqnroll.Tool.Helpers.Reports;
using TestReporter.Reqnroll.Tool.Helpers.Features;
using TestReporter.Reqnroll.Tool.Helpers.StepDefinitions;

namespace TestReporter.Reqnroll.Tool
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<ConsoleArguments>(args).WithParsedAsync(async parsed =>
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate:
                        "[{Timestamp:G}] [{Level}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                if (!Directory.Exists(parsed.ProjectFolder))
                {
                    Log.Error("Features directory not found: {Directory}.", parsed.ProjectFolder);
                    Environment.Exit(1);
                }

                if (!File.Exists(ApplicationConstants.ReportTemplatePath))
                {
                    Log.Error("File not found: {File}.", ApplicationConstants.ReportTemplatePath);
                    Environment.Exit(1);
                }

                var projectFile = Directory.EnumerateFiles(parsed.ProjectFolder,
                        ApplicationConstants.ProjectFileExtension, SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(projectFile))
                {
                    Log.Error("*.csproj file has not been found.");
                    Environment.Exit(1);
                }

                var stopwatch = Stopwatch.StartNew();

                // Get all directories that are not excluded
                var projectDirectories = Directory.EnumerateDirectories(parsed.ProjectFolder)
                    .Where(d => !ApplicationConstants.ExcludeDirectories.Contains(
                        Path.GetFileName(d), StringComparer.OrdinalIgnoreCase))
                    .ToArray();

                // Collect all CS files in one pass, then split by type
                var allCsFiles = projectDirectories
                    .SelectMany(d => Directory.EnumerateFiles(d, ApplicationConstants.StepDefinitionFileExtension, SearchOption.AllDirectories))
                    .ToArray();

                var stepPaths = allCsFiles
                    .Where(p => !p.EndsWith(".feature.cs", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Log.Information("Found {Count} code files.", stepPaths.Count);

                var featureCsPaths = allCsFiles
                    .Where(p => p.EndsWith(".feature.cs", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Log.Information("Found {Count} generated feature code files.", featureCsPaths.Count);

                var stepDefinitionsInfo =
                    await StepDefinitionHelper.ExtractInformationFromFilesAsync(stepPaths);

                Log.Information("Finished extracting information about step definitions");

                var stepDefinitionsGeneratedInfo =
                    await CSharpFeatureHelper.ExtractInformationFromFilesAsync(featureCsPaths);

                Log.Information("Finished extracting information about generated feature's code");

                var stepDefinitionCallInformation =
                    StepDefinitionCallCountHelper
                        .CalculateNumberOfCalls(stepDefinitionsInfo, stepDefinitionsGeneratedInfo).ToList();

                if (!stepDefinitionCallInformation.Any())
                {
                    Log.Error("No step definitions have been found.");
                    Environment.Exit(0);
                }

                Log.Information("Found {Count} step definitions.", stepDefinitionCallInformation.Count);

                Log.Information("Staring generating HTML test report file.");

                var reportSettings = new ReportSettings
                {
                    MaterialIcons = ApplicationConstants.MaterialIcons,
                    GeneratedDateTime = DateTime.UtcNow.ToString("g"),
                    ProjectName = Path.GetFileNameWithoutExtension(projectFile),
                    ReqnrollIconPath = ApplicationConstants.ReqnrollIconPathGithubUrl,
                    MaterialJsLibraryPath = ApplicationConstants.MaterialJsLibraryPath,
                    MaterialCssLibraryPath = ApplicationConstants.MaterialCssLibraryPath
                };

                var resultHtml = await TestReportGenerator.GetHtmlReportAsync(stepDefinitionCallInformation, reportSettings);

                Log.Information("Finished generating HTML test report file.");

                var testReportHtmlFileName = string.Format(ApplicationConstants.GeneratedReportFilePathWithName,
                    reportSettings.ProjectName);

                var testReportOutputDirectory = parsed.TestReportDirectory ?? Directory.GetCurrentDirectory();
                var testReportFullPath = Path.GetFullPath(testReportOutputDirectory);

                if (!Directory.Exists(testReportFullPath))
                {
                    Directory.CreateDirectory(testReportFullPath);
                }
                
                var testReportHtmlFilePath = Path.Combine(testReportFullPath, testReportHtmlFileName);

                Log.Information("Saving generated test report.");

                await File.WriteAllTextAsync(testReportHtmlFilePath, resultHtml);

                Log.Information("Generated test report file path: {FilePath}", testReportHtmlFilePath);
                
                stopwatch.Stop();

                var elapsed = stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.ff");

                Log.Information("Elapsed time: {ElapsedTime}", elapsed);
            });
        }
    }
}