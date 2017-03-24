Write-Host "Current directory: $PSScriptRoot"
$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
	$releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

	if (!(Test-Path $releaseFolder))
	{
		continue;
	}

	$zipFileName    = "$projectFolder.zip"
	7z a $zipFileName "$releaseFolder\*"
	
	Push-AppveyorArtifact $zipFileName
}

$nugetVersion = $env:APPVEYOR_BUILD_VERSION

if ($env:APPVEYOR_REPO_BRANCH -eq "master")
{
	$newVersion	= $nugetVersion.Split("-") | Select-Object -first 1
	$message    = "NuGet version changed from '$nugetVersion' to '$newVersion'"

	Add-AppveyorMessage $message
	Write-Host $message

	$nugetVersion = $newVersion
}

NuGet pack -Version $nugetVersion
Push-AppveyorArtifact *.nupkg
