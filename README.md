# Katas in C#

[![Gitpod ready-to-code](https://img.shields.io/badge/Gitpod-ready--to--code-blue?logo=gitpod)](https://gitpod.io/#https://github.com/wonderbird/katas-in-csharp)
[![Build Status Badge](https://github.com/wonderbird/katas-in-csharp/workflows/.NET/badge.svg)](https://github.com/wonderbird/katas-in-csharp/actions?query=workflow%3A%22.NET%22)
[![Test Coverage (coveralls)](https://img.shields.io/coveralls/github/wonderbird/katas-in-csharp)](https://coveralls.io/github/wonderbird/katas-in-csharp)
[![Test Coverage (codeclimate)](https://img.shields.io/codeclimate/coverage-letter/wonderbird/katas-in-csharp)](https://codeclimate.com/github/wonderbird/katas-in-csharp/trends/test_coverage_total)
[![Code Maintainability](https://img.shields.io/codeclimate/maintainability-percentage/wonderbird/katas-in-csharp)](https://codeclimate.com/github/wonderbird/katas-in-csharp)
[![Issues in Code](https://img.shields.io/codeclimate/issues/wonderbird/katas-in-csharp)](https://codeclimate.com/github/wonderbird/katas-in-csharp/issues)
[![Technical Debt](https://img.shields.io/codeclimate/tech-debt/wonderbird/katas-in-csharp)](https://codeclimate.com/github/wonderbird/katas-in-csharp)
[![CodeScene Code Health](https://codescene.io/projects/21236/status-badges/code-health)](https://codescene.io/projects/21236/jobs/latest-successful/results)
[![CodeScene System Mastery](https://codescene.io/projects/21236/status-badges/system-mastery)](https://codescene.io/projects/21236/jobs/latest-successful/results)

This repository contains my implementation of some code katas in Go.

## Thanks

Many thanks to [JetBrains](https://www.jetbrains.com/?from=dotnet-starter) who provide
an [Open Source License](https://www.jetbrains.com/community/opensource/) for this project ❤️.

## Development

### Adding New Katas

Use the shell script [add.sh](add.sh) to add new katas. Read the header comment in [add.sh](add.sh) for usage
instructions.

### Quick-Start

Click the [![Gitpod ready-to-code](https://img.shields.io/badge/Gitpod-ready--to--code-blue?logo=gitpod)](https://gitpod.io/#https://github.com/wonderbird/dotnet-starter)
badge (also above) to launch a web IDE.

If that does not work for you or if you'd like to have the project on your local machine, then continue reading.

### Prerequisites

To compile, test and run this project the latest [.NET Core SDK](https://dotnet.microsoft.com/download) is required on
your machine. For calculating code metrics I recommend [metrix++](https://github.com/metrixplusplus/metrixplusplus).
This requires python.

If you are interested in test coverage, then you'll need the following tools installed:

```shell
# Note: Apple Silicon users append -a arm64 to each command.
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool
```

## Build, Test, Run

Run the following commands from the folder containing the `.sln` file in order to build and test.

### Build the Solution and Run the Tests

```sh
dotnet build
dotnet test

# If you like continuous testing then use the dotnet file watcher to trigger your tests
dotnet watch -p ./RomanNumerals.Logic.Tests test

# As an alternative, run the tests with coverage and produce a coverage report
rm -r RomanNumerals.Logic.Tests/TestResults && \
  dotnet test --no-restore --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput='./TestResults/coverage.cobertura.xml' && \
  reportgenerator "-reports:RomanNumerals.Logic.Tests/TestResults/*.xml" "-targetdir:report" "-reporttypes:Html;lcov" "-title:RomanNumerals"
open report/index.html
```

### Before Creating a Pull Request ...

... check modified shell scripts

```shell
shellcheck ./add.sh
```

... apply code formatting rules

```shell
dotnet format
```

... and check code metrics using [metrix++](https://github.com/metrixplusplus/metrixplusplus)

```shell
# Collect metrics
metrix++ collect --std.code.complexity.cyclomatic --std.code.lines.code --std.code.todo.comments --std.code.maintindex.simple -- .

# Get an overview
metrix++ view --db-file=./metrixpp.db

# Apply thresholds
metrix++ limit --db-file=./metrixpp.db --max-limit=std.code.complexity:cyclomatic:5 --max-limit=std.code.lines:code:25:function --max-limit=std.code.todo:comments:0 --max-limit=std.code.mi:simple:1
```

At the time of writing, I want to stay below the following thresholds:

```shell
--max-limit=std.code.complexity:cyclomatic:5
--max-limit=std.code.lines:code:25:function
--max-limit=std.code.todo:comments:0
--max-limit=std.code.mi:simple:1
```

I allow generated files named `*.feature.cs` to exceed these thresholds.

Finally, remove all code duplication. The next section describes how to detect code duplication.

## Identify Code Duplication

The `tools\dupfinder.bat` or `tools/dupfinder.sh` file calls
the [JetBrains dupfinder](https://www.jetbrains.com/help/resharper/dupFinder.html) tool and creates an HTML report of
duplicated code blocks in the solution directory.

In order to use the `dupfinder` you need to globally install
the [JetBrains ReSharper Command Line Tools](https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html)
On Unix like operating systems you also need [xsltproc](http://xmlsoft.org/XSLT/xsltproc2.html), which is pre-installed
on macOS.

From the folder containing the `.sln` file run

```sh
tools\dupfinder.bat
```

or

```sh
tools/dupfinder.sh
```

respectively.

The report will be created as `dupfinder-report.html` in the current directory.

# References

## .NET Core

* GitHub: [aspnet / Hosting / samples / GenericHostSample](https://github.com/aspnet/Hosting/tree/2.2.0/samples/GenericHostSample)

## Code Quality

* Continuous Testing
    * Scott Hanselman: [Command Line: Using dotnet watch test for continuous testing with .NET Core 1.0 and XUnit.net](https://www.hanselman.com/blog/command-line-using-dotnet-watch-test-for-continuous-testing-with-net-core-10-and-xunitnet)
    * Steve Smith (Ardalis): [Automate Testing and Running Apps with dotnet watch](https://ardalis.com/automate-testing-and-running-apps-with-dotnet-watch/)
* Microsoft: [Use code coverage for unit testing](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage?tabs=linux)
* GitHub: [coverlet-coverage / coverlet](https://github.com/coverlet-coverage/coverlet)
* GitHub: [danielpalme / ReportGenerator](https://github.com/danielpalme/ReportGenerator)
* JetBrains s.r.o.: [dupFinder Command-Line Tool](https://www.jetbrains.com/help/resharper/dupFinder.html)
* Vidar Holen: [ShellCheck](https://github.com/koalaman/shellcheck) - shell script static analysis tool
* Scott Hanselman: [EditorConfig code formatting from the command line with .NET Core's dotnet format global tool](https://www.hanselman.com/blog/editorconfig-code-formatting-from-the-command-line-with-net-cores-dotnet-format-global-tool)
* [EditorConfig.org](https://editorconfig.org)
* GitHub: [dotnet / roslyn - .editorconfig](https://github.com/dotnet/roslyn/blob/master/.editorconfig)
* Check all the badges on top of this README