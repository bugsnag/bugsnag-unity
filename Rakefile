require "open3"
require "xcodeproj"
require "rbconfig"

HOST_OS = RbConfig::CONFIG['host_os']
def is_mac?; HOST_OS =~ /darwin/i; end
def is_windows?; HOST_OS =~ /mingw|mswin|windows/i; end

##
#
# Find the directory that Unity has been installed in. This can be set via
# an ENV variable, if this has not been set then it will look in the default
# install location for both windows and mac.
#
def unity_directory
  if ENV.has_key? "UNITY_DIR"
    ENV["UNITY_DIR"]
  else
    ['/Applications/Unity', 'C:\Program Files\Unity'].find do |dir|
      File.exists? dir
    end
  end
end

##
#
# Find the Unity executable based on the unity directory.
#
def unity_executable
  [File.join(unity_directory, "Unity.app", "Contents", "MacOS", "Unity"), File.join(unity_directory, "Editor", "Unity.exe")].find do |unity|
    File.exists? unity
  end
end

unless unity_executable
  raise "Unable to locate Unity executable in #{unity_directory}"
end

def unity_dll_location
  [File.join(unity_directory, "Unity.app", "Contents", "Managed"), File.join(unity_directory, "Editor", "Data", "Managed")].find do |unity|
    File.exists? unity
  end
end

##
#
# Run a command with the unity executable and the default command line parameters
# that we apply
#
def unity(*cmd)
  cmd = cmd.unshift(unity_executable, "-force-free", "-batchmode", "-nographics", "-logFile", "unity.log", "-quit")
  sh *cmd do |ok, res|
    if !ok
      sh "cat", "unity.log"
      raise "unity error"
    end
  end
end

def current_directory
  File.dirname(__FILE__)
end

def project_path
  File.join(current_directory, "unity", "PackageProject")
end

def assets_path
  File.join(project_path, "Assets", "Plugins")
end

namespace :plugin do
  namespace :build do
    task all: [:assets, :cocoa, :csharp, :android, :webgl]
    task :clean do
      # remove any leftover artifacts from the package generation directory
      sh "git", "clean", "-dfx", "unity"
    end
    task assets: [:clean] do
      cp_r File.join(current_directory, "src", "Assets"), project_path
    end
    task cocoa: [:clean] do
      next unless is_mac?
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

      osx_dir = File.join(assets_path, "OSX", "Bugsnag")
      ios_dir = File.join(assets_path, "iOS", "Bugsnag")
      tvos_dir = File.join(assets_path, "tvOS", "Bugsnag")

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
    task csharp: [:clean] do
      if is_windows?
        env = { "UnityDir" => unity_dll_location }
        system env, "powershell", "-File", "build.ps1"
      else
        env = { "UnityDir" => unity_dll_location }
        system env, "./build.sh"
      end
      dll = File.join("src", "BugsnagUnity", "bin", "Release", "net35", "BugsnagUnity.dll")
      cp File.realpath(dll), assets_path
    end
    task android: [:clean] do
      android_dir = File.join(assets_path, "Android")

      cd "bugsnag-android" do
        sh "./gradlew", "sdk:build", "--quiet"
      end

      android_lib = File.join("bugsnag-android", "sdk", "build", "outputs", "aar", "bugsnag-android-release.aar")

      cp android_lib, android_dir
    end
    task webgl: [:clean] do
      bugsnag_js = File.realpath(File.join("bugsnag-js", "src", "bugsnag.js"))
      cd assets_path do
        webgl_file = File.join("WebGL", "bugsnag.jspre")
        cp bugsnag_js, webgl_file
      end
    end
  end

  task export: %w[plugin:build:all] do
    package_output = File.join(current_directory, "Bugsnag.unitypackage")
    rm_f package_output
    unity "-projectPath", project_path, "-exportPackage", "Assets", package_output
  end

  task maze_runner: %w[plugin:export] do
    sh "bundle", "exec", "bugsnag-maze-runner"
  end
end

namespace :example do
  namespace :build do
    task prepare: %w[plugin:export] do
      # import the built bugsnag package into the sample application
      example = File.absolute_path "example"
      package = File.absolute_path "Bugsnag.unitypackage"
      unity "-projectPath", example, "-importPackage", package

      # here we have to uncomment the lines that reference bugsnag. These are
      # commented out so that we can import the package above and have it compile
      # before the bugsnag references have been added.
      bugsnag_file = File.join(example, "Assets", "Main.cs")
      c = File.read(bugsnag_file).gsub("//", "")
      File.write(bugsnag_file, c)
    end

    task ios: %w[example:build:prepare] do
      example = File.absolute_path "example"
      unity "-projectPath", example, "-executeMethod", "Main.iOSBuild"
    end

    task android: %w[example:build:prepare] do
      example = File.absolute_path "example"
      unity "-projectPath", example, "-executeMethod", "Main.AndroidBuild"
    end

    task all: %w[example:build:ios example:build:android]
  end
end

task default: %w[plugin:maze_runner]
