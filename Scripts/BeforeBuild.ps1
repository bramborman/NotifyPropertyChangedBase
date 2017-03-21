if ($env:APPVEYOR_REPO_BRANCH -eq "master")
{
	$newVersion = $env:APPVEYOR_BUILD_VERSION -replace "-$env:APPVEYOR_REPO_BRANCH"
	$message    = "Build version changed from '$env:APPVEYOR_BUILD_VERSION' to '$newVersion'"

	Update-AppveyorBuild -Version $newVersion
	
	Add-AppveyorMessage $message
	Write-Host $message
}

NuGet restore