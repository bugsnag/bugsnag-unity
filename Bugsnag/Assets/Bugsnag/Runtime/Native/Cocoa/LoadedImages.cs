﻿#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BSG_COCOA_DEV
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace BugsnagUnity
{
    /// <summary>
    /// Caches the list of loaded images as reported by the native runtime, and provides searching by address.
    /// </summary>
    class LoadedImages
    {
        /// <summary>
        /// Refresh the list of loaded images to match what the native side currently says.
        /// </summary>
        /// <param name="mainImageFileName">The file name of the main image file</param>
        /// <remarks>
        /// Note: If anything goes wrong during the refresh, the currently cached state won't change.
        /// </remarks>
        #nullable enable
        public void Refresh(String? mainImageFileName)
        {
            UInt64 loadedNativeImagesAt = NativeCode.bugsnag_lastChangedLoadedImages();
            if (loadedNativeImagesAt == LastLoadedNativeImagesAt)
            {
                // Only refresh if something changed.
                return;
            }

            var imageCount = NativeCode.bugsnag_getLoadedImageCount();
            if (imageCount == 0)
            {
                return;
            }

            // Ask for the current count * 2 in case new images get added between calls
            var nativeImages = new NativeLoadedImage[imageCount * 2];
            var count = NativeCode.bugsnag_getLoadedImages(nativeImages, (UInt64)nativeImages.LongLength);
            try
            {
                if (count == 0)
                {
                    return;
                }

                UInt64 mainLoadAddress = 0;
                var images = new LoadedImage[count];
                for (UInt64 i = 0; i < count; i++)
                {
                    var nativeImage = nativeImages[i];
                    var uuid = new byte[16];
                    Marshal.Copy(nativeImage.UuidBytes, uuid, 0, 16);
                    var fileName = Marshal.PtrToStringAnsi(nativeImage.FileName);
                    var isMainImage = fileName == mainImageFileName;

                    var image = new LoadedImage(nativeImage.LoadAddress,
                                                nativeImage.Size,
                                                fileName,
                                                new Guid(uuid).ToString(),
                                                isMainImage);
                    if (isMainImage)
                    {
                        mainLoadAddress = image.LoadAddress;
                    }
                    images[i] = image;
                }

                // Update cache
                Images = images;
                MainImageLoadAddress = mainLoadAddress;
                LowestImageLoadAddress = images[0].LoadAddress;
                LastLoadedNativeImagesAt = loadedNativeImagesAt;
            }
            finally
            {
                // bugsnag_getLoadedImages() locks a mutex, so we must call bugsnag_unlockLoadedImages()
                NativeCode.bugsnag_unlockLoadedImages();
            }
        }

        /// <summary>
        /// Find the native loaded image that corresponds to a native instruction address
        /// supplied by il2cpp_native_stack_trace().
        /// </summary>
        /// <param name="address">The address to find the corresponding image of</param>
        /// <returns>The corresponding image, or null</returns>
        #nullable enable
        public LoadedImage? FindImageAtAddress(UInt64 address)
        {
            if (address < LowestImageLoadAddress)
            {
                address += MainImageLoadAddress;
            }

            int idx = Array.BinarySearch(Images, address, new AddressToImageComparator());
            if (idx < 0)
            {
                return null;
            }
            return Images[idx];
        }

        private LoadedImage[] Images = new LoadedImage[0];
        private UInt64 MainImageLoadAddress = 0;
        private UInt64 LowestImageLoadAddress = 0;
        private UInt64 LastLoadedNativeImagesAt = 0;

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