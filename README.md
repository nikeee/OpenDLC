# OpenDLC [![Build Status](https://travis-ci.org/nikeee/OpenDLC.svg)](https://travis-ci.org/nikeee/OpenDLC) [![NuGet](https://img.shields.io/nuget/v/OpenDLC.svg)](https://www.nuget.org/packages/OpenDLC/)

Opening up DLC formats.


## RSDF
```C#
var container = await RsdfContainer.FromFileAsync(pathToFile);
Console.WriteLine("All link in this file:");
foreach(RsdfEntry currentLink in container)
{
	Console.WriteLine(currentLink);
}
```
