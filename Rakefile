require "open3"
require "xcodeproj"
require "rbconfig"
require 'fileutils'
require 'tmpdir'
require "json"

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

  if ENV.has_key? 'UNITY_VERSION'
    if is_mac?
      "/Applications/Unity/Hub/Editor/#{ENV['UNITY_VERSION']}"
    elsif is_windows?
      "C:\\Program Files\\Unity\\Hub\\Editor\\#{ENV['UNITY_VERSION']}"
    end
  else
    raise 'No unity version set - use $UNITY_VERSION'
  end
end

##
#
# Find the Unity executable based on the unity directory.
#
def unity_executable dir=unity_directory
  [File.join(dir, "Unity.app", "Contents", "MacOS", "Unity"),
   File.join(dir, "Editor", "Unity"),
   File.join(dir, "Editor", "Unity.exe")].find do |unity|
    File.exist? unity
  end
end


##
# Get existing unity executable path or exit with error
#
# Returns pair containing unity path and executable
def get_required_unity_paths
  dir = unity_directory
  exe = unity_executable(dir)
  raise "No unity executable found in '#{dir}'" if exe.nil?
  unless File.exist? exe
    raise "Unity not found at path '#{exe}' - set $UNITY_DIR (full path) or $UNITY_VERSION (loaded via hub) to customize"
  end
  [dir, exe]
end

#
# Run a command with the Unity executable and apply default command line parameters.
#
def unity(*cmd)
  raise "Unable to locate Unity executable in #{unity_directory}" unless unity_executable

  unity_options = [unity_executable, "-batchmode", "-logFile", "unity.log", "-quit", "-nographics"]

  full_cmd = unity_options + cmd
  sh *full_cmd do |ok, res|
    unless ok
      puts File.read("unity.log") if File.exist?("unity.log")
      raise "Unity error: #{res}"
    end
  end
end

def current_directory
  File.dirname(__FILE__)
end

def project_path
  File.join(current_directory, "Bugsnag")
end

def plugins_dir
  File.join(project_path, "Assets", "Bugsnag/Plugins")
end

def run_unit_tests
  unity "-runTests", "-projectPath", project_path, "-testPlatform", "EditMode", "-testResults", File.join(current_directory, "testResults.xml")
end

def apply_plugin_import_settings
  unity "-projectPath", project_path, "-executeMethod", "ForceImportSettings.ApplyImportSettings"
end

def export_package name="Bugsnag.unitypackage"
  package_output = File.join(current_directory, name)
  FileUtils.rm_rf package_output
  unity "-projectPath", project_path, "-exportPackage", "Assets/Bugsnag", package_output
end

