# Xdiff

<img src="https://github.com/jkone27/xdiff/blob/master/Pics/dom-view.png?raw=true" width="20%" height="20%"/>

A simple fsharp xml diff

```csharp
var diffs = XmlDiffUtils.ComputeDiff("A.xml", "B.xml", new string[] {});
foreach (var d in diffs)
    Console.WriteLine(d);
```

[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/jkone27/AliceMQ/issues)


### Status
[![Build Status](https://img.shields.io/travis/jkone27/xdiff.svg)](https://travis-ci.org/jkone27/xdiff)

