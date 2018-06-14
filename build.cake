#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Default");
var solution = File("./Bugsnag.Unity.sln");
var configuration = Argument("configuration", "Release");
var outputPath = Argument<string>("output", null);
var nativePlatforms = new string[] { "Android" };

Task("Restore-NuGet-Packages")
    .Does(() => NuGetRestore(solution));

Task("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() => {
    MSBuild(solution, settings =>
      settings
        .SetVerbosity(Verbosity.Minimal)
        .SetConfiguration(configuration));
    foreach (var platform in nativePlatforms) {
      MSBuild(solution, settings =>
        settings
          .WithProperty("UnityNativePlatform", platform)
          .SetVerbosity(Verbosity.Minimal)
          .SetConfiguration(configuration));
    }
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() => {
    var assemblies = GetFiles($"./tests/**/bin/{configuration}/**/*.Tests.dll");
    NUnit3(assemblies);
  });

Task("CopyToUnity")
  .WithCriteria(() => outputPath != null)
  .IsDependentOn("Build")
  .Does(() => {
    CopyFileToDirectory($"./src/Bugsnag.Unity/bin/{configuration}/net35/Bugsnag.Unity.dll", $"{outputPath}/Assets/Plugins");
    foreach (var platform in nativePlatforms) {
      CopyFileToDirectory($"./src/Bugsnag.Unity/bin/{configuration}/{platform}/net35/Bugsnag.Unity.dll", $"{outputPath}/Assets/Plugins/{platform}");
    }
    CopyFileToDirectory($"./src/Assets/Standard Assets/Bugsnag/Bugsnag.cs", $"{outputPath}/Assets/Standard Assets/Bugsnag");
  });

Task("Default")
  .IsDependentOn("Test")
  .IsDependentOn("CopyToUnity");

RunTarget(target);
