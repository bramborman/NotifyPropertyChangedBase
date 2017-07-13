Write-Host "`nAppVeyor-AfterBuild script executed"
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

dotnet vstest NotifyPropertyChangedBase.Tests.NetCore\bin\Release\netcoreapp1.0\NotifyPropertyChangedBase.Tests.NetCore.dll | Tee-Object -Variable testOutput
$results = $testOutput | Where-Object { $_.StartsWith("Total tests:") }
$counts = [Regex]::Matches($results, "\d+") | ForEach-Object { $_.Value }

if ($counts.Count -ne 4)
{
    throw "Total number of numbers parsed is wrong. `$counts.Count: '$($counts.Count)'"
}

$total	 = [int]$counts[0]
$passed  = [int]$counts[1]
$failed  = [int]$counts[2]
$skipped = [int]$counts[3]

# Simple check whether we've parsed the results correctly
if ($total -ne ($passed + $failed + $skipped))
{
    throw "Total number of test run was not the same as sum of test passed, failed and skipped."
}

if ($failed -ne 0)
{
    throw "$failed of unit tests failed"
}

$target = """C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe"""
$targetArgs = "/testcontainer:""NotifyPropertyChangedBase.Tests.Net45\bin\Release\NotifyPropertyChangedBase.Tests.Net45.dll"""
$filter = """+[NotifyPropertyChangedBase*]* -[NotifyPropertyChangedBase.Tests*]*"""
$output = "OpenCoverResults.xml"

choco install opencover.portable codecov
OpenCover.Console.exe -target:$target -targetargs:$targetArgs -filter:$filter -output:$output
codecov -f $output
