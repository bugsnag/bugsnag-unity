#!/usr/bin/env bash

# Define default arguments.
CONFIGURATION="Release"
SOLUTION="./BugsnagUnity.sln"
VERSION="8.1.0"

# Parse arguments.
for i in "$@"; do
    case $1 in
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--version) VERSION="$2"; shift ;;
        *) ;;
    esac
    shift
done

# Restore NuGet packages
# NOTE it is necessary to call restore on every project, not just the solution!
# Builds will intermitently fail if this is not done every time.
echo "Restoring NuGet packages..."
dotnet restore "$SOLUTION"
dotnet restore src/BugsnagUnity/BugsnagUnity.csproj
dotnet restore src/BugsnagUnity/BugsnagUnity.Android.csproj 
dotnet restore src/BugsnagUnity/BugsnagUnity.iOS.csproj 
dotnet restore src/BugsnagUnity/BugsnagUnity.MacOS.csproj 
dotnet restore src/BugsnagUnity/BugsnagUnity.Windows.csproj 

if [ $? -ne 0 ]; then
    echo "An error occurred while restoring NuGet packages."
    exit 1
fi

# Build the solution
echo "Building the solution..."
dotnet build "$SOLUTION" -c "$CONFIGURATION" /p:Version="$VERSION"
if [ $? -ne 0 ]; then
    echo "An error occurred while building the solution."
    exit 1
fi

# Run tests
echo "Running tests..."
dotnet test "$SOLUTION" -c "$CONFIGURATION" --no-build
if [ $? -ne 0 ]; then
    echo "An error occurred while running tests."
    exit 1
fi

echo "Build and tests completed successfully."
