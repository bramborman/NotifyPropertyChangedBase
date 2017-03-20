$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
	$wat  = $projectFolder.ToString()
	$releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

	if (!(Test-Path $releaseFolder))
	{
		continue;
	}

	$zipFileName    = $projectFolder.ToString() + ".zip"
	$zipFilesString = $releaseFolder.ToString() + "\*"
	7z a $zipFileName $zipFilesString

	Push-AppveyorArtifact $zipFileName
}
