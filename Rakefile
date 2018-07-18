require "open3"
require "xcodeproj"
require "rbconfig"

HOST_OS = RbConfig::CONFIG['host_os']
def is_mac?; HOST_OS =~ /darwin/i; end
def is_windows?; HOST_OS =~ /mingw|mswin|windows/i; end

$UNITY = ['/Applications/Unity/Unity.app/Contents/MacOS/Unity', 'C:\Program Files\Unity\Editor\Unity.exe'].find do |unity|
  File.exists? unity
end

def unity(*cmd)
  cmd = cmd.unshift($UNITY, "-force-free", "-batchmode", "-nographics", "-logFile", "unity.log", "-quit")
  sh *cmd do |ok, res|
    if !ok
      sh "cat", "unity.log"
      raise "unity error"
    end
  end
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
  if is_mac?
    Rake::Task[:create_cocoa_plugins].invoke(assets_path)
  end
  Rake::Task[:create_android_plugin].invoke(assets_path)
  Rake::Task[:create_csharp_plugin].invoke(assets_path)

  package_output = File.join(current_directory, "Bugsnag.unitypackage")
  rm_f package_output
  unity "-projectpath", project_path, "-exportpackage", "Assets", package_output
end

task :clean do
  cd 'bugsnag-android' do
    sh "./gradlew", "clean", "--quiet"
  end
  cd 'bugsnag-cocoa' do
    sh "make", "clean"
    sh "make", "BUILD_OSX=1", "clean"
  end
end

task :build_examples do
  # import the built bugsnag package into the sample application
  example = File.absolute_path "example"
  package = File.absolute_path "Bugsnag.unitypackage"
  unity "-projectpath", example, "-importPackage", package

  # here we have to uncomment the lines that reference bugsnag. These are
  # commented out so that we can import the package above and have it compile
  # before the bugsnag references have been added.
  bugsnag_file = File.join(example, "Assets", "Main.cs")
  c = File.read(bugsnag_file).gsub("//", "")
  File.write(bugsnag_file, c)

  unity "-projectpath", example, "-executeMethod", "Main.iOSBuild"
  unity "-projectpath", example, "-executeMethod", "Main.AndroidBuild"
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

  cd 'bugsnag-android' do
    sh "./gradlew", "sdk:build", "--quiet"
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
    ["bugsnag-ios", "bugsnag-osx", "bugsnag-tvos"].each do |project_name|
      project_file = File.join("#{project_name}.xcodeproj")
      project = Xcodeproj::Project.new(project_file)

      case project_name
      when "bugsnag-tvos"
        target = project.new_target(:static_library, "bugsnag-tvos", :tvos, "9.0")
      when "bugsnag-ios"
        target = project.new_target(:static_library, "bugsnag-ios", :ios, "9.0")
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

      project.build_configurations.each do |build_configuration|
        if ["bugsnag-ios", "bugsnag-tvos"].include? project_name
          build_configuration.build_settings["ONLY_ACTIVE_ARCH"] = "NO"
          build_configuration.build_settings["VALID_ARCHS"] = ["x86_64", "i386", "armv7", "arm64"]
        end
        case build_configuration.type
        when :debug
          build_configuration.build_settings["OTHER_CFLAGS"] = "-fembed-bitcode-marker"
        when :release
          build_configuration.build_settings["OTHER_CFLAGS"] = "-fembed-bitcode"
        end
      end

      project.save
      Open3.pipeline(["xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", "Release", "build", "build"], ["xcpretty"])
      if project_name == "bugsnag-ios"
        Open3.pipeline(["xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", "Release", "-sdk", "iphonesimulator", "build", "build"], ["xcpretty"])
      end
      if project_name == "bugsnag-tvos"
        Open3.pipeline(["xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", "Release", "-sdk", "appletvsimulator", "build", "build"], ["xcpretty"])
      end
    end
  end

  osx_dir = File.join(args[:path], "OSX", "Bugsnag")
  ios_dir = File.join(args[:path], "iOS", "Bugsnag")
  tvos_dir = File.join(args[:path], "tvOS", "Bugsnag")

  cd build_dir do
    cd "build" do
      # we just need to copy the os x bundle into the correct directory
      cp_r File.join("Release", "bugsnag-osx.bundle"), osx_dir

      # for ios and tvos we need to build a fat binary that includes architecture
      # slices for both the device and the simulator
      [["iphone", "ios", ios_dir], ["appletv", "tvos", tvos_dir]].each do |long_name, short_name, directory|
        library = "libbugsnag-#{short_name}.a"
        device_library = File.join("Release-#{long_name}os", library)
        simulator_library = File.join("Release-#{long_name}simulator", library)

        output_library = File.join(directory, library)
        sh "lipo", "-create", device_library, simulator_library, "-output", output_library
      end
    end
  end
end

task :create_csharp_plugin, [:path] do |task, args|
  if is_windows?
    sh "powershell", "-File", "build.ps1"
  else
    sh "./build.sh"
  end
  dll = File.join("src", "Bugsnag.Unity", "bin", "Release", "net35", "Bugsnag.Unity.dll")
  cp File.realpath(dll), args[:path]
end

task default: [:build]
