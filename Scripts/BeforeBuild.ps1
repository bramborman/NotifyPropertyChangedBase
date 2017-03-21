if ($env:APPVEYOR_REPO_BRANCH -eq "master")
{
	$newVersion = $env:APPVEYOR_BUILD_VERSION -replace "-$env:APPVEYOR_REPO_BRANCH"
	
	# Write the message using APPVEYOR_BUILD_VERSION variable before we change it using the Update-AppveyorBuild function
	Add-AppveyorMessage "Build version changed from $env:APPVEYOR_BUILD_VERSION to $newVersion"

	Update-AppveyorBuild -Version $newVersion
}

NuGet restore