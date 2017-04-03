$buildVersion = $env:APPVEYOR_BUILD_VERSION

# Check whether this is commit in branch 'master' and not just PR to the branch
if (($env:APPVEYOR_REPO_BRANCH -eq "master") -and ($env:APPVEYOR_PULL_REQUEST_TITLE -eq $null) -and ($env:APPVEYOR_PULL_REQUEST_NUMBER -eq $null))
{
	$newVersion	= $buildVersion.Split("-") | Select-Object -first 1
	$message    = "Build version changed from '$buildVersion' to '$newVersion'"

	$buildVersion = $newVersion
	Update-AppveyorBuild -Version $buildVersion
	# Set the environment variable explicitly so it will be preserved to deployments (specifically GitHub Releases)
	Set-AppveyorBuildVariable "APPVEYOR_BUILD_VERSION" $buildVersion

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

nuget pack -Version $buildVersion

# Throw the exception if NuGet creating fails to make the AppVeyor build fail too
if($LastExitCode -ne 0)
{
	$host.SetShouldExit($LastExitCode)
}

Push-AppveyorArtifact *.nupkg
