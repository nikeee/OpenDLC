# OpenDLC

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
