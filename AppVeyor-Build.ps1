# MIT License
#
# Copyright (c) 2018 Marian Dolinský
#
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation
# files (the "Software"), to deal in the Software without
# restriction, including without limitation the rights to use,
# copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following
# conditions:
#
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
# HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.

Write-Host "`nChecking license headers"
Write-Host   "========================"
.\Check-LicenseHeaders.ps1

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Set-BuildVersion.ps1"
.\Set-BuildVersion.ps1

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Set-PureBuildVersion.ps1"
.\Set-PureBuildVersion.ps1

Write-Host "`nVersions patching"
Write-Host   "================="
Install-Module -Name powershell-yaml -Force

if($LastExitCode -ne 0)
{
    Exit-AppveyorBuild
}

$yaml           = Get-Content .\appveyor.yml -Raw
$appveyorConfig = ConvertFrom-Yaml $yaml
$buildVersion   = $appveyorConfig.version.Replace('-', '.').Replace("{branch}", $null).Replace("{build}", $env:APPVEYOR_BUILD_NUMBER)
$projectFiles   = Get-ChildItem -Include "*.csproj" -Recurse

foreach ($projectFile in $projectFiles)
{
    $xml = [xml](Get-Content $projectFile.FullName)

    $propertyGroup = $xml | Select-Xml -XPath "/Project/PropertyGroup[Version='1.0.0']"
    $propertyGroup = $propertyGroup.Node

    $propertyGroup.Version          = $buildVersion
    $propertyGroup.FileVersion      = $buildVersion
    $propertyGroup.AssemblyVersion  = $buildVersion

    if (!($projectFile.Name.Contains("Tests")))
    {
        $propertyGroup.PackageVersion = $env:APPVEYOR_BUILD_VERSION
    }
    
    $xml.Save($projectFile.FullName)

    if($LastExitCode -ne 0)
    {
        Exit-AppveyorBuild
    }
}

Write-Host "`nLibrary Build"
Write-Host   "============="
nuget restore
dotnet pack NotifyPropertyChangedBase\NotifyPropertyChangedBase.csproj -c Release -o $(Get-Location)
dotnet build NotifyPropertyChangedBase\NotifyPropertyChangedBase.csproj -c Release --no-incremental /p:DebugType=PdbOnly
msbuild NotifyPropertyChangedBase.Android /p:Configuration=Release /nologo /verbosity:minimal /t:pack /p:PackageOutputPath=$(Get-Location)

Write-Host "`nTests Build"
Write-Host   "==========="
dotnet build NotifyPropertyChangedBase.Tests\NotifyPropertyChangedBase.Tests.csproj -c Release

Write-Host "`nArtifacts"
Write-Host   "========="

foreach ($nuget in Get-ChildItem *.nupkg)
{
    Push-AppveyorArtifact $nuget
}

$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
	$releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

	if (!(Test-Path $releaseFolder))
	{
		throw "Invalid project release folder. `$releaseFolder: '$releaseFolder'"
        Exit-AppveyorBuild
	}

	$zipFileName = "$projectFolder.$env:APPVEYOR_BUILD_VERSION.zip"
	7z a $zipFileName "$releaseFolder\*"

	Push-AppveyorArtifact $zipFileName
}

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Deployment-Skipping.ps1"
.\Deployment-Skipping.ps1

Write-Host "`n.NET Core tests"
Write-Host   "==============="
dotnet vstest NotifyPropertyChangedBase.Tests\bin\Release\netcoreapp1.0\NotifyPropertyChangedBase.Tests.NetCore.dll /logger:trx
(New-Object "System.Net.WebClient").UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", (Resolve-Path "TestResults\*.trx"))

Write-Host "`nCodecov"
Write-Host   "======="
choco install opencover.portable codecov --no-progress

$target = "dotnet.exe"
$targetArgs = "test NotifyPropertyChangedBase.Tests\NotifyPropertyChangedBase.Tests.csproj -c Release -f net45 --no-build"
$filter = """+[NotifyPropertyChangedBase*]* -[NotifyPropertyChangedBase.Tests*]*"""
$output = "OpenCoverResults.xml"

OpenCover.Console.exe -register:user -target:$target -targetargs:$targetArgs -filter:$filter -output:$output
codecov -f $output
