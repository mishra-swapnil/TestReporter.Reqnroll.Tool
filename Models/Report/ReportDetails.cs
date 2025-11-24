using System.Collections.Generic;

namespace TestReporter.Reqnroll.Tool.Models.Report
{
    public class ReportDetails
    {
        public string ProjectName { get; set; } = string.Empty;

        public string MaterialIcons { get; set; } = string.Empty;

        public int TotalNumberOfSteps { get; set; }

        public string ReqnrollIconPath { get; set; } = string.Empty;

        public string GeneratedDateTime { get; set; } = string.Empty;

        public int TotalNumberOfUnusedSteps { get; set; }

        public string MaterialJsLibraryPath { get; set; } = string.Empty;

        public string MaterialCssLibraryPath { get; set; } = string.Empty;

        public IEnumerable<ReportResult> Results { get; set; } = new List<ReportResult>();
    }
}