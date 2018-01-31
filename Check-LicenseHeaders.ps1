function GetOneLined ($Path)
{
    $content = Get-Content $Path -Encoding UTF8
    $output = ""

    foreach ($line in $content)
    {
        $output += $line
        
        if ($line -ne "")
        {
            $output += " "
        }
    }

    return $output
}


$files = Get-ChildItem -Include *.cs,*.ps1 -Exclude *AssemblyInfo.cs,*.g.cs,*.g.i.cs -Recurse
$excludedDirectories = "\\bin\\?", "\\obj\\?"

foreach ($excludedDirectory in $excludedDirectories)
{
    $files = $files | Where-Object{ $_.fullname -notmatch $excludedDirectory }
}

$license = GetOneLined "LICENSE.md"

foreach ($file in $files)
{
    $fileContent = GetOneLined $file

    if ($file.Extension -eq ".cs")
    {
        $fileContent = $fileContent.Replace("// ", "")
    }
    elseif ($file.Extension -eq ".ps1")
    {
        $fileContent = $fileContent.Replace("# ", "")
    }
    
    if (!($fileContent.StartsWith($license)))
    {
        throw "File $($file.FullName.Substring((Resolve-Path .).Path.Length)) has a license header issue on line $($i + 1)"
    
        if ($env:APPVEYOR)
        {
            Exit-AppveyorBuild
        }
    }
}
