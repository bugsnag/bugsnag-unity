#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Default");
var solution = File("./BugsnagUnity.sln");
var configuration = Argument("configuration", "Release");
var project = File("./src/BugsnagUnity/BugsnagUnity.csproj");
var version = "1.0.0";

Task("Restore-NuGet-Packages")
    .Does(() => NuGetRestore(solution));

Task("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .IsDependentOn("PopulateVersion")
  .Does(() => {
    MSBuild(solution, settings =>
      settings
        .SetVerbosity(Verbosity.Minimal)
        .WithProperty("Version", version)
        .SetConfiguration(configuration));
  });

Task("PopulateVersion")
  .WithCriteria(!BuildSystem.IsLocalBuild)
  .Does(() => {
    if (string.IsNullOrEmpty(TravisCI.Environment.Build.Tag))
    {
      version = $"{version}-dev-{TravisCI.Environment.Repository.Commit.Substring(0, 7)}";
    }
    else
    {
      version = TravisCI.Environment.Build.Tag.TrimStart('v');
    }
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() => {
    var assemblies = GetFiles($"./tests/**/bin/{configuration}/**/*.Tests.dll");
    NUnit3(assemblies);
  });

Task("Default")
  .IsDependentOn("Test");

RunTarget(target);
