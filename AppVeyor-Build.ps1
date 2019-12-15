Write-Host "`nLibrary Build"
Write-Host   "============="
nuget restore
dotnet pack NotifyPropertyChangedBase\NotifyPropertyChangedBase.csproj -c Release -o $(Get-Location)
dotnet build NotifyPropertyChangedBase\NotifyPropertyChangedBase.csproj -c Release --no-incremental /p:DebugType=PdbOnly
msbuild NotifyPropertyChangedBase.Android /p:Configuration=Release /nologo /verbosity:minimal /t:pack /p:PackageOutputPath=$(Get-Location)

Write-Host "`nTests Build"
Write-Host   "==========="
dotnet build NotifyPropertyChangedBase.Tests\NotifyPropertyChangedBase.Tests.csproj -c Release

Write-Host "`nArtifacts"
Write-Host   "========="

foreach ($nuget in Get-ChildItem *.nupkg)
{
    Push-AppveyorArtifact $nuget
}

$projectFolders = Get-ChildItem -Directory -Filter "NotifyPropertyChangedBase*"

foreach ($projectFolder in $projectFolders)
{
    $releaseFolder = Join-Path $projectFolder.FullName "\bin\Release"

    if (!(Test-Path $releaseFolder))
    {
        throw "Invalid project release folder. `$releaseFolder: '$releaseFolder'"
        Exit-AppveyorBuild
    }

    $zipFileName = "$projectFolder.$env:APPVEYOR_BUILD_VERSION.zip"
    7z a $zipFileName "$releaseFolder\*"

    Push-AppveyorArtifact $zipFileName
}

#Write-Host "`n.NET Core tests"
#Write-Host   "==============="
#dotnet vstest NotifyPropertyChangedBase.Tests\bin\Release\netcoreapp1.0\NotifyPropertyChangedBase.Tests.NetCore.dll /logger:trx
#(New-Object "System.Net.WebClient").UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", (Resolve-Path "TestResults\*.trx"))

#Write-Host "`nCodecov"
#Write-Host   "======="
#choco install opencover.portable codecov --no-progress

#$target = "dotnet.exe"
#$targetArgs = "test NotifyPropertyChangedBase.Tests\NotifyPropertyChangedBase.Tests.csproj -c Release -f net45 --no-build"
#$filter = """+[*]* -[NotifyPropertyChangedBase.Tests*]*"""
#$output = "OpenCoverResults.xml"

#OpenCover.Console.exe -register:user -target:$target -targetargs:$targetArgs -filter:$filter -output:$output
#codecov -f $output
