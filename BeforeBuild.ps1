Start-FileDownload "https://raw.githubusercontent.com/bramborman/AppVeyorBuildScripts/master/Scripts/Set-BuildVersion.ps1"
.\Set-BuildVersion.ps1

Start-FileDownload "https://dist.nuget.org/win-x86-commandline/v4.1.0/nuget.exe"
.\nuget restore

if($LastExitCode -ne 0)
{
	$host.SetShouldExit($LastExitCode)
}

# Patch .NET Core and .NET Standard
Install-Module -Name powershell-yaml

$yaml           = Get-Content .\appveyor.yml -Raw
$appveyorConfig = ConvertFrom-Yaml $yaml
$buildVersion   = $appveyorConfig.assembly_info.assembly_version.Replace("{build}", $env:APPVEYOR_BUILD_NUMBER)
$csprojs        = "NotifyPropertyChangedBase.NetCore", "NotifyPropertyChangedBase.NetStandard"

foreach ($csproj in $csprojs)
{
    $xml                    = [xml](Get-Content ".\$csproj\$csproj.csproj")
    $propertyGroup          = $xml.Project.PropertyGroup
    $propertyGroup.Version  = $buildVersion
    $xml.Save($xmlPath)
}
