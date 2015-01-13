require 'shellwords'

desc "Build the plugin"
task :build do

  path = Dir.pwd + "/temp.unityproject"

  sh "rm -r temp.unityproject" if File.exist? "temp.unityproject"
  sh "/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit -createproject #{Shellwords.escape(path)}"

  ios_dir = path + "/Assets/Plugins/iOS"
  android_dir = path + "/Assets/Plugins/Android"

  mkdir_p ios_dir
  mkdir_p android_dir

  # Copy unity-specific files
  cp_r "src/Assets", path

  # Copy iOS notifier files
  `find bugsnag-cocoa/Source/Bugsnag bugsnag-cocoa/KSCrash/Source/KSCrash/Recording bugsnag-cocoa/KSCrash/Source/KSCrash/Reporting/Filters/KSCrashReportFilter.h  -name '*.m' -or -name '*.c' -or -name '*.mm' -or -name '*.h' -or -name '*.cpp'`.split("\n").each do |x|
    cp x, ios_dir
  end

  cd 'bugsnag-android' do
    sh "./gradlew clean :build"
  end

  Dir["bugsnag-android/build/outputs/jar/*.jar"].each do |file|
    cp file, android_dir
  end

  sh "/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectpath #{Shellwords.escape(path)} -exportpackage Assets Bugsnag.unitypackage"

  cp path + "/Bugsnag.unitypackage", "."
end

task default: [:build]
