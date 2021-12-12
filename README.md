# AspNetCoreAnalyzers

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreAnalyzers.svg)](https://www.nuget.org/packages/AspNetCoreAnalyzers/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://dev.azure.com/johan-larsson/AspNetCoreAnalyzers/_apis/build/status/AspNetCoreAnalyzers-CI?branchName=master)](https://dev.azure.com/johan-larsson/AspNetCoreAnalyzers/_build/latest?definitionId=3&branchName=master)
[![Build status](https://ci.appveyor.com/api/projects/status/wk4ra33vaa9okd9o/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/aspnetcoreanalyzers/branch/master)
[![Join the chat at https://gitter.im/DotNetAnalyzers/AspNetCoreAnalyzers](https://badges.gitter.im/DotNetAnalyzers/AspNetCoreAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/AspNetCoreAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Roslyn analyzers for ASP.NET.Core.

![animation](https://user-images.githubusercontent.com/1640096/51425954-b0d84380-1be3-11e9-8818-dd66e116a30a.gif)


| Id       | Title
| :--      | :--
| [ASP001](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP001.md)| Parameter name does not match the name specified by the route parameter
| [ASP002](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP002.md)| Route parameter name does not match the method parameter name
| [ASP003](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP003.md)| Parameter type does not match the type specified by the name specified by the route parameter
| [ASP004](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP004.md)| Route parameter type does not match the method parameter type
| [ASP005](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP005.md)| Syntax error in parameter
| [ASP006](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP006.md)| Escape constraint regex
| [ASP007](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP007.md)| The method has no corresponding parameter
| [ASP008](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP008.md)| Invalid route parameter name
| [ASP009](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP009.md)| Use kebab-cased urls
| [ASP010](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP010.md)| Unexpected character in url
| [ASP011](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP011.md)| Route parameter appears more than once
| [ASP012](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP012.md)| Don't use [controller]
| [ASP013](https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP013.md)| Name the controller to match the route


## Using AspNetCoreAnalyzers

The preferable way to use the analyzers is to add the nuget package [AspNetCoreAnalyzers](https://www.nuget.org/packages/AspNetCoreAnalyzers/)
to the project(s).

The severity of individual rules may be configured using [.ruleset](https://msdn.microsoft.com/en-us/library/dd264996.aspx) files
in Visual Studio.

## Installation

AspNetCoreAnalyzers can be installed using [Paket](https://fsprojects.github.io/Paket/) or the NuGet command line or the NuGet Package Manager in Visual Studio.


**Install using the command line:**
```bash
Install-Package AspNetCoreAnalyzers
```

## Updating

The ruleset editor does not handle changes IDs well, if things get out of sync you can try:

1) Close visual studio.
2) Edit the ProjectName.rulset file and remove the AspNetCoreAnalyzers element.
3) Start visual studio and add back the desired configuration.

Above is not ideal, sorry about this. Not sure if this is our bug.


## Current status

Early alpha, names and IDs may change.
