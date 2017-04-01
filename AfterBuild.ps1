$buildVersion = $env:APPVEYOR_BUILD_VERSION

if (($env:APPVEYOR_REPO_BRANCH -eq "master") -and ($env:APPVEYOR_PULL_REQUEST_TITLE -eq $null) -and ($env:APPVEYOR_PULL_REQUEST_NUMBER -eq $null))
{
	$newVersion	= $buildVersion.Split("-") | Select-Object -first 1
	$message    = "Build version changed from '$buildVersion' to '$newVersion'"

	$buildVersion = $newVersion
	Update-AppveyorBuild -Version $buildVersion

	Add-AppveyorMessage $message
	Write-Host $message
}

$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
	$releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

	if (!(Test-Path $releaseFolder))
	{
		continue;
	}

	$zipFileName = "$projectFolder.$buildVersion.zip"
	7z a $zipFileName "$releaseFolder\*"
	
	Push-AppveyorArtifact $zipFileName
}

NuGet pack -Version $buildVersion
Push-AppveyorArtifact *.nupkg
