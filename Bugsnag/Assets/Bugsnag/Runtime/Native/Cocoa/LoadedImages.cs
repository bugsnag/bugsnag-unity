#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BSG_COCOA_DEV
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace BugsnagUnity
{
    class LoadedImages
    {
        /// <summary>
        /// Refresh the list of loaded images to match what the native side currently says.
        /// </summary>
        /// <remarks>
        /// Note: You MUST call this at least once before using an instance of this class!
        /// </remarks>
        public void Refresh()
        {
            UInt64 loadedNativeImagesAt = NativeCode.bugsnag_lastChangedLoadedImages();
            if (loadedNativeImagesAt == lastLoadedNativeImagesAt)
            {
                return;
            }
            lastLoadedNativeImagesAt = loadedNativeImagesAt;

            // Ask for the current count * 2 in case new images get added between calls
            var nativeImages = new NativeLoadedImage[NativeCode.bugsnag_getLoadedImageCount() * 2];
            var count = NativeCode.bugsnag_getLoadedImages(nativeImages, (UInt64)nativeImages.LongLength);
            var images = new LoadedImage[count];
            for (UInt64 i = 0; i < count; i++)
            {
                var nativeImage = nativeImages[i];
                var uuid = new byte[16];
                Marshal.Copy(nativeImage.UuidBytes, uuid, 0, 16);
                images[i] = new LoadedImage(nativeImage.LoadAddress,
                                            nativeImage.Size,
                                            Marshal.PtrToStringAnsi(nativeImage.FileName),
                                            new Guid(uuid).ToString());
            }
            Images = images;
            // bugsnag_getLoadedImages() locks a mutex, so we must call bugsnag_unlockLoadedImages()
            NativeCode.bugsnag_unlockLoadedImages();
        }

        /// <summary>
        /// Find the native loaded image that corresponds to a native instruction address
        /// supplied by il2cpp_native_stack_trace().
        /// </summary>
        /// <param name="address">The address to find the corresponding image of</param>
        /// <returns>The corresponding image, or null</returns>
        public LoadedImage FindImageAtAddress(UInt64 address)
        {
            int idx = Array.BinarySearch(Images, address, new AddressToImageComparator());
            if (idx < 0)
            {
                return null;
            }
            return Images[idx];
        }

        /// <summary>
        /// The currently loaded images, as of the last call to Refresh().
        /// </summary>
        public LoadedImage[] Images;
        private UInt64 lastLoadedNativeImagesAt = 0;

        private class AddressToImageComparator : IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                LoadedImage image = (LoadedImage)x;
                UInt64 address = (UInt64)y;
                if (address < image.LoadAddress)
                {
                    return 1;
                }
                if (address > image.LoadAddress + image.Size)
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}
#endif