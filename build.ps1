#!/usr/bin/env pwsh
# Parse arguments. Must be the first non commented line for reasons of powershell.
param (
    [string]$configuration = "Release",
    [string]$version = "8.0.0"
)

# Define default arguments.
$CONFIGURATION = "Release"
$SOLUTION = "./BugsnagUnity.sln"
$VERSION = "8.0.0"

# Set arguments if passed
if ($args) {
    for ($i = 0; $i -lt $args.Length; $i++) {
        switch ($args[$i]) {
            '-c' { $CONFIGURATION = $args[$i + 1]; $i++ }
            '--configuration' { $CONFIGURATION = $args[$i + 1]; $i++ }
            '-v' { $VERSION = $args[$i + 1]; $i++ }
            '--version' { $VERSION = $args[$i + 1]; $i++ }
        }
    }
}

# Ensure configuration is set to provided argument value
$CONFIGURATION = $configuration
$VERSION = $version

# Restore NuGet packages
# NOTE it is necessary to call restore on every project, not just the solution!
# Builds will intermittently fail if this is not done every time.
Write-Output "Restoring NuGet packages..."
dotnet restore $SOLUTION
dotnet restore "src/BugsnagUnity/BugsnagUnity.csproj"
dotnet restore "src/BugsnagUnity/BugsnagUnity.Android.csproj"
dotnet restore "src/BugsnagUnity/BugsnagUnity.iOS.csproj"
dotnet restore "src/BugsnagUnity/BugsnagUnity.MacOS.csproj"
dotnet restore "src/BugsnagUnity/BugsnagUnity.Windows.csproj"

if ($LASTEXITCODE -ne 0) {
    Write-Output "An error occurred while restoring NuGet packages."
    exit 1
}

# Build the solution
Write-Output "Building the solution..."
dotnet build $SOLUTION -c $CONFIGURATION -p:Version=$VERSION -p:Platform="Any CPU"
if ($LASTEXITCODE -ne 0) {
    Write-Output "An error occurred while building the solution."
    exit 1
}

# Run tests
Write-Output "Running tests..."
dotnet test $SOLUTION -c $CONFIGURATION --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Output "An error occurred while running tests."
    exit 1
}

Write-Output "Build and tests completed successfully."