def assemble_android filter_abis=true
  abi_filters = filter_abis ? "-PABI_FILTERS=armeabi-v7a,x86" : "-Pnoop_filters=true"
  android_dir = File.join(plugins_dir, "Android")
  FileUtils.mkdir_p(android_dir)
  Dir.chdir"bugsnag-android" do
    sh "./gradlew", "assembleRelease", abi_filters
  end

  Dir.chdir"bugsnag-android-unity" do
    sh "./gradlew", "assembleRelease", abi_filters
  end

  # copy each modularised bugsnag-android artefact
  android_core_lib = File.join("bugsnag-android", "bugsnag-android-core", "build", "outputs", "aar", "bugsnag-android-core-release.aar")
  anr_lib = File.join("bugsnag-android", "bugsnag-plugin-android-anr", "build", "outputs", "aar", "bugsnag-plugin-android-anr-release.aar")
  ndk_lib = File.join("bugsnag-android", "bugsnag-plugin-android-ndk", "build", "outputs", "aar", "bugsnag-plugin-android-ndk-release.aar")

  # copy kotlin dependencies required by bugsnag-android. the exact files required for each
  # version can be found here:
  # https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/1.4.32/kotlin-stdlib-1.4.32.pom
  # The exact version number here should match the version in the EDM manifest in the BugsnagEditor.cs script and in the upm-tools/EDM/BugsnagAndroidDependencies.xml file.
  # All should be informed by what the android notifier is using
  kotlin_stdlib = File.join("android-libs", "org.jetbrains.kotlin.kotlin-stdlib-1.4.32.jar")
  kotlin_stdlib_common = File.join("android-libs", "org.jetbrains.kotlin.kotlin-stdlib-common-1.4.32.jar")
  kotlin_annotations = File.join("android-libs", "org.jetbrains.annotations-13.0.jar")

  # copy unity lib
  unity_lib = File.join("bugsnag-android-unity", "build", "outputs", "aar", "bugsnag-android-unity-release.aar")
  FileUtils.cp android_core_lib, File.join(android_dir, "bugsnag-android-release.aar")
  FileUtils.cp ndk_lib, File.join(android_dir, "bugsnag-plugin-android-ndk-release.aar")
  FileUtils.cp anr_lib, File.join(android_dir, "bugsnag-plugin-android-anr-release.aar")
  FileUtils.cp unity_lib, File.join(android_dir, "bugsnag-android-unity-release.aar")
  FileUtils.mkdir File.join(android_dir, "Kotlin")
  FileUtils.cp kotlin_stdlib, File.join(android_dir, "Kotlin/kotlin-stdlib.jar")
  FileUtils.cp kotlin_stdlib_common, File.join(android_dir, "Kotlin/kotlin-stdlib-common.jar")
  FileUtils.cp kotlin_annotations, File.join(android_dir, "Kotlin/kotlin-annotations.jar")

end

def get_current_version()
  version_line = File.open("build.sh").read.lines.find { |line| line.start_with?("VERSION=\"") }.strip
  return version_line.delete_prefix("VERSION=\"").delete_suffix("\"")
end

# Generate the upm package by importing the package file
def create_upm(package_dir, upm_tools, package_file, cli_args)
  # Clone upm package
  if File.directory?(package_dir)
    FileUtils.remove_dir(package_dir)
  end
  system("git clone --recursive --depth 1 https://github.com/bugsnag/bugsnag-unity-upm.git #{package_dir}")
  
  version = get_current_version()

  # Check for the release package
  if not File.file?(package_file)
    throw "#{package_file} not found, please build the package."
  end
  puts "SDK package found"

  
  if ENV["UNITY_UPM_VERSION"] != nil and ENV["UNITY_UPM_VERSION"].length > 0 
    unity_executable = "/Applications/Unity/Hub/Editor/#{ENV["UNITY_UPM_VERSION"]}/Unity.app/Contents/MacOS"
  elsif ENV["UNITY_PATH"] != nil and ENV["UNITY_PATH"].length > 0
    unity_executable = File.join(ENV["UNITY_PATH"], "Unity")
  end

  puts "Unity executable #{unity_executable}"
  # Importing the .unitypackage into the import project
  project_path = File.join(upm_tools, "UPMImportProject")
  system("#{unity_executable} #{cli_args} -projectPath #{project_path} -ignoreCompilerErrors -importPackage #{package_file}")

  puts "Copying over the unpacked sdk files"
  import_project = File.join(project_path, "Assets", "Bugsnag", ".")

  # Copying the unpacked sdk files
  FileUtils.cp_r(import_project, package_dir)

  # Copying the package manifest, assembly definitions, and README
  ["package.json", "package.json.meta", "README.md", "README.md.meta"].each { |file| FileUtils.cp(File.join(upm_tools, file), package_dir)}
  FileUtils.cp(File.join(upm_tools, "AssemblyDefinitions", "Bugsnag.asmdef"), File.join(package_dir, "Scripts"))
  FileUtils.cp(File.join(upm_tools, "AssemblyDefinitions", "Bugsnag.asmdef.meta"), File.join(package_dir, "Scripts"))
  FileUtils.cp(File.join(upm_tools, "AssemblyDefinitions", "BugsnagEditor.asmdef"), File.join(package_dir, "Editor"))
  FileUtils.cp(File.join(upm_tools, "AssemblyDefinitions", "BugsnagEditor.asmdef.meta"), File.join(package_dir, "Editor"))
  
  # Remove EDM menu from package
  if File.file?(File.join(package_dir, "Editor", "BugsnagEditor.EDM.cs"))
    File.delete(File.join(package_dir, "Editor", "BugsnagEditor.EDM.cs"))
    File.delete(File.join(package_dir, "Editor", "BugsnagEditor.EDM.cs.meta"))
  end

  # Set version in manifest and README
  puts "Setting the version #{version} in copied manifest and readme"
  File.write(File.join(package_dir, "README.md"), File.read(File.join(package_dir, "README.md")).gsub("VERSION_STRING", "v#{version}"))
  File.write(File.join(package_dir, "package.json"), File.read(File.join(package_dir, "package.json")).gsub("VERSION_STRING", version))
