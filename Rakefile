require "xcodeproj"

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
  Rake::Task[:create_cocoa_plugins].invoke(args[:path])
  Rake::Task[:create_android_plugin].invoke(args[:path])
  Rake::Task[:create_csharp_plugin].invoke(args[:path])
end

task :create_webgl_plugin, [:path] do |task, args|
  webgl_dir = "#{args[:path]}/Assets/Plugins/WebGL"
  cp "bugsnag-js/src/bugsnag.js", File.join(webgl_dir, "bugsnag.jspre")
end

task :create_android_plugin, [:path] do |task, args|
  # Create android directory
  android_dir = "#{args[:path]}/Assets/Plugins/Android"
  mkdir_p android_dir

  # Create clean build of the android notifier
  cd 'bugsnag-android' do
    sh "./gradlew sdk:build"
  end

  cp "bugsnag-android/sdk/build/outputs/aar/bugsnag-android-release.aar", android_dir
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

  osx_dir = "#{args[:path]}/Assets/Plugins/OSX/Bugsnag"
  rm_rf osx_dir
  mkdir_p osx_dir
  ios_dir = "#{args[:path]}/Assets/Plugins/iOS/Bugsnag"
  rm_rf ios_dir
  mkdir_p ios_dir

  cd build_dir do
    cd "build" do
      cp_r File.join("Release", "bugsnag-osx.bundle"), osx_dir
      cp File.join("Release-iphoneos", "libbugsnag-ios.a"), ios_dir
    end
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
