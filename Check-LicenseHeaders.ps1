# MIT License
#
# Copyright (c) 2018 Marian Dolinský
#
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation
# files (the "Software"), to deal in the Software without
# restriction, including without limitation the rights to use,
# copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following
# conditions:
#
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
# HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.

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

Write-Host "No license header issues found"
