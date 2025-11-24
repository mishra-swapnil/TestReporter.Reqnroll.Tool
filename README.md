# TestReporter.Reqnroll.Tool

[![NuGet version (TestReporter.Reqnroll.Tool)](https://img.shields.io/nuget/v/TestReporter.Reqnroll.Tool.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/TestReporter.Reqnroll.Tool/)

TestReporter.Reqnroll.Tool is a .NET Core Global Tool used to generate HTML report file for [Reqnroll](https://reqnroll.net/) step definitions usage.

## Installation

```text
dotnet tool install --global TestReporter.Reqnroll.Tool
```

## Usage

```text
Arguments:

  -p, --project    Required. Path to the Reqnroll project folder

  -o, --output     Path to directory, where test report file will be saved

  --help           Display this help screen.

  --version        Display version information.
```

## Examples

#### Generate report for project and save HTML in current folder:
```text
reqnroll-report --project "Test project folder"
```

#### Generate report for project and save HTML in output folder:
```text
reqnroll-report --project "Test project folder" --output "Report Output folder"
```