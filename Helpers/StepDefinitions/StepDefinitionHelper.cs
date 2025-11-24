using Serilog;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using TestReporter.Reqnroll.Tool.Constants;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using TestReporter.Reqnroll.Tool.Models.Attributes;

namespace TestReporter.Reqnroll.Tool.Helpers.StepDefinitions
{
    public static class StepDefinitionHelper
    {
        public static async Task<List<AttributeInformation>> ExtractInformationFromFilesAsync(
            IEnumerable<string> stepDefinitionFilePaths)
        {
            var tasks = stepDefinitionFilePaths.Select(ExtractInformationFromFileAsync);
            var results = await Task.WhenAll(tasks);
            return results.SelectMany(x => x).ToList();
        }

        private static async Task<IEnumerable<AttributeInformation>> ExtractInformationFromFileAsync(string path)
        {
            Log.Information("Extracting information about step definitions from file: {Path}", path);

            var stepDefinitionContent = await File.ReadAllTextAsync(path);

            var attributes = CSharpSyntaxTree.ParseText(stepDefinitionContent)
                .GetRoot()
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Where(a => ApplicationConstants.StepDefinitionAttributeMethods.Contains(a.Name.ToString()))
                .ToList();

            var results = new List<AttributeInformation>();
            
            foreach (var attribute in attributes)
            {
                var argumentType = attribute.Name.ToString();
                var attributeArgumentText = attribute.ArgumentList?.Arguments
                    .FirstOrDefault()
                    ?.ToFullString();

                var argumentValue = await CSharpScript.EvaluateAsync<string>(attributeArgumentText);

                Log.Information("Found attribute of type {Type} with argument {Argument}",
                    argumentType, argumentValue);

                results.Add(new AttributeInformation
                {
                    Type = argumentType,
                    Value = argumentValue
                });
            }

            return results;
        }
    }
}