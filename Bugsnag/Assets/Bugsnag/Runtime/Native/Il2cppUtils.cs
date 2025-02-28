using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    internal class Il2cppUtils
    {

#if UNITY_ANDROID
        private const int IL2CPP_BUILD_ID_MAX_LENGTH = 40;
#endif

#if ENABLE_IL2CPP && UNITY_2023_1_OR_NEWER

        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_gchandle_get_target(IntPtr gchandle);
        private static IntPtr GHandle_Get_Target(IntPtr gchandle) => il2cpp_gchandle_get_target(gchandle);

#elif ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER

        [DllImport("__Internal")]
        private static extern IntPtr il2cpp_gchandle_get_target(int gchandle);
        private static IntPtr GHandle_Get_Target(IntPtr gchandle) => il2cpp_gchandle_get_target(gchandle.ToInt32());

#endif

#if ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER

        [DllImport("__Internal")]
        private static extern void il2cpp_free(IntPtr ptr);

        [DllImport("__Internal")]
        private static extern void il2cpp_native_stack_trace(IntPtr exc, out IntPtr addresses, out int numFrames, out IntPtr imageUUID, out IntPtr imageName);

        private static void NativeStackTrace(IntPtr exc, out IntPtr addresses, out int numFrames, out IntPtr imageUUID, out IntPtr imageName) =>
            il2cpp_native_stack_trace(exc, out addresses, out numFrames, out imageUUID, out imageName);

        private static void Free(IntPtr ptr) => il2cpp_free(ptr);

#else

        private static void Free(IntPtr ptr) {}

#endif

        #nullable enable
        private static string? ExtractString(IntPtr pString, Int32 iLimit)
        {
            return (pString == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(pString, iLimit);
        }

        private static string? ExtractString(IntPtr pString)
        {
            return (pString == IntPtr.Zero) ? null : Marshal.PtrToStringAnsi(pString);
        }

        private static Int32 FindStringTerminator(IntPtr pString, string terminator)
        {
            if (pString == IntPtr.Zero)
            {
                return 0;
            }

            var i = 0;
            var terminatorIndex = 0;
            while (true)
            {
                var b = Marshal.ReadByte(pString, i);

                if (b == 0)
                {
                    break;
                }

                // next index
                i++;

                var ch = Convert.ToChar(b);

                if (terminator[terminatorIndex] == ch)
                {
                    terminatorIndex++;

                    if (terminatorIndex == terminator.Length)
                    {
                        break;
                    }
                }
                else
                {
                    terminatorIndex = 0;
                }
            }

            return i;
        }

        internal static StackTraceLine[] ToStackFrames(System.Exception exception, Func<System.Exception, IntPtr[], String, String, StackTraceLine[]> stackTransformer)
        {
             var notFound = new StackTraceLine[0];
             if (exception == null)
             {
                 return notFound;
             }

 #if ENABLE_IL2CPP && UNITY_2021_3_OR_NEWER
             var hException = GCHandle.Alloc(exception);
             var pNativeAddresses = IntPtr.Zero;
             var pImageUuid = IntPtr.Zero;
             var pImageName = IntPtr.Zero;
             try
             {
                 if (hException == null)
                 {
                     return notFound;
                 }

                 var pException = Il2cppUtils.GHandle_Get_Target(GCHandle.ToIntPtr(hException));

                 if (pException == IntPtr.Zero)
                 {
                     return notFound;
                 }

                 var frameCount = 0;
                 string? mainImageFileName = null;
                 string? mainImageUuid = null;

                 NativeStackTrace(pException, out pNativeAddresses, out frameCount, out pImageUuid, out pImageName);
                 if (pNativeAddresses == IntPtr.Zero)
                 {
                     return notFound;
                 }

#if UNITY_ANDROID

                 // Marshal.PtrToStringAnsi without a limit is unsafe due to an off-by-one error where `strncpy` is
                 // incorrectly given `strlen` instead of `strlen + 1`, so we look for ".so" as a terminator (or NULL
                 // whichever comes first)
                 var iMainImageFileNameLimit = Il2cppUtils.FindStringTerminator(pImageName, ".so");
                 mainImageFileName = Il2cppUtils.ExtractString(pImageName, iMainImageFileNameLimit);
                 mainImageUuid = Il2cppUtils.ExtractString(pImageUuid, IL2CPP_BUILD_ID_MAX_LENGTH);

#else

                 mainImageFileName = Il2cppUtils.ExtractString(pImageName);
                 mainImageUuid = Il2cppUtils.ExtractString(pImageUuid);

#endif

                 var nativeAddresses = new IntPtr[frameCount];
                 Marshal.Copy(pNativeAddresses, nativeAddresses, 0, frameCount);

                 return stackTransformer(exception, nativeAddresses, mainImageFileName, mainImageUuid);
             }
             finally
             {
                 if (pImageUuid != IntPtr.Zero)
                 {
                     Il2cppUtils.Free(pImageUuid);
                 }
                 if (pImageName != IntPtr.Zero)
                 {
                     Il2cppUtils.Free(pImageName);
                 }
                 if (pNativeAddresses != IntPtr.Zero)
                 {
                     Il2cppUtils.Free(pNativeAddresses);
                 }
                 if (hException != null)
                 {
                     hException.Free();
                 }
             }
 #else
             return notFound;
 #endif
        }
    }
}
