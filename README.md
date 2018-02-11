# OpenDLC [![Travis Build Status](https://travis-ci.org/nikeee/OpenDLC.svg)](https://travis-ci.org/nikeee/OpenDLC) [![Windows Build status](https://ci.appveyor.com/api/projects/status/3j28aejetcfv57b3?svg=true)](https://ci.appveyor.com/project/nikeee/opendlc) [![NuGet](https://img.shields.io/nuget/v/OpenDLC.svg)](https://www.nuget.org/packages/OpenDLC/)

.NET API for DLC formats. Supports DLC, RSDF and CCF.


## RSDF
```C#
var container = await RsdfContainer.FromFileAsync(pathToFile);
Console.WriteLine("All links in this file:");
foreach(RsdfEntry currentLink in container)
{
    Console.WriteLine(currentLink);
}
```

## CCF
```C#
var container = await CcfContainer.FromFileAsync(pathToFile);
Console.WriteLine("All links in this file:");
foreach(CcfPackage currentPackage in container)
{
    foreach(CcfEntry currentLink in currentPackage)
    {
        Console.WriteLine(currentLink);
    }
}
```

## DLC
You need an internet connection to decrypt the DLC format. This is by design.
```C#
// Contact the JD developers to get an appId and appSecret
var appSettings = new DlcAppSettings(appId, appSecret, appRevision);

var container = await DlcContainer.FromFileAsync(pathToFile, appSettings);
foreach(DlcPackage currentPackage in container)
{
    foreach(DlcEntry currentLink in currentPackage)
    {
        Console.WriteLine(currentLink);
    }
}
```


Pretty much the same. You can even join the links together.
```C#
var someRsdf = await RsdfContainer.FromFileAsync(pathToFile);
var someCcf = await CcfContainer.FromFileAsync(pathToFile);
IEnumerable<DownloadEntry> allLinks = someRsdf.Concat(someCcf.Packages.Concat());
Console.WriteLine("All links in CCF/RSDF:");
foreach(DownloadEntry currentLink in allLinks)
{
    Console.WriteLine(currentLink);
}
```