end

# Generate the upm-edm4u based on the upm package_dir
def create_edm(package_dir, upm_tools, edm_package)
  if File.directory?(edm_package)
    FileUtils.remove_dir(edm_package)
  end
  system("git clone --recursive --depth 1 https://github.com/bugsnag/bugsnag-unity-upm-edm4u.git #{edm_package}")
  FileUtils.cp_r(package_dir, edm_package)

  # Remove bundled kotlin libs
  if File.directory?(File.join(edm_package, "Plugins", "Android", "Kotlin"))
    FileUtils.remove_dir(File.join(edm_package, "Plugins", "Android", "Kotlin"))
    File.delete(File.join(edm_package, "Plugins", "Android", "Kotlin.meta"))
  end

  # Copy in the EDM4U manifest
  FileUtils.cp(File.join(upm_tools, "EDM", "BugsnagAndroidDependencies.xml"), File.join(edm_package, "Editor"))
  FileUtils.cp(File.join(upm_tools, "EDM", "BugsnagAndroidDependencies.xml.meta"), File.join(edm_package, "Editor"))

  # Change the readme title to reference EDM4U
  updated_readme = File.read(File.join(edm_package, "README.md")).gsub("Bugsnag SDK for Unity", "Bugsnag SDK for Unity Including EDM4U Support").gsub("bugsnag-unity-upm.git", "bugsnag-unity-upm-edm4u.git")
  File.write(File.join(edm_package, "README.md"), updated_readme)
end

# Test the UPM package by adding the file to the dependencies of the minimalapp
def test_package(cli_args, package_dir)
  project_root = File.join(File.dirname(__FILE__), "features", "fixtures", "minimalapp")
  manifest = File.join(project_root, "Packages", "manifest.json")
  File.write(manifest, "{\"dependencies\": {\"com.bugsnag.unitynotifier\": \"file:#{package_dir}\"}}")
  system("#{unity_executable} #{cli_args} -logFile unity.log -projectPath #{project_root}")
  if $?.exitstatus == 1
    throw "Failed to import UPM"
  end
end

# Commit and tag the release
def update_package_git(package_dir)
  version = get_current_version()
  Dir.chdir(package_dir) do
    system("git add -A")
    system("git commit -m \"Release V#{version}\"")
    system("git push")
    if $?.exitstatus != 0
      throw "Cannot push."
    end
    system("git tag v#{version}")
    system("git push origin v#{version}")
    if $?.exitstatus != 0
      throw "Cannot push tag."
    end
  end
end

