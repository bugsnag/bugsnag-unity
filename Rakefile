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
def unity(*cmd, force_free: true, no_graphics: true)
  raise "Unable to locate Unity executable in #{unity_directory}" unless unity_executable

  cmd_prepend = [unity_executable, "-force-free", "-batchmode", "-nographics", "-logFile", "unity.log", "-quit"]
  unless force_free
    cmd_prepend = cmd_prepend - ["-force-free"]
  end
  unless no_graphics
    cmd_prepend = cmd_prepend - ["-nographics"]
  end
  cmd = cmd.unshift(*cmd_prepend)
  sh *cmd do |ok, res|
    if !ok
      puts File.read("unity.log") if File.exists?("unity.log")

      raise "unity error: #{res}"
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

def export_package name="Bugsnag.unitypackage"
  package_output = File.join(current_directory, name)
  rm_f package_output
  unity "-projectPath", project_path, "-exportPackage", "Assets", package_output, force_free: false
end

def assemble_android filter_abis=true
  abi_filters = filter_abis ? "-PABI_FILTERS=armeabi-v7a,x86" : "-Pnoop_filters=true"
  android_dir = File.join(assets_path, "Android")

  cd "bugsnag-android" do
    sh "./gradlew", "bugsnag-android-core:assembleRelease", "bugsnag-plugin-android-ndk:assembleRelease", "bugsnag-plugin-android-anr:assembleRelease", abi_filters
  end

  cd "bugsnag-android-unity" do
    sh "./gradlew", "assembleRelease", abi_filters
  end

  # copy each modularised bugsnag-android artefact
  android_core_lib = File.join("bugsnag-android", "bugsnag-android-core", "build", "outputs", "aar", "bugsnag-android-core-release.aar")
  anr_lib = File.join("bugsnag-android", "bugsnag-plugin-android-anr", "build", "outputs", "aar", "bugsnag-plugin-android-anr-release.aar")
  ndk_lib = File.join("bugsnag-android", "bugsnag-plugin-android-ndk", "build", "outputs", "aar", "bugsnag-plugin-android-ndk-release.aar")

  # copy kotlin dependencies required by bugsnag-android. the exact files required for each
  # version can be found here:
  # https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/1.3.61/kotlin-stdlib-1.3.61.pom
  kotlin_stdlib = File.join("android-libs", "kotlin-stdlib-1.3.61.jar")
  kotlin_stdlib_common = File.join("android-libs", "kotlin-stdlib-common-1.3.61.jar")
  kotlin_annotations = File.join("android-libs", "annotations-13.0.jar")

  # copy unity lib
  unity_lib = File.join("bugsnag-android-unity", "build", "outputs", "aar", "bugsnag-android-unity-release.aar")

  cp android_core_lib, File.join(android_dir, "bugsnag-android-release.aar")
  cp ndk_lib, File.join(android_dir, "bugsnag-android-ndk-release.aar")
  cp anr_lib, File.join(android_dir, "bugsnag-plugin-android-anr-release.aar")
  cp unity_lib, File.join(android_dir, "bugsnag-android-unity-release.aar")
  cp kotlin_stdlib, File.join(android_dir, "kotlin-stdlib.jar")
  cp kotlin_stdlib_common, File.join(android_dir, "kotlin-stdlib-common.jar")
  cp kotlin_annotations, File.join(android_dir, "kotlin-annotations.jar")
end

