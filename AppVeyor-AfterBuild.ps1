﻿Write-Host "`nAppVeyor-AfterBuild script executed"
Write-Host   "==================================="

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Set-PureBuildVersion.ps1"
.\Set-PureBuildVersion.ps1

$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
	# Skip the Shared projects
	if (($projectFolder.Name -eq "NotifyPropertyChangedBase") -or ($projectFolder.Name -eq "NotifyPropertyChangedBase.Tests"))
	{
		continue;
	}

	$releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

	if (Test-Path "$releaseFolder\netcoreapp1.0")
	{
		$releaseFolder = Join-Path $releaseFolder "\netcoreapp1.0"
	}
	elseif (Test-Path "$releaseFolder\netstandard1.0")
	{
		$releaseFolder = Join-Path $releaseFolder "\netstandard1.0"
	}

	if (!(Test-Path $releaseFolder))
	{
		throw "Invalid project release folder. `$releaseFolder: '$releaseFolder'"
	}

	$zipFileName = "$projectFolder.$env:APPVEYOR_BUILD_VERSION.zip"
	7z a $zipFileName "$releaseFolder\*"
	
	Push-AppveyorArtifact $zipFileName
}

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/NuGet-Pack.ps1"
.\NuGet-Pack.ps1

Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Deployment-Skipping.ps1"
.\Deployment-Skipping.ps1

dotnet vstest NotifyPropertyChangedBase.Tests.NetCore\bin\Release\netcoreapp1.0\NotifyPropertyChangedBase.Tests.NetCore.dll /logger:trx
(New-Object "System.Net.WebClient").UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", (Resolve-Path "TestResults\*.trx"))

$target = """C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe"""
$targetArgs = "/testcontainer:""NotifyPropertyChangedBase.Tests.Net45\bin\Release\NotifyPropertyChangedBase.Tests.Net45.dll"""
$filter = """+[NotifyPropertyChangedBase*]* -[NotifyPropertyChangedBase.Tests*]*"""
$output = "OpenCoverResults.xml"

choco install opencover.portable codecov
OpenCover.Console.exe -register:user -target:$target -targetargs:$targetArgs -filter:$filter -output:$output
codecov -f $output