def build_upm_package
  assembly_info_path = File.join("Bugsnag", "Assets", "Bugsnag", "Runtime", "AssemblyInfo.cs")
  version_match = File.read(assembly_info_path).match(/AssemblyVersion\("(\d+\.\d+\.\d+)/)

  unless version_match
    raise "Could not extract version from #{assembly_info_path}"
  end

  version = version_match[1]
  script = File.join("upm", "build-upm-package.sh")
  command = "#{script} #{version}"

  unless system command
    raise 'build upm package failed'
  end
end

namespace :plugin do
  namespace :build do
    cocoa_build_dir = "bugsnag-cocoa-build"

    task native_plugins: [:cocoa, :android, :apply_plugin_settings]
    
    desc "Delete all build artifacts"
    task :clean do

      sh "git", "clean", "-dfx", "Bugsnag/Assets/Bugsnag/Plugins"

      FileUtils.rm_rf cocoa_build_dir
      unless is_windows?
        # remove android build area
        Dir.chdir "./bugsnag-android" do
          sh "./gradlew", "clean"
        end

        Dir.chdir "bugsnag-android-unity" do
          sh "./gradlew", "clean"
        end
      end
    end

    task :cocoa do
      next unless is_mac?
      build_type = "Release" # "Debug" or "Release"
      FileUtils.mkdir_p cocoa_build_dir
      FileUtils.cp_r "bugsnag-cocoa/Bugsnag", cocoa_build_dir
      bugsnag_unity_native_cocoa_file = File.realpath("BugsnagUnity.mm", "Bugsnag/NativeSrc/Cocoa")
      public_headers = Dir.entries(File.join(cocoa_build_dir, "Bugsnag", "include", "Bugsnag"))

      Dir.chdir cocoa_build_dir do
        ["bugsnag-ios", "bugsnag-osx", "bugsnag-tvos"].each do |project_name|
          project_file = File.join("#{project_name}.xcodeproj")
          next if File.exist?(project_file)

          project = Xcodeproj::Project.new(project_file)

          # Create platform-specific build targets, linking deps if needed.
          # Define TARGET_OS* macros since they aren't added for static targets.
          case project_name
          when "bugsnag-tvos"
            target = project.new_target(:static_library, "bugsnag-tvos", :tvos, "9.2")
            target_macro = "-DTARGET_OS_TV"

            # Link UIKit during compilation
            phase = target.build_phases.find { |p| p.is_a?(Xcodeproj::Project::PBXFrameworksBuildPhase) }
            target.add_system_frameworks("UIKit").each do |file_ref|
              phase.add_file_reference file_ref
            end
          when "bugsnag-ios"
            target = project.new_target(:static_library, "bugsnag-ios", :ios, "9.0")
            target_macro = "-DTARGET_OS_IPHONE"

            # Link UIKit during compilation
            phase = target.build_phases.find { |p| p.is_a?(Xcodeproj::Project::PBXFrameworksBuildPhase) }
            target.add_system_frameworks("UIKit").each do |file_ref|
              phase.add_file_reference file_ref
            end
          when "bugsnag-osx"
            target = project.new_target(:bundle, "bugsnag-osx", :osx, "10.11")
            target_macro = "-DTARGET_OS_MAC"
          end

          group = project.new_group("Bugsnag")

          source_files = Dir.glob(File.join("Bugsnag", "**", "*.{c,h,mm,cpp,m}"))
                            .map(&File.method(:realpath))
                            .tap { |files| files << bugsnag_unity_native_cocoa_file }
                            .map { |f| group.new_file(f) }

          target.add_file_references(source_files) do |build_file|
            if public_headers.include? build_file.file_ref.name
              build_file.settings = { "ATTRIBUTES" => ["Public"] }
            end
          end

          project.build_configurations.each do |build_configuration|
            build_configuration.build_settings["HEADER_SEARCH_PATHS"] = " $(SRCROOT)/Bugsnag/include/"

            build_configuration.build_settings["GENERATE_INFOPLIST_FILE"] = "YES"

            if ["bugsnag-ios", "bugsnag-tvos"].include? project_name
              build_configuration.build_settings["ONLY_ACTIVE_ARCH"] = "NO"
              build_configuration.build_settings["VALID_ARCHS"] = ["x86_64", "armv7", "arm64"]
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

      osx_dir = File.join(plugins_dir, "MacOS")

      ios_dir = File.join(plugins_dir, "iOS")

      tvos_dir = File.join(plugins_dir, "tvOS")

      # Ensure these directories exist
      [osx_dir, ios_dir, tvos_dir].each do |dir|
        FileUtils.mkdir_p(dir) unless File.directory?(dir)
      end

      #copy framework usage api file
      FileUtils.cp_r(File.join(current_directory,"bugsnag-cocoa", "Bugsnag", "resources", "PrivacyInfo.xcprivacy"), ios_dir)

      Dir.chdir cocoa_build_dir do
        Dir.chdir "build" do
          def is_fat library_path
            stdout, stderr, status = Open3.capture3("lipo", "-info", library_path)
            return !stdout.start_with?('Non-fat')
          end
          # we just need to copy the os x bundle into the correct directory

          FileUtils.cp_r(File.join(build_type, "bugsnag-osx.bundle"), osx_dir)

          # for ios and tvos we need to build a fat binary that includes architecture
          # slices for both the device and the simulator
          [["iphone", "ios", ios_dir], ["appletv", "tvos", tvos_dir]].each do |long_name, short_name, directory|
            library = "libbugsnag-#{short_name}.a"
            device_library = File.join("#{build_type}-#{long_name}os", library)
            simulator_dir = "#{build_type}-#{long_name}simulator"
            simulator_library = File.join(simulator_dir, library)
            simulator_x64 = File.join(simulator_dir, "libBugsnagStatic-x64.a")
            output_library = File.join(directory, library)

            if is_fat simulator_library
              sh "lipo", "-extract", "x86_64", simulator_library, "-output", simulator_x64
            else
              simulator_x64 = simulator_library
            end

            sh "lipo", "-create", device_library, simulator_x64, "-output", output_library

          end
        end
      end
    end

    task :android do
      assemble_android(false)
    end

    task :apply_plugin_settings do
      apply_plugin_import_settings
    end
  end

  desc "Generate release artifacts"
  task export: ["plugin:build:clean"] do
    Rake::Task["plugin:build:native_plugins"].invoke unless is_windows?
    run_unit_tests
    export_package("Bugsnag.unitypackage")
    build_upm_package
  end
end

namespace :test do
  namespace :android do
    task :build do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Prepare the test fixture project by importing the plugins
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "prepare_fixture.sh")
      unless system env, script
        raise 'Preparation of test fixture failed'
      end

      # Build the Android APK
      script = File.join("features", "scripts", "build_android.sh release")
      unless system env, script
        raise 'Android APK build failed'
      end
    end

    task :build_dev do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Prepare the test fixture project by importing the plugins
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "prepare_fixture.sh")
      unless system env, script
        raise 'Preparation of test fixture failed'
      end

      # Build the Android APK
      script = File.join("features", "scripts", "build_android.sh dev")
      unless system env, script
        raise 'Android APK build failed'
      end
    end
  end
 

  namespace :ios do
    task :generate_xcode do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Prepare the test fixture project by importing the plugins
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "prepare_fixture.sh")
      unless system env, script
        raise 'Preparation of test fixture failed'
      end

      # Generate the Xcode project
      Dir.chdir"features" do
        script = File.join("scripts", "generate_xcode_project.sh release")
        unless system env, script
          raise 'IPA build failed'
        end
      end
    end

    task :generate_xcode_dev do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Prepare the test fixture project by importing the plugins
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "prepare_fixture.sh")
      unless system env, script
        raise 'Preparation of test fixture failed'
      end

      # Generate the Xcode project
      Dir.chdir"features" do
        script = File.join("scripts", "generate_xcode_project.sh dev")
        unless system env, script
          raise 'IPA build failed'
        end
      end
    end

    task :build_xcode do
      # Build and archive from the Xcode project
      Dir.chdir"features" do
        script = File.join("scripts", "build_ios.sh release")
        unless system script
          raise 'IPA build failed'
        end
      end
    end

    task :build_xcode_dev do
      # Build and archive from the Xcode project
      Dir.chdir"features" do
        script = File.join("scripts", "build_ios.sh dev")
        unless system script
          raise 'IPA build failed'
        end
      end
    end

    task build: %w[test:ios:generate_xcode test:ios:build_xcode] do
    end
  end
end 