$UNITY = ['/Applications/Unity/Unity.app/Contents/MacOS/Unity', 'C:\Program Files\Unity\Editor\Unity.exe'].find do |unity|
  File.exists? unity
end

desc "Build the plugin"
task :build do
  path = File.expand_path "temp.unityproject"
  # Ruby uses / as the file separator on all platforms, which works fine in Ruby
  # when we need to pass this to external programs such as Unity though it needs
  # to have the correct file separator. On windows ALT_SEPARATOR is defined as \
  # On the Mac it is undefined.
  if File::ALT_SEPARATOR
    path = path.tr(File::SEPARATOR, File::ALT_SEPARATOR)
  end

  rm_rf "temp.unityproject"
  sh $UNITY, "-batchmode", "-quit", "-createproject", path

  Rake::Task[:copy_into_project].invoke(path)

  # Create the package so that the metadata files are created
  sh $UNITY, "-batchmode", "-quit", "-projectpath", path, "-exportpackage", "Assets", "Bugsnag.unitypackage"

  # Add support for tvOS to the iOS files by modifying the metadata
  Rake::Task[:include_tvos_support].invoke(path)

  # Create the package with the new metadata
  sh $UNITY, "-batchmode", "-quit", "-projectpath", path, "-exportpackage", "Assets", "Bugsnag.unitypackage"

  cp "#{path}/Bugsnag.unitypackage", "."
end

desc "Update the example app's C# scripts"
task :update do
  cp_r "src/Assets", Dir.pwd + "/example"
end

task :clean do
  cd 'bugsnag-android' do
    sh "./gradlew", "clean"
  end
  cd 'bugsnag-android-unity' do
    cp "../bugsnag-android/gradle.properties", "gradle.properties"
    sh "../bugsnag-android/gradlew", "clean"
  end
  cd 'bugsnag-cocoa' do
    sh "make", "clean"
    sh "make", "BUILD_OSX=1", "clean"
  end
end

namespace :build do
  desc "Build and run the iOS app"
  task :ios do
    cd "example" do
      sh $UNITY, "-batchmode", "-quit", "-logFile", "build.log", "-executeMethod", "NotifyButtonScript.BuildIos"
    end
  end

  desc "Build and run the Android app"
  task :android do
    cd "example" do
      sh $UNITY, "-batchmode", "-quit", "-logFile", "build.log", "-executeMethod", "NotifyButtonScript.BuildAndroid"
    end
  end
end

desc "Update the example app's dependencies"
task :update_example_plugins, [:path] do |task, args|
  Rake::Task[:copy_into_project].invoke(File.expand_path("example"))
end

task :copy_into_project, [:path] do |task, args|
  # Copy unity-specific files for all plugins
  cp_r "src/Assets", args[:path]

  # Create the individual platform plugins
  Rake::Task[:create_webgl_plugin].invoke(args[:path])
  Rake::Task[:create_ios_plugin].invoke(args[:path])
  Rake::Task[:create_android_plugin].invoke(args[:path])
  Rake::Task[:create_osx_plugin].invoke(args[:path])
  Rake::Task[:create_csharp_plugin].invoke(args[:path])
end

task :create_webgl_plugin, [:path] do |task, args|
  webgl_dir = "#{args[:path]}/Assets/Plugins/WebGL"
  cp "bugsnag-js/src/bugsnag.js", File.join(webgl_dir, "bugsnag.jspre")
end

task :create_ios_plugin, [:path] do |task, args|
  # Create iOS directory
  ios_dir = "#{args[:path]}/Assets/Plugins/iOS/Bugsnag"
  rm_rf ios_dir
  mkdir_p ios_dir

  # Copy iOS bugsnag notifier and KSCrash directory files
  bugsnag_path = "bugsnag-cocoa/Source"
  kscrash_dir = "bugsnag-cocoa/Source/KSCrash/Source/KSCrash/"
  recording_path = kscrash_dir + "Recording"
  reporting_path = kscrash_dir + "Reporting"

  # Copy over basic additional KSCrash reporting files
  recording_sentry_path = kscrash_dir + "Recording/Sentry"
  recording_tools_path = kscrash_dir + "Recording/Tools"
  kscrash_filter_path = kscrash_dir + "Reporting/Filters/"

  `find #{recording_path} #{reporting_path} #{bugsnag_path} #{recording_sentry_path} #{recording_tools_path} #{kscrash_filter_path} -name '*.m' -or -name '*.c' -or -name '*.mm' -or -name '*.h' -or -name '*.cpp'`.split("\n").each do |x|
    cp x, ios_dir
  end

  # Copy unity to bugsnag-cocoa wrapper
  cp "src/BugsnagUnity.mm", ios_dir

  # Replace framework reference <Bugsnag/Bugsnag.h> with direct header file "Bugsnag.h" in the wrapper file
  wrapper_file = ios_dir + "/BugsnagUnity.mm"
  `sed -e 's/^\\(#import \\)<Bugsnag\\/\\(.*.h\\)>/\\1\"\\2\"/' -i '' #{wrapper_file}`

  # Rename any <KSCrash/*.h> framework references to the specific header files
  Dir[ios_dir + "/*.*"].each do |file|
    `sed -e 's/^\\(#import \\)<KSCrash\\/\\(.*.h\\)>/\\1\"\\2\"/' -i '' #{file}`
  end
end

task :create_android_plugin, [:path] do |task, args|
  # Create android directory
  android_dir = "#{args[:path]}/Assets/Plugins/Android"
  mkdir_p android_dir

  # Create clean build of the android notifier
  cd 'bugsnag-android' do
    sh "./gradlew sdk:build"
  end

  cd 'bugsnag-android-unity' do
    cp "../bugsnag-android/gradle.properties", "gradle.properties"
    sh "../bugsnag-android/gradlew build"
  end

  cp "bugsnag-android/sdk/build/outputs/aar/bugsnag-android-release.aar", android_dir
  cp "bugsnag-android-unity/build/outputs/aar/bugsnag-android-unity-release.aar", android_dir
end

task :create_osx_plugin, [:path] do |task, args|
  # Create OSX directory
  osx_dir = "#{args[:path]}/Assets/Plugins/OSX/Bugsnag"
  rm_rf osx_dir
  mkdir_p osx_dir

  # Create the OSX notifier framework and copy it into the Unity OSX project
  rm_rf "bugsnag-osx/bugsnag-osx/Bugsnag.framework"
  cd 'bugsnag-cocoa' do
    sh "make BUILD_OSX=1 build/Build/Products/Release/Bugsnag.framework"
    sh "cp -R build/Build/Products/Release/Bugsnag.framework ../bugsnag-osx/bugsnag-osx"
  end

  # Create the Unity OSX bundle and copy it to the OSX directory
  cd 'bugsnag-osx' do
    sh "xcodebuild -configuration Release build build | tee xcodebuild.log | xcpretty"
    cp_r "build/Release/bugsnag-osx.bundle", osx_dir
  end
end

task :include_tvos_support, [:path] do |task, args|
  tvos_dir = "#{args[:path]}/Assets/Plugins/iOS/Bugsnag"
  Dir[tvos_dir + "/*.meta"].each do |file|
    # Keep the first 11 lines, everything before plaform data
    `sed -i '' '1,11!d' #{file}`

    # Add on support for iOS and tvOS
    `echo "    iOS:\n      enabled: 1\n    tvOS:\n      enabled: 1\n" >> #{file}`
  end
end

task :create_csharp_plugin, [:path] do |task, args|
  sh "./build.sh", "--output=#{args[:path]}"
end

task default: [:build]
