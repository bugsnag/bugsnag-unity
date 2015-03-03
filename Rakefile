require 'shellwords'

$UNITY = "/Applications/Unity/Unity.app/Contents/MacOS/Unity"

desc "Build the plugin"
task :build do

  $path = Dir.pwd + "/temp.unityproject"
  sh "rm -r temp.unityproject" if File.exist? "temp.unityproject"
  sh "#$UNITY -batchmode -quit -createproject #{Shellwords.escape($path)}"

  Rake::Task[:copy_into_project].invoke

  sh "#$UNITY -batchmode -quit -projectpath #{Shellwords.escape($path)} -exportpackage Assets Bugsnag.unitypackage"
  cp $path + "/Bugsnag.unitypackage", "."

end

desc "Update the example app's C# scripts"
task :update do
  cp_r "src/Assets", Dir.pwd + "/example"
end

namespace :build do

  desc "Build and run the iOS app"
  task :ios do
    Dir.chdir "example" do
      sh "#$UNITY -batchmode -quit -executeMethod NotifyButtonScript.BuildIos"
    end
  end

  desc "Build and run the Android app"
  task :android do
    Dir.chdir "example" do
      sh "#$UNITY -batchmode -quit -executeMethod NotifyButtonScript.BuildAndroid"
    end
  end

end


desc "Update the example app's dependencies"
task :update_example_plugins do
  $path = Dir.pwd + "/example"
  Rake::Task[:copy_into_project].invoke
end

task :copy_into_project do
  unless defined?($path)
    raise "Use rake update_example, or rake build instead."
  end

  ios_dir = $path + "/Assets/Plugins/iOS"
  android_dir = $path + "/Assets/Plugins/Android"

  mkdir_p ios_dir
  mkdir_p android_dir

  # Copy unity-specific files
  cp_r "src/Assets", $path

  # Copy iOS notifier files
  `find bugsnag-cocoa/Source/Bugsnag bugsnag-cocoa/KSCrash/Source/KSCrash/Recording bugsnag-cocoa/KSCrash/Source/KSCrash/Reporting/Filters/KSCrashReportFilter.h  -name '*.m' -or -name '*.c' -or -name '*.mm' -or -name '*.h' -or -name '*.cpp'`.split("\n").each do |x|
    cp x, ios_dir
  end

  cd 'bugsnag-android' do
    sh "./gradlew clean build"
  end

  Dir["bugsnag-android/build/outputs/jar/*.jar"].each do |file|
    cp file, android_dir
  end
end

task default: [:build]
