# XmlDifference

[![.NET](https://github.com/jkone27/xdiff/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jkone27/xdiff/actions/workflows/dotnet.yml)

<img src="https://github.com/jkone27/xdiff/blob/master/Pics/dom-view.png?raw=true" width="20%" height="20%"/>

A simple fsharp xml diff that you can call from C#

```csharp

//workaround for F# limitation in extension methods..
using static XmlDifference.DiffExtensions.DifferenceExtension;

var diffs = "A.xml".Difference("B.xml");
foreach (var d in diffs)
    Console.WriteLine(d);
```

[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/jkone27/AliceMQ/issues)
