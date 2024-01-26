require "open3"
require "xcodeproj"
require "rbconfig"

unless ENV['GITHUB_ACTIONS'].nil?
  require "bumpsnag"
end

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
    File.exists? unity
  end
end

def unity_dll_location
  [File.join(unity_directory, "Unity.app", "Contents", "Managed"), File.join(unity_directory, "Editor", "Data", "Managed")].find do |unity|
    File.exists? unity
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
  unless File.exists? exe
    raise "Unity not found at path '#{exe}' - set $UNITY_DIR (full path) or $UNITY_VERSION (loaded via hub) to customize"
  end
  [dir, exe]
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
  File.join(project_path, "Assets", "Bugsnag/Plugins")
end

def export_package name="Bugsnag.unitypackage"
  package_output = File.join(current_directory, name)
  rm_f package_output
  unity "-projectPath", project_path, "-exportPackage", "Assets/Bugsnag", package_output, force_free: false
end

def assemble_android filter_abis=true
  abi_filters = filter_abis ? "-PABI_FILTERS=armeabi-v7a,x86" : "-Pnoop_filters=true"
  android_dir = File.join(assets_path, "Android")

  cd "bugsnag-android" do
    sh "./gradlew", "assembleRelease", abi_filters
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
  # https://repo1.maven.org/maven2/org/jetbrains/kotlin/kotlin-stdlib/1.4.32/kotlin-stdlib-1.4.32.pom
  # The exact version number here should match the version in the EDM manifest in the BugsnagEditor.cs script and in the upm-tools/EDM/BugsnagAndroidDependencies.xml file. 
  # All should be informed by what the android notifier is using
  kotlin_stdlib = File.join("android-libs", "org.jetbrains.kotlin.kotlin-stdlib-1.4.32.jar")
  kotlin_stdlib_common = File.join("android-libs", "org.jetbrains.kotlin.kotlin-stdlib-common-1.4.32.jar")
  kotlin_annotations = File.join("android-libs", "org.jetbrains.annotations-13.0.jar")

  # copy unity lib
  unity_lib = File.join("bugsnag-android-unity", "build", "outputs", "aar", "bugsnag-android-unity-release.aar")

  cp android_core_lib, File.join(android_dir, "bugsnag-android-release.aar")
  cp ndk_lib, File.join(android_dir, "bugsnag-android-ndk-release.aar")
  cp anr_lib, File.join(android_dir, "bugsnag-plugin-android-anr-release.aar")
  cp unity_lib, File.join(android_dir, "bugsnag-android-unity-release.aar")
  mkdir File.join(android_dir, "Kotlin")
  cp kotlin_stdlib, File.join(android_dir, "Kotlin/kotlin-stdlib.jar")
  cp kotlin_stdlib_common, File.join(android_dir, "Kotlin/kotlin-stdlib-common.jar")
  cp kotlin_annotations, File.join(android_dir, "Kotlin/kotlin-annotations.jar")
end

