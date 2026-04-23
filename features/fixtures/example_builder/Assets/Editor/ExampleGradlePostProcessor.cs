using System;
using System.IO;
using System.Linq;
using UnityEditor.Android;
using UnityEngine;

public class ExampleGradlePostProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // Unity passes the module path (often .../Gradle/unityLibrary).
        // gradle.properties lives at the Gradle project root (the parent directory).
        var gradleRoot = Directory.GetParent(path)?.FullName;
        if (string.IsNullOrEmpty(gradleRoot))
        {
            return;
        }

        var gradlePropertiesPath = Path.Combine(gradleRoot, "gradle.properties");
        if (!File.Exists(gradlePropertiesPath))
        {
            return;
        }

        var lines = File.ReadAllLines(gradlePropertiesPath).ToList();

        // Ensure AndroidX is enabled, and disable Jetifier. Bugsnag is AndroidX-native and
        // Jetifier can produce transformed jars that occasionally trigger D8 issues.
        SetOrAdd(lines, "android.useAndroidX", "true");
        SetOrAdd(lines, "android.enableJetifier", "false");
        // Only enable the alternative transform option for Unity 6000+ (AGP
        // versions used by Unity 6000). Leave 2021/2022 behavior unchanged so
        // CI builds for those editors are not affected.
        var majorVersionString = Application.unityVersion.Split('.')[0];
        if (int.TryParse(majorVersionString, out var majorVersion) && majorVersion >= 6000)
        {
            SetOrAdd(lines, "android.useFullClasspathForDexingTransform", "true");

            // Remove embedded kotlin stdlib jars from the generated Gradle
            // project to avoid duplicate-class errors when AGP pulls a
            // kotlin stdlib dependency (seen as KotlinNullPointerException
            // duplicate-class errors during assemble). This only runs for
            // Unity 6000+ to avoid changing older CI behavior.
            try
            {
                // Remove commonly embedded Kotlin/JetBrains jars that can conflict
                // with Gradle-resolved dependencies (e.g. kotlin-annotations.jar,
                // kotlin-stdlib*.jar). Match several patterns to be resilient.
                var patterns = new[] { "kotlin-stdlib*.jar", "kotlin-annotations*.jar", "kotlin-*.jar" };
                foreach (var pattern in patterns)
                {
                    var embeddedJars = Directory.GetFiles(gradleRoot, pattern, SearchOption.AllDirectories);
                    foreach (var jar in embeddedJars)
                    {
                        try { File.Delete(jar); }
                        catch (Exception ex) { Debug.LogWarning($"Failed to delete embedded jar '{jar}': {ex.Message}"); }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error while scanning for embedded kotlin jars: {ex.Message}");
            }
        }
        
        // Increase JVM heap to prevent StackOverflowError in D8 (especially Unity 2021)
        SetOrAdd(lines, "org.gradle.jvmargs", "-Xmx4096m -XX:MaxMetaspaceSize=1024m -XX:+HeapDumpOnOutOfMemoryError");

        File.WriteAllLines(gradlePropertiesPath, lines);
    }

    private static void SetOrAdd(System.Collections.Generic.List<string> lines, string key, string value)
    {
        var prefix = key + "=";
        for (var index = 0; index < lines.Count; index++)
        {
            if (lines[index].StartsWith(prefix, StringComparison.Ordinal))
            {
                lines[index] = prefix + value;
                return;
            }
        }

        lines.Add(prefix + value);
    }
}
