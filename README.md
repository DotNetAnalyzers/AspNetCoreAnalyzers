# AspNetCoreAnalyzers

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreAnalyzers.svg)](https://www.nuget.org/packages/AspNetCoreAnalyzers/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://dev.azure.com/johan-larsson/AspNetCoreAnalyzers/_apis/build/status/AspNetCoreAnalyzers-CI?branchName=master)](https://dev.azure.com/johan-larsson/AspNetCoreAnalyzers/_build/latest?definitionId=3&branchName=master)
[![Build status](https://ci.appveyor.com/api/projects/status/wk4ra33vaa9okd9o/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/aspnetcoreanalyzers/branch/master)
[![Join the chat at https://gitter.im/DotNetAnalyzers/AspNetCoreAnalyzers](https://badges.gitter.im/DotNetAnalyzers/AspNetCoreAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/AspNetCoreAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Roslyn analyzers for ASP.NET.Core.


<!-- start generated table -->
<table>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP001.md">ASP001</a></td>
    <td>Parameter name does not match the name specified by the route parameter.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP002.md">ASP002</a></td>
    <td>Route parameter name does not match the method parameter name.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP003.md">ASP003</a></td>
    <td>Parameter type does not match the type specified by the name specified by the route parameter.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP004.md">ASP004</a></td>
    <td>Route parameter type does not match the method parameter type.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP005.md">ASP005</a></td>
    <td>Syntax error in parameter.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP006.md">ASP006</a></td>
    <td>Escape constraint regex.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP007.md">ASP007</a></td>
    <td>The method has no corresponding parameter.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP008.md">ASP008</a></td>
    <td>Invalid route parameter name.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP009.md">ASP009</a></td>
    <td>Use kebab-cased urls.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP010.md">ASP010</a></td>
    <td>Unexpected character in url.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP011.md">ASP011</a></td>
    <td>Route parameter appears more than once.</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP012.md">ASP012</a></td>
    <td>Don't use [controller].</td>
  </tr>
  <tr>
    <td><a href="https://github.com/DotNetAnalyzers/AspNetCoreAnalyzers/tree/master/documentation/ASP013.md">ASP013</a></td>
    <td>Name the controller to match the route.</td>
  </tr>
<table>
<!-- end generated table -->


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