namespace :plugin do
  namespace :build do
    cocoa_build_dir = "bugsnag-cocoa-build"
    if is_windows?
      task all: [:assets, :csharp]
      task all_android64: [:assets, :csharp]
    else
      task all: [:assets, :cocoa, :android, :csharp, ]
      task all_android64: [:assets, :cocoa, :android_64bit, :csharp ]
    end


    desc "Delete all build artifacts"
    task :clean do
      # remove any leftover artifacts from the package generation directory
      sh "git", "clean", "-dfx", "unity"
      # remove cocoa build area
      FileUtils.rm_rf cocoa_build_dir
      unless is_windows?
        # remove android build area
        cd "bugsnag-android" do
          sh "./gradlew", "clean"
        end

        cd "bugsnag-android-unity" do
          sh "./gradlew", "clean"
        end
      end
    end
    task :assets do
      cp_r File.join(current_directory, "src", "Assets"), project_path, preserve: true
    end
    task :cocoa do
      next unless is_mac?
      build_type = "Release" # "Debug" or "Release"
      FileUtils.mkdir_p cocoa_build_dir
      FileUtils.cp_r "bugsnag-cocoa/Bugsnag", cocoa_build_dir
      bugsnag_unity_file = File.realpath("BugsnagUnity.m", "src")
      public_headers = Dir.entries(File.join(cocoa_build_dir, "Bugsnag", "include", "Bugsnag"))

      cd cocoa_build_dir do
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
            .tap { |files| files << bugsnag_unity_file }
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

      osx_dir = File.join(assets_path, "OSX")

      ios_dir = File.join(assets_path, "iOS")

      tvos_dir = File.join(assets_path, "tvOS")

      #copy framework usage api file
      cp_r File.join("bugsnag-cocoa", "Bugsnag", "resources", "PrivacyInfo.xcprivacy"), ios_dir

      cd cocoa_build_dir do
        cd "build" do
          def is_fat library_path
            stdout, stderr, status = Open3.capture3("lipo", "-info", library_path)
            return !stdout.start_with?('Non-fat')
          end
          # we just need to copy the os x bundle into the correct directory

          cp_r File.join(build_type, "bugsnag-osx.bundle"), osx_dir

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
      assemble_android(true)
    end

    task :android_64bit do
      assemble_android(false)
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
        windows_dir = File.join(assets_path, "Windows")        
        cp File.realpath("BugsnagUnity.Windows.dll"), windows_dir
        cp File.realpath("BugsnagUnity.iOS.dll"), File.join(assets_path, "tvOS")
        cp File.realpath("BugsnagUnity.iOS.dll"), File.join(assets_path, "iOS")
        cp File.realpath("BugsnagUnity.MacOS.dll"), File.join(assets_path, "OSX")
        cp File.realpath("BugsnagUnity.Android.dll"), File.join(assets_path, "Android")
      end
    end


  end

  task :export_package do
    export_package
  end

  desc "Generate release artifacts"
  task export: ["plugin:build:clean"] do
    #Rake::Task["plugin:build:all"].invoke
    #export_package("Bugsnag.unitypackage")
    Rake::Task["plugin:build:all_android64"].invoke
    export_package("Bugsnag.unitypackage")
  end

  desc "Generate release artifacts from cache (using Android 64-bit)"
  task :quick_export do
    Rake::Task["plugin:build:all_android64"].invoke
    export_package("Bugsnag.unitypackage")
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
      bugsnag_file = File.join(example, "Assets", "Scripts", "Main.cs")
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
      script = File.join("features", "scripts", "build_android.sh")
      unless system env, script
        raise 'Android APK build failed'
      end
    end
  end

  namespace :edm do
    task :build do
      # Check that a Unity version has been selected and the path exists before calling the build script
      unity_path, unity = get_required_unity_paths

      # Build the Android APK
      env = { "UNITY_PATH" => File.dirname(unity) }
      script = File.join("features", "scripts", "build_edm.sh")
      unless system env, script
        raise 'EDM APK build failed'
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
      cd "features" do
        script = File.join("scripts", "generate_xcode_project.sh")
        unless system env, script
          raise 'IPA build failed'
        end
      end
    end

    task :build_xcode do
      # Build and archive from the Xcode project
      cd "features" do
        script = File.join("scripts", "build_ios.sh")
        unless system script
          raise 'IPA build failed'
        end
      end
    end

    task build: %w[test:ios:generate_xcode test:ios:build_xcode] do
    end
  end
end

namespace :dependencies do
  task :update do
    target_submodule = ENV['TARGET_SUBMODULE']
    target_version = ENV['TARGET_VERSION']

    if target_submodule.nil? || target_version.nil?
      raise 'Submodule or version targets not provided, exiting'
      exit(1)
    end

    pp "Updating submodule: #{target_submodule} to version: #{target_version}"
    updated = Bumpsnag.update_submodule(target_submodule, target_version)

    if updated
      local_info = Bumpsnag.get_git_info
      target_info = Bumpsnag.get_git_info(target_submodule)

      target_pr = local_info[:latest_pr] + 1
      origin_repo = 'https://github.com/bugsnag/bugsnag-unity'
      target_repo = target_info[:origin]

      changelog_message = "Update #{target_submodule} to [#{target_version}](#{target_repo}/releases/tag/#{target_version}) [##{target_pr}](#{origin_repo}/pull/#{target_pr})"

      Bumpsnag.add_changelog_entry(changelog_message, 'Dependencies')

      release_branch = "bumpsnag-#{target_submodule}-#{target_version}"

      commit_message = "Update #{target_submodule} to #{target_version} [full ci]"

      Bumpsnag.change_branch(release_branch, true)
      Bumpsnag.commit_changes(commit_message, [target_submodule, 'CHANGELOG.md'])
      Bumpsnag.push_changes(release_branch)

      pp 'Update complete'
    else
      pp "Nothing was updated"
    end
  end
end
