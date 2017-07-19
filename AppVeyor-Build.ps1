Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Set-BuildVersion.ps1"
.\Set-BuildVersion.ps1

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Set-PureBuildVersion.ps1"
.\Set-PureBuildVersion.ps1

Write-Host "`nVersions patching"
Write-Host   "================="
Install-Module -Name powershell-yaml -Force

if($LastExitCode -ne 0)
{
	$host.SetShouldExit($LastExitCode)
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
        $host.SetShouldExit($LastExitCode)
    }
}

Write-Host "`nBuild"
Write-Host   "====="
dotnet restore
dotnet build -c Release -l "C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"

Write-Host "`nNuGet pack"
Write-Host   "=========="
dotnet pack NotifyPropertyChangedBase\NotifyPropertyChangedBase.csproj --no-build -c Release -o $(Get-Location)

Write-Host "`nArtifacts"
Write-Host   "========="

Push-AppveyorArtifact *.nupkg
$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
	$releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

	if (!(Test-Path $releaseFolder))
	{
		throw "Invalid project release folder. `$releaseFolder: '$releaseFolder'"
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
$target = """C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe"""
$targetArgs = "/testcontainer:""NotifyPropertyChangedBase.Tests.Net45\bin\Release\NotifyPropertyChangedBase.Tests.Net45.dll"""
$filter = """+[NotifyPropertyChangedBase*]* -[NotifyPropertyChangedBase.Tests*]*"""
$output = "OpenCoverResults.xml"

choco install opencover.portable codecov --no-progress
OpenCover.Console.exe -register:user -target:$target -targetargs:$targetArgs -filter:$filter -output:$output
codecov -f $output
