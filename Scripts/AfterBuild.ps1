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

Add-AppveyorMessage "TEST - PR number: $env:APPVEYOR_PULL_REQUEST_NUMBER"
Add-AppveyorMessage "TEST - PR title: $env:APPVEYOR_PULL_REQUEST_TITLE"

NuGet pack -Version $env:APPVEYOR_BUILD_VERSION
Push-AppveyorArtifact *.nupkg
