Write-Host "`nAfterBuild script executed"
Write-Host   "=========================="

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