namespace :plugin do
  namespace :build do
    cocoa_build_dir = "bugsnag-cocoa-build"
    task all: [:assets, :cocoa, :csharp, :android]
    task all_android64: [:assets, :cocoa, :csharp, :android_64bit]

    desc "Delete all build artifacts"
    task :clean do
      # remove any leftover artifacts from the package generation directory
      sh "git", "clean", "-dfx", "unity"
      # remove cocoa build area
      FileUtils.rm_rf cocoa_build_dir
      # remove android build area
      cd "bugsnag-android" do
        sh "./gradlew", "clean"
      end

      cd "bugsnag-android-unity" do
        sh "./gradlew", "clean"
      end
    end
    task :assets do
      cp_r File.join(current_directory, "src", "Assets"), project_path, preserve: true
    end
    task :cocoa do
      next unless is_mac?
      build_type = "Release" # "Debug" or "Release"
      FileUtils.mkdir_p cocoa_build_dir
      FileUtils.cp_r "bugsnag-cocoa/Source", cocoa_build_dir
      bugsnag_unity_file = File.realpath("BugsnagUnity.mm", "src")
      public_headers = [
        "BugsnagMetaData.h",
        "Bugsnag.h",
        "BugsnagBreadcrumb.h",
        "BugsnagCrashReport.h",
        "BSG_KSCrashReportWriter.h",
        "BugsnagConfiguration.h",
      ]

      cd cocoa_build_dir do
        ["bugsnag-ios", "bugsnag-osx", "bugsnag-tvos"].each do |project_name|
          project_file = File.join("#{project_name}.xcodeproj")
          next if File.exist?(project_file)

          project = Xcodeproj::Project.new(project_file)

          # Create platform-specific build targets, linking deps if needed.
          # Define TARGET_OS* macros since they aren't added for static targets.
          case project_name
          when "bugsnag-tvos"
            target = project.new_target(:static_library, "bugsnag-tvos", :tvos, "9.0")
            target_macro = "-DTARGET_OS_TV"

            # Link UIKit during compilation
            phase = target.build_phases.find { |p| p.is_a?(Xcodeproj::Project::PBXFrameworksBuildPhase) }
            target.add_system_frameworks("UIKit").each do |file_ref|
              phase.add_file_reference file_ref
            end
          when "bugsnag-ios"
            target = project.new_target(:static_library, "bugsnag-ios", :ios, "8.0")
            target_macro = "-DTARGET_OS_IPHONE"

            # Link UIKit during compilation
            phase = target.build_phases.find { |p| p.is_a?(Xcodeproj::Project::PBXFrameworksBuildPhase) }
            target.add_system_frameworks("UIKit").each do |file_ref|
              phase.add_file_reference file_ref
            end
          when "bugsnag-osx"
            target = project.new_target(:bundle, "bugsnag-osx", :osx, "10.8")
            target_macro = "-DTARGET_OS_MAC"
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
              build_configuration.build_settings["OTHER_CFLAGS"] = "-fembed-bitcode-marker #{target_macro}"
            when :release
              build_configuration.build_settings["OTHER_CFLAGS"] = "-fembed-bitcode #{target_macro}"
            end
          end

          project.save
          Open3.pipeline(["xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", build_type, "build", "build"], ["xcpretty"])
          if project_name == "bugsnag-ios"
            Open3.pipeline(["xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", build_type, "-sdk", "iphonesimulator", "build", "build"], ["xcpretty"])
          end
          if project_name == "bugsnag-tvos"
            Open3.pipeline(["xcodebuild", "-project", "#{project_name}.xcodeproj", "-configuration", build_type, "-sdk", "appletvsimulator", "build", "build"], ["xcpretty"])
          end
        end
      end

      osx_dir = File.join(assets_path, "OSX", "Bugsnag")
      ios_dir = File.join(assets_path, "iOS", "Bugsnag")
      tvos_dir = File.join(assets_path, "tvOS", "Bugsnag")

      cd cocoa_build_dir do
        cd "build" do
          # we just need to copy the os x bundle into the correct directory
          cp_r File.join(build_type, "bugsnag-osx.bundle"), osx_dir

          # for ios and tvos we need to build a fat binary that includes architecture
          # slices for both the device and the simulator
          [["iphone", "ios", ios_dir], ["appletv", "tvos", tvos_dir]].each do |long_name, short_name, directory|
            library = "libbugsnag-#{short_name}.a"
            device_library = File.join("#{build_type}-#{long_name}os", library)
            simulator_library = File.join("#{build_type}-#{long_name}simulator", library)

            output_library = File.join(directory, library)
            sh "lipo", "-create", device_library, simulator_library, "-output", output_library
          end
        end
      end
    end
    task :csharp do
      if is_windows?
        env = { "UnityDir" => unity_dll_location }
        unless system env, "powershell", "-File", "build.ps1"
          raise "Failed to build csharp plugin"
        end
      else
        env = { "UnityDir" => unity_dll_location }
        unless system env, "./build.sh"
          raise "Failed to build csharp plugin"
        end
      end

      cd File.join("src", "BugsnagUnity", "bin", "Release", "net35") do
        cp File.realpath("BugsnagUnity.dll"), assets_path
        cp File.realpath("BugsnagUnity.Windows.dll"), File.join(assets_path, "Windows")
        cp File.realpath("BugsnagUnity.iOS.dll"), File.join(assets_path, "tvOS")
        cp File.realpath("BugsnagUnity.iOS.dll"), File.join(assets_path, "iOS")
        cp File.realpath("BugsnagUnity.MacOS.dll"), File.join(assets_path, "OSX")
        cp File.realpath("BugsnagUnity.Android.dll"), File.join(assets_path, "Android")
      end
    end

    task :android do
      assemble_android(true)
    end

    task :android_64bit do
      assemble_android(false)
    end
  end

  task :export_package do
    export_package
  end

  desc "Generate release artifacts"
  task export: ["plugin:build:clean"] do
    Rake::Task["plugin:build:all"].invoke
    export_package("Bugsnag.unitypackage")
    Rake::Task["plugin:build:all_android64"].invoke
    export_package("Bugsnag-with-android-64bit.unitypackage")
  end

  desc "Generate release artifacts from cache (using Android 64-bit)"
  task :quick_export do
    Rake::Task["plugin:build:all_android64"].invoke
    export_package("Bugsnag-with-android-64bit.unitypackage")
  end

  desc "Run integration tests"
  task maze_runner: %w[plugin:export] do
    sh "bundle", "exec", "bugsnag-maze-runner"
  end
end

namespace :travis do
  def with_license &block
    # Ensure a Packages/manifest.json exists in all locations
    `mkdir Packages && echo '{}' > Packages/manifest.json`
    `mkdir #{unity_directory}/Unity.app/Contents/Packages && echo '{}' > #{unity_directory}/Unity.app/Contents/Packages/manifest.json`

    # activate the unity license
    unity "-serial", ENV["UNITY_SERIAL"], "-username", ENV["UNITY_USERNAME"], "-password", ENV["UNITY_PASSWORD"], force_free: false, no_graphics: false
    sleep 10
    begin
      yield
    ensure
      unity "-returnlicense", force_free: false, no_graphics: false
      sleep 10
    end

  end

  task :export_plugin do
    with_license do
      Rake::Task["plugin:export"].invoke
    end
  end

  task :maze_runner do
    with_license do
      sh "bundle", "exec", "bugsnag-maze-runner", "--color", "--verbose"
    end
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
