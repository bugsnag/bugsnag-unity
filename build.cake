#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Default");
var solution = File("./Bugsnag.Unity.sln");
var configuration = Argument("configuration", "Release");

Task("Build")
  .Does(() => {
    MSBuild(solution, settings =>
      settings
        .SetVerbosity(Verbosity.Minimal)
        .SetConfiguration(configuration));
  });

Task("Test")
  .IsDependentOn("Build")
  .Does(() => {
    var assemblies = GetFiles($"./tests/**/bin/{configuration}/**/*.Tests.dll");
    NUnit3(assemblies);
  });

Task("CopyToUnity")
  .IsDependentOn("Build")
  .Does(() => {
    CopyFileToDirectory(@".\src\Bugsnag.Unity\bin\Release\net35\Bugsnag.Unity.dll", @"C:\Users\marti\Documents\bugsnag-unity\Assets");
  });

Task("Default")
  .IsDependentOn("Test")
  .IsDependentOn("CopyToUnity");

RunTarget(target);
