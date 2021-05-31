using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace BugsnagUnity
{
    public static class PostProcessBuild
    {
        static readonly Regex ShellScriptPhaseRegex = new Regex(@"(?<uuid>[A-Z0-9]*) \/\* ShellScript \*\/ = {");
        static readonly Regex MainNativeTargetRegex = new Regex(@"[A-Z0-9]* \/\* Unity-iPhone \*\/ = {");
        static readonly Regex ShellScriptBuildPhaseRegex = new Regex(@"(?<uuid>[A-Z0-9]*) \/\* ShellScript \*\/,");

        class XcodeProjectInformation
        {
            public LinkedList<string> Lines { get; }

            public LinkedListNode<string> CurrentLine { get; private set; }

            public bool SymbolUploadScriptInstalled => SymbolUploadScriptUuid != null;

            public string SymbolUploadScriptUuid { get; set; }

            public LinkedListNode<string> SymbolUploadScriptInsertionNode { get; set; }

            public List<string> MainTargetShellScriptUuids { get; }

            public LinkedListNode<string> SymbolUploadUuidReferenceInsertionNode { get; set; }

            public XcodeProjectInformation(LinkedList<string> lines)
            {
                Lines = lines;
                CurrentLine = lines.First;
                MainTargetShellScriptUuids = new List<string>();
            }

            public void AdvanceCurrentLine()
            {
                CurrentLine = CurrentLine.Next;
            }

            public void ResetToFirstLine()
            {
                CurrentLine = Lines.First;
            }
        }

        public static void Apply(LinkedList<string> lines)
        {
            Apply(lines, Guid.NewGuid().ToString("N").Substring(0, 24).ToUpper());
        }

        /// <summary>
        /// Apply the required changes to the lines of the xcode project.
        /// </summary>
        /// <param name="lines">Lines.</param>
        /// <param name="symbolScriptUploadUuid">Symbol script upload UUID.</param>
        public static void Apply(LinkedList<string> lines, string symbolScriptUploadUuid)
        {

            var info = new XcodeProjectInformation(lines);


            if (!ProjectHasShellScriptSection(info))
            {
                CreateShellscriptBuildSection(info);
            }
            info.ResetToFirstLine();

            while (info.CurrentLine != null)
            {
                ProcessBuildConfigurationSection(info);
                ProcessShellScriptSection(info);
                ProcessNativeTargetSection(info);
                info.AdvanceCurrentLine();
            }


            // if the symbol upload script has not been added to the project file then
            // we need to insert it in the correct location and identify it with provided
            // uuid
            if (!info.SymbolUploadScriptInstalled)
            {
                info.SymbolUploadScriptUuid = symbolScriptUploadUuid;
                foreach (var line in SymbolUploadScript(info.SymbolUploadScriptUuid))
                {
                    info.Lines.AddBefore(info.SymbolUploadScriptInsertionNode, line);
                }
            }
            // if the upload script has not been added to the main target then we need
            // to add the uuid that identifies the upload script as a build phase.
            if (!info.MainTargetShellScriptUuids.Contains(info.SymbolUploadScriptUuid))
            {
                info.Lines.AddBefore(info.SymbolUploadUuidReferenceInsertionNode, string.Format(@"				{0} /* ShellScript */,", info.SymbolUploadScriptUuid));
            }
        }

        private static bool ProjectHasShellScriptSection(XcodeProjectInformation info)
        {
            while (info.CurrentLine != null)
            {
                if (info.CurrentLine.Value.Contains("PBXShellScriptBuildPhase"))
                {
                    return true;
                }
                info.AdvanceCurrentLine();
            }
            return false;
        }

        private static void CreateShellscriptBuildSection(XcodeProjectInformation info)
        {
            info.ResetToFirstLine();
            LinkedListNode<string> additionLocation = null;
            while (info.CurrentLine != null)
            {
                if (info.CurrentLine.Value.Contains("End PBXResourcesBuildPhase section"))
                {
                    additionLocation = info.CurrentLine;
                    break;
                }
                info.AdvanceCurrentLine();
            }
            foreach (var line in ShellBuildSection())
            {
                info.Lines.AddAfter(additionLocation, line);
            }
        }

        /// <summary>
        /// This provides the symbol upload script for insertion with the provided
        /// uuid to identify it.
        /// </summary>
        /// <returns>The upload script.</returns>
        /// <param name="uuid">UUID.</param>
        static IEnumerable<string> SymbolUploadScript(string uuid)
        {
            yield return string.Format(@"		{0} /* ShellScript */ = {{", uuid);
            yield return @"			isa = PBXShellScriptBuildPhase;";
            yield return @"			buildActionMask = 2147483647;";
            yield return @"			name = ""Upload dSYMs to Bugsnag"";";
            yield return @"			files = (";
            yield return @"			);";
            yield return @"			inputPaths =(";
            yield return @"			""${BUILT_PRODUCTS_DIR}/${INFOPLIST_PATH}"",";
            yield return @"			""${DWARF_DSYM_FOLDER_PATH}/${DWARF_DSYM_FILE_NAME}/Contents/Resources/DWARF/${TARGET_NAME}""";
            yield return @"			);";
            yield return @"			outputPaths = (";
            yield return @"			);";
            yield return @"			runOnlyForDeploymentPostprocessing = 0;";
            yield return @"			shellPath = ""/usr/bin/env ruby"";";
            yield return @"			shellScript = ""# bugsnag dsym upload script\nfork do\n  Process.setsid\n  STDIN.reopen(\""/dev/null\"")\n  STDOUT.reopen(\""/dev/null\"", \""a\"")\n  STDERR.reopen(\""/dev/null\"", \""a\"")\n\n  require \""shellwords\""\n\n  Dir[\""#{ENV[\""DWARF_DSYM_FOLDER_PATH\""]}/*/Contents/Resources/DWARF/*\""].each do |dsym|\n    system(\""curl -F dsym=@#{Shellwords.escape(dsym)} -F projectRoot=#{Shellwords.escape(ENV[\""PROJECT_DIR\""])} https://upload.bugsnag.com/\"")\n  end\nend\n"";";
            yield return @"		};";
        }

        static IEnumerable<string> ShellBuildSection()
        {
            yield return Environment.NewLine;
            yield return @"/* End PBXShellScriptBuildPhase section */";
            yield return @"/* Begin PBXShellScriptBuildPhase section */";
            yield return Environment.NewLine;
        }

        /// <summary>
        /// Processes the build configuration section. Here we need to enable two
        /// build flags relating to certain exception types. We also need to ensure
        /// that the -ObjC linker flag has been applied.
        /// </summary>
        /// <param name="info">Data.</param>
        static void ProcessBuildConfigurationSection(XcodeProjectInformation info)
        {
            if (info.CurrentLine.Value.Equals("/* Begin XCBuildConfiguration section */"))
            {
                while (info.CurrentLine != null)
                {
                    if (info.CurrentLine.Value.Equals("/* End XCBuildConfiguration section */"))
                    {
                        break;
                    }

                    // if these flags are not set at all should we add them? This is all
                    // that we used to do and we did not handle them not being set at all
                    if (info.CurrentLine.Value.Contains("GCC_ENABLE_OBJC_EXCEPTIONS") ||
                      info.CurrentLine.Value.Contains("GCC_ENABLE_CPP_EXCEPTIONS"))
                    {
                        info.CurrentLine.Value = info.CurrentLine.Value.Replace("NO", "YES");
                    }

                    if (info.CurrentLine.Value.EndsWith("OTHER_LDFLAGS = (", System.StringComparison.InvariantCulture))
                    {
                        bool hasLinkerFlag = false;
                        while (info.CurrentLine != null)
                        {
                            if (info.CurrentLine.Value.EndsWith(");", System.StringComparison.InvariantCulture))
                            {
                                if (!hasLinkerFlag)
                                {
                                    info.Lines.AddBefore(info.CurrentLine, "\t\t\t\t\t\"-ObjC\",");
                                }
                                break;
                            }

                            if (info.CurrentLine.Value.Contains("-ObjC"))
                            {
                                hasLinkerFlag = true;
                            }

                            info.AdvanceCurrentLine();
                        }
                    }

                    info.AdvanceCurrentLine();
                }
            }
        }

        /// <summary>
        /// Processes the shell script section. Here we need to look at all of the
        /// shell scripts that have been added to the project and determine if the
        /// symbol upload script has been added. If it has we take a note of the uuid
        /// that identifies it so that we can add it to the main target if needed.
        /// We also note the node where we need to insert the script if it is missing
        /// </summary>
        /// <param name="info">Data.</param>
        static void ProcessShellScriptSection(XcodeProjectInformation info)
        {
            if (info.CurrentLine.Value.Equals("/* Begin PBXShellScriptBuildPhase section */"))
            {
                string uuid = null;
                while (info.CurrentLine != null)
                {
                    if (info.CurrentLine.Value.Equals("/* End PBXShellScriptBuildPhase section */"))
                    {
                        // if we haven't inserted the script yet then this is where we need to insert it
                        // we may need to handle this section not existing yet
                        info.SymbolUploadScriptInsertionNode = info.CurrentLine;
                        break;
                    }

                    // the beginning of a shell script
                    if (ShellScriptPhaseRegex.IsMatch(info.CurrentLine.Value))
                    {
                        uuid = info.CurrentLine.Value;

                        while (info.CurrentLine != null && !info.CurrentLine.Value.EndsWith("};", System.StringComparison.InvariantCulture))
                        {
                            if (info.CurrentLine.Value.Contains("bugsnag dsym upload script"))
                            {
                                var match = ShellScriptPhaseRegex.Match(uuid);
                                var group = match.Groups["uuid"];
                                info.SymbolUploadScriptUuid = group.Value;
                            }

                            info.AdvanceCurrentLine();
                        }
                    }
                    else
                    {
                        info.AdvanceCurrentLine();
                    }
                }
            }
        }


      

        /// <summary>
        /// Processes the native target section. Here we need to find the main target
        /// which is the Unity application itself. We then to take a note of all of
        /// the shell scripts that have been added to the build phases for this main
        /// target. We need this so that we can determine if the symbol upload script
        /// has been added to this build phases. If it hasn't we also take a note of
        /// the node where we will need to insert the uuid later on.
        /// </summary>
        /// <param name="info">Data.</param>
        static void ProcessNativeTargetSection(XcodeProjectInformation info)
        {
            if (info.CurrentLine.Value.Equals("/* Begin PBXNativeTarget section */"))
            {
                while (info.CurrentLine != null && !info.CurrentLine.Value.Equals("/* End PBXNativeTarget section */"))
                {
                    if (MainNativeTargetRegex.IsMatch(info.CurrentLine.Value))
                    {
                        // we have found the main target, advance to the build phases
                        while (info.CurrentLine != null && !info.CurrentLine.Value.EndsWith("buildPhases = (", System.StringComparison.InvariantCulture))
                        {
                            info.AdvanceCurrentLine();
                        }

                        while (info.CurrentLine != null)
                        {
                            if (info.CurrentLine.Value.EndsWith(");", System.StringComparison.InvariantCulture))
                            {
                                info.SymbolUploadUuidReferenceInsertionNode = info.CurrentLine;
                                break;
                            }

                            if (ShellScriptBuildPhaseRegex.IsMatch(info.CurrentLine.Value))
                            {
                                var match = ShellScriptBuildPhaseRegex.Match(info.CurrentLine.Value);
                                var group = match.Groups["uuid"];
                                info.MainTargetShellScriptUuids.Add(group.Value);
                            }

                            info.AdvanceCurrentLine();
                        }
                    }

                    info.AdvanceCurrentLine();
                }
            }
        }

    }
}
