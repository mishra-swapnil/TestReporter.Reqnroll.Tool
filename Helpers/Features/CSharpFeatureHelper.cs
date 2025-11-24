using System;
using Serilog;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using TestReporter.Reqnroll.Tool.Constants;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using TestReporter.Reqnroll.Tool.Models.Attributes;

namespace TestReporter.Reqnroll.Tool.Helpers.Features
{
    public static class CSharpFeatureHelper
    {
        public static async Task<List<AttributeInformation>> ExtractInformationFromFilesAsync(
            IEnumerable<string> stepDefinitionGeneratedFilePaths)
        {
            var tasks = stepDefinitionGeneratedFilePaths.Select(ExtractInformationFromFileAsync);
            var results = await Task.WhenAll(tasks);
            return results.SelectMany(x => x).ToList();
        }

        private static async Task<IEnumerable<AttributeInformation>> ExtractInformationFromFileAsync(string path)
        {
            Log.Information("Extracting information about generated feature code from file: {Path}", path);

            var generatedStepDefinitionContent = await File.ReadAllTextAsync(path);

            var nodes = CSharpSyntaxTree.ParseText(generatedStepDefinitionContent)
                .GetRoot()
                .DescendantNodes()
                .ToList();

            var generatedStepDefinitions = await ExtractInformationAboutGeneratedStepDefinitionsAsync(nodes, path);
            var generatedStepOutlines = await ExtractInformationAboutGeneratedScenarioOutlinesAsync(nodes, path);
            return generatedStepDefinitions.Union(generatedStepOutlines);
        }

        private static async Task<List<AttributeInformation>> ExtractInformationAboutGeneratedStepDefinitionsAsync(
            IEnumerable<SyntaxNode> nodes, string path)
        {
            var invocations = nodes.OfType<InvocationExpressionSyntax>()
                .Where(invokedMethod =>
                {
                    if (invokedMethod.Expression is not MemberAccessExpressionSyntax memberAccess)
                        return false;
                    
                    if (!ApplicationConstants.GeneratedStepDefinitionMethods.Contains(memberAccess.Name.ToString()))
                        return false;
                    
                    return !invokedMethod.ArgumentList.Arguments.Any(arg =>
                        arg.ToString().Contains(ApplicationConstants.ExcludeExamplePattern));
                })
                .ToList();

            var results = new List<AttributeInformation>();
            var featureFileName = Path.GetFileNameWithoutExtension(path);

            foreach (var invokedMethod in invocations)
            {
                var memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)invokedMethod.Expression;
                var methodArgumentTypeName = memberAccessExpressionSyntax.Name.ToString();
                var methodArgumentText = invokedMethod.ArgumentList.Arguments.FirstOrDefault()?.ToString();

                var methodArgumentValue = await CSharpScript.EvaluateAsync<string>(methodArgumentText);

                Log.Information("Found method call of type {Type} with argument {Argument}",
                    methodArgumentTypeName, methodArgumentValue);

                results.Add(new AttributeInformation
                {
                    Type = methodArgumentTypeName,
                    Value = methodArgumentValue,
                    FeatureFileName = featureFileName
                });
            }

            return results;
        }

        private static async Task<List<AttributeInformation>> ExtractInformationAboutGeneratedScenarioOutlinesAsync(
            List<SyntaxNode> nodes, string path)
        {
            var methodCalList = nodes
                .OfType<InvocationExpressionSyntax>()
                .Where(invokedMethod =>
                {
                    if (invokedMethod.Expression is not MemberAccessExpressionSyntax memberAccess)
                        return false;
                    
                    if (!ApplicationConstants.GeneratedStepDefinitionMethods.Contains(memberAccess.Name.ToString()))
                        return false;
                    
                    return invokedMethod.ArgumentList.Arguments.Any(arg =>
                        arg.ToString().Contains(ApplicationConstants.ExcludeExamplePattern));
                })
                .ToList();

            var featureFileName = Path.GetFileNameWithoutExtension(path);
            var results = new List<AttributeInformation>();

            foreach (var x in methodCalList)
            {
                var memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)x.Expression;
                var methodArgumentTypeName = memberAccessExpressionSyntax.Name.ToString();

                var formattedStringSyntax = x.ArgumentList.Arguments.FirstOrDefault();
                var invokeExpression = formattedStringSyntax?.Expression as InvocationExpressionSyntax;
                var formatStringText = $"return {invokeExpression?.ToFullString()};";

                var formatArgumentsNames = invokeExpression
                    ?.ArgumentList
                    .Arguments
                    .Skip(1)
                    .Select(arg => arg.ToFullString())
                    .ToList();

                var argumentNameValues = ExtractMethodInfoFromInvocation(x, nodes);
                
                foreach (var arg in argumentNameValues)
                {
                    var filteredArgs = arg
                        .Where(k => formatArgumentsNames?.Contains(k.Item1) == true)
                        .Select(v => (v.Item1, v.Item2?.ToFullString()))
                        .ToList();

                    var dictScript = string.Join("\n", filteredArgs.Select(kvp => $"var {kvp.Item1} = {kvp.Item2};"));
                    
                    var scriptState = await CSharpScript.RunAsync<string>(dictScript);
                    var result = await scriptState.ContinueWithAsync<string>(formatStringText);
                    
                    results.Add(new AttributeInformation
                    {
                        Type = methodArgumentTypeName,
                        Value = result.ReturnValue,
                        FeatureFileName = featureFileName
                    });
                }
            }

            return results;
        }

        private static IEnumerable<List<ValueTuple<string, ArgumentSyntax>>> ExtractMethodInfoFromInvocation(
            SyntaxNode invocationExpressionSyntax, IEnumerable<SyntaxNode> nodes)
        {
            var methodInfo = FindMethodExpressionSyntax(invocationExpressionSyntax);
            var methodName = methodInfo.Identifier.Text;

            var methodParameters = methodInfo.ParameterList
                .Parameters
                .Select(x => x.Identifier.Text)
                .ToList();

            var parameterCount = methodParameters.Count - 1;

            return nodes.OfType<InvocationExpressionSyntax>()
                .Where(im => im.Expression is MemberAccessExpressionSyntax mi && mi.Name.ToString() == methodName)
                .Select(x => methodParameters.Zip(x.ArgumentList.Arguments, (param, arg) => (param, arg))
                    .Take(parameterCount)
                    .ToList())
                .ToList();
        }

        private static MethodDeclarationSyntax FindMethodExpressionSyntax(SyntaxNode expressionSyntax) =>
            expressionSyntax switch
            {
                MethodDeclarationSyntax e => e,
                _ => FindMethodExpressionSyntax(expressionSyntax.Parent!)
            };
    }
}