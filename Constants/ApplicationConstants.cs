using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace TestReporter.Reqnroll.Tool.Constants
{
    public static class ApplicationConstants
    {
        private static readonly Lazy<string> PackageDirectoryPathLazy = new Lazy<string>(() =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty);

        private static string PackageDirectoryPath => PackageDirectoryPathLazy.Value;

        public static string ProjectFileExtension { get; } = "*.csproj";

        public static string TestReportTemplateKey { get; } = "TestReport";

        public static string StepDefinitionFileExtension { get; } = "*.cs";

        public static string ExcludeExamplePattern { get; } = "string.Format";

        public static string FeatureCSharpFileExtension { get; } = "*.feature.cs";

        public static string GeneratedReportFilePathWithName { get; } = "Test Report - {0}.html";

        public static IReadOnlyList<string> ExcludeDirectories { get; } =
            new[] { "bin", "obj" };

        private static readonly string[] _StepDefinitionAttributeMethods =
            new[] { "Given", "When", "Then" };

        public static readonly IReadOnlyList<string> StepDefinitionAttributeMethods = 
            _StepDefinitionAttributeMethods
                .Concat(_StepDefinitionAttributeMethods.Select(x => x + "Async"))
                .ToArray();

        private static readonly string[] _GeneratedStepDefinitionMethods =
            new[] { "When", "Given", "Then", "And", "But" };

        public static readonly IReadOnlyList<string> GeneratedStepDefinitionMethods = 
            _GeneratedStepDefinitionMethods
                .Concat(_GeneratedStepDefinitionMethods.Select(x => x + "Async"))
                .ToArray();

        public static string ReportTemplatePath => 
            Path.Combine(PackageDirectoryPath, "Report", "Template.cshtml");

        public static string MaterialIcons { get; } =
            "https://fonts.googleapis.com/icon?family=Material+Icons";

        public static string MaterialCssLibraryPath { get; } =
            "https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/css/materialize.min.css";

        public static string MaterialJsLibraryPath { get; } =
            "https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0/js/materialize.min.js";

        public static string ReqnrollIconPathGithubUrl { get; } =
            "https://raw.githubusercontent.com/reqnroll/Reqnroll/refs/heads/main/reqnroll.ico";
    }
}