[CmdletBinding(PositionalBinding=$false)]
param(
    [bool] $RunTests = $true,
    [string] $Prerelease = "ci",
    [string] $PullRequestNumber
)

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "  RunTests: $RunTests"
Write-Host "  Prerelease: $Prerelease"
Write-Host "  PullRequestNumber: $PullRequestNumber"
Write-Host "  dotnet --version:" (dotnet --version)

$CreatePackages = $true
$packageOutputFolder = "$PSScriptRoot\.nupkgs"
$proejctsToPack = @(
    'Blueprint.Compiler',
    'Blueprint.Core',
    'Blueprint.Api',
    'Blueprint.ApplicationInsights',
    'Blueprint.Hangfire',
    'Blueprint.NHibernate',
    'Blueprint.Notifications',
    'Blueprint.Postgres',
    'Blueprint.SqlServer',
    'Blueprint.Stackify',
    'Blueprint.StructureMap',
    'Blueprint.Testing')

$testsToRun =
    @('Blueprint.Tests')

if ($PullRequestNumber) {
    Write-Host "Building for a pull request (#$PullRequestNumber), skipping packaging." -ForegroundColor Yellow
    $CreatePackages = $false
}

Write-Host "Building solution..." -ForegroundColor "Magenta"
dotnet restore ".\Blueprint.sln" -v minimal /p:CI=true
dotnet build ".\Blueprint.sln" -c Release /p:CI=true
Write-Host "Done building." -ForegroundColor "Green"

if ($RunTests) {
    foreach ($project in $testsToRun) {
        Write-Host "Running tests: $project (all frameworks)" -ForegroundColor "Magenta"
        Push-Location ".\tests\$project"

        dotnet test -c Release --no-build --logger trx
        if ($LastExitCode -ne 0) {
            Write-Host "Error with tests, aborting build." -Foreground "Red"
            Pop-Location
            Exit 1
        }

        Write-Host "Tests passed!" -ForegroundColor "Green"
	    Pop-Location
    }
}

if ($CreatePackages) {
    New-Item -ItemType Directory -Path $packageOutputFolder -Force | Out-Null
    Write-Host "Clearing existing $packageOutputFolder..." -NoNewline
    Get-ChildItem $packageOutputFolder | Remove-Item
    Write-Host "done." -ForegroundColor "Green"

    Write-Host "Building all packages" -ForegroundColor "Green"

    foreach ($project in $proejctsToPack) {
        Write-Host "Packing $project (dotnet pack)..." -ForegroundColor "Magenta"
        dotnet pack ".\src\$project\$project.csproj" --no-build -c Release /p:PackageOutputPath=$packageOutputFolder /p:NoPackageAnalysis=true /p:CI=true /p:Prerelease=$Prerelease
        Write-Host ""
    }
}

Write-Host "Done."