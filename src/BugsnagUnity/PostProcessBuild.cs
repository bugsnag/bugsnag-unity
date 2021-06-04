using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace BugsnagUnity
{
    public static class PostProcessBuild
    {
      
        class XcodeProjectInformation
        {
            public LinkedList<string> Lines { get; }

            public LinkedListNode<string> CurrentLine { get; private set; }

            public XcodeProjectInformation(LinkedList<string> lines)
            {
                Lines = lines;
                CurrentLine = lines.First;
            }

            public void AdvanceCurrentLine()
            {
                CurrentLine = CurrentLine.Next;
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

            while (info.CurrentLine != null)
            {
                ProcessBuildConfigurationSection(info);
                info.AdvanceCurrentLine();
            }            
        }
     
        /// <summary>
        /// Processes the build configuration section. Here we need to enable two
        /// build flags relating to certain exception types.
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

                    info.AdvanceCurrentLine();
                }
            }
        }
    }
}
