@echo off

echo Removing existing packages in current directory...
del *.nupkg

echo Packing packages...
"../.nuget/NuGet.exe" Pack "./OpenDLC.nuspec"

echo Pushing packages to NuGet.org...
"../.nuget/NuGet.exe" Push *.nupkg
