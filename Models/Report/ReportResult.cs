using System.Collections.Generic;
using TestReporter.Reqnroll.Tool.Models.Attributes;

namespace TestReporter.Reqnroll.Tool.Models.Report
{
    public class ReportResult
    {
        public string Type { get; set; }

        public List<AttributeInformationDetailed> Attributes { get; set; }
    }
}