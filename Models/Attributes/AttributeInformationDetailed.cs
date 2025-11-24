using System.Collections.Generic;
using TestReporter.Reqnroll.Tool.Models.StepDefinitions;

namespace TestReporter.Reqnroll.Tool.Models.Attributes
{
    public class AttributeInformationDetailed : AttributeInformation
    {
        public int NumberOfCalls { get; set; }

        public IEnumerable<StepDetails> GeneratedStepDefinitions { get; set; } = new List<StepDetails>();
    }
}