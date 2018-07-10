require "xcodeproj"

$UNITY = ['/Applications/Unity/Unity.app/Contents/MacOS/Unity', 'C:\Program Files\Unity\Editor\Unity.exe'].find do |unity|
  File.exists? unity
end

desc "Build the plugin"
task :build do
  # remove any leftover artifacts from the package generation directory
  sh "git", "clean", "-dfx", "unity"
  current_directory = File.dirname(__FILE__)
  project_path = File.join(current_directory, "unity", "PackageProject")
  assets_path = File.join(current_directory, "src", "Assets")

  # Copy unity-specific files for all plugins
  cp_r assets_path, project_path

  assets_path = File.join(project_path, "Assets", "Plugins")

  # Create the individual platform plugins
  Rake::Task[:create_webgl_plugin].invoke(assets_path)
  Rake::Task[:create_cocoa_plugins].invoke(assets_path)
  Rake::Task[:create_android_plugin].invoke(assets_path)
  Rake::Task[:create_csharp_plugin].invoke(assets_path)

  package_output = File.join(current_directory, "Bugsnag.unitypackage")
  rm_f package_output
  sh $UNITY, "-batchmode", "-quit", "-projectpath", project_path, "-exportpackage", "Assets", package_output
end

task :clean do
  cd 'bugsnag-android' do
    sh "./gradlew", "clean"
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

task :update_example_plugins, [:package_path] do |task, args|
  sh $UNITY, "-batchmode", "-quit", "-projectpath", "example", "-logFile", "build.log", "-importPackage", args[:package_path]
  cd "example" do
  end
end

task :create_webgl_plugin, [:path] do |task, args|
  bugsnag_js = File.realpath(File.join("bugsnag-js", "src", "bugsnag.js"))
  cd args[:path] do
    webgl_file = File.join("WebGL", "bugsnag.jspre")
    cp bugsnag_js, webgl_file
  end
end

task :create_android_plugin, [:path] do |task, args|
  android_dir = File.join(args[:path], "Android")

  cd "bugsnag-android" do
    sh "./gradlew sdk:build"
  end

  android_lib = File.join("bugsnag-android", "sdk", "build", "outputs", "aar", "bugsnag-android-release.aar")

  cp android_lib, android_dir
end

task :create_cocoa_plugins, [:path] do |task, args|
  build_dir = "bugsnag-cocoa-build"
  FileUtils.rm_rf build_dir
  FileUtils.mkdir_p build_dir
  FileUtils.cp_r "bugsnag-cocoa/Source", build_dir
  bugsnag_unity_file = File.realpath("BugsnagUnity.mm", "src")
  public_headers = [
    "BugsnagMetaData.h",
    "Bugsnag.h",
    "BugsnagBreadcrumb.h",
    "BugsnagCrashReport.h",
    "BSG_KSCrashReportWriter.h",
    "BugsnagConfiguration.h",
  ]

  cd build_dir do
    ["bugsnag-ios", "bugsnag-osx"].each do |project_name|
      project_file = File.join("#{project_name}.xcodeproj")
      project = Xcodeproj::Project.new(project_file)

      case project_name
      when "bugsnag-ios"
        target = project.new_target(:static_library, "bugsnag-ios", :ios)
      when "bugsnag-osx"
        target = project.new_target(:bundle, "bugsnag-osx", :osx, "10.11")
      end

      group = project.new_group("Bugsnag")

      source_files = Dir.glob(File.join("Source", "**", "*.{c,h,mm,cpp,m}"))
        .map(&File.method(:realpath))
        .tap { |files| files << bugsnag_unity_file }
        .map { |f| group.new_file(f) }

      target.add_file_references(source_files) do |build_file|
        if public_headers.include? build_file.file_ref.name
          build_file.settings = { "ATTRIBUTES" => ["Public"] }
        end
      end

      project.save
      sh "xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", "Release", "build", "build"
    end
  end

  osx_dir = File.join(args[:path], "OSX", "Bugsnag")
  ios_dir = File.join(args[:path], "iOS", "Bugsnag")

  cd build_dir do
    cd "build" do
      cp_r File.join("Release", "bugsnag-osx.bundle"), osx_dir
      cp File.join("Release-iphoneos", "libbugsnag-ios.a"), ios_dir
    end
  end
end

task :create_csharp_plugin, [:path] do |task, args|
  sh "./build.sh"
  dll = File.join("src", "Bugsnag.Unity", "bin", "Release", "net35", "Bugsnag.Unity.dll")
  cp File.realpath(dll), args[:path]
end

task default: [:build]
