do {
    dotnet test "./NotifyPropertyChangedBase.Tests/NotifyPropertyChangedBase.Tests.csproj" --no-build
    Write-Host "`nPress R to run again or any other key to exit . . ."
} while ([System.Console]::ReadKey($true).Key -eq [System.ConsoleKey]::R)