using System;
using System.IO;
using System.Linq;
using RazorEngine;
using RazorEngine.Templating;
using System.Threading.Tasks;
using System.Collections.Generic;
using TestReporter.Reqnroll.Tool.Constants;
using TestReporter.Reqnroll.Tool.Models.Report;
using TestReporter.Reqnroll.Tool.Models.Attributes;

namespace TestReporter.Reqnroll.Tool.Helpers.Reports
{
    public static class TestReportGenerator
    {
        private static readonly Lazy<Task<string>> TemplateContentAsync = new Lazy<Task<string>>(async () => 
        {
            var lines = await File.ReadAllLinesAsync(ApplicationConstants.ReportTemplatePath);
            return string.Join(Environment.NewLine, lines.Skip(1));
        });

        public static async Task<string> GetHtmlReportAsync(List<AttributeInformationDetailed> stepsCalls,
            ReportSettings reportSettings)
        {
            var templateContent = await TemplateContentAsync.Value;
            
            return Engine.Razor.RunCompile(
                templateContent,
                ApplicationConstants.TestReportTemplateKey, typeof(ReportDetails),
                new ReportDetails
                {
                    TotalNumberOfSteps = stepsCalls.Count,
                    ProjectName = reportSettings.ProjectName,
                    MaterialIcons = reportSettings.MaterialIcons,
                    ReqnrollIconPath = reportSettings.ReqnrollIconPath,
                    GeneratedDateTime = reportSettings.GeneratedDateTime,
                    MaterialJsLibraryPath = reportSettings.MaterialJsLibraryPath,
                    MaterialCssLibraryPath = reportSettings.MaterialCssLibraryPath,
                    TotalNumberOfUnusedSteps = stepsCalls.Count(x => x.NumberOfCalls == 0),
                    Results = stepsCalls.GroupBy(x => x.Type)
                        .Select(x => new ReportResult
                        {
                            Type = x.Key,
                            Attributes = x.ToList()
                        })
                });
        }
    }
}