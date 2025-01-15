using System;

namespace BugsnagUnity
{
    class LoadedImage
    {
        public LoadedImage(UInt64 loadAddress, UInt64 size, string fileName, string uuid, bool isMainImage)
        {
            LoadAddress = loadAddress;
            Size = size;
            FileName = fileName;
            Uuid = uuid;
            IsMainImage = isMainImage;
        }

        public readonly UInt64 LoadAddress;
        public readonly UInt64 Size;
        public readonly string FileName;
        public readonly string Uuid;
        public readonly bool IsMainImage;
    }
}
