using System;
using System.Collections.Generic;

namespace BugsnagUnity
{
    public interface IMetadataEditor
    {
        void AddMetadata(string section, string key, object value);

        void AddMetadata(string section, Dictionary<string, object> metadata);

        void ClearMetadata(string section);

        void ClearMetadata(string section, string key);

        Dictionary<string, object> GetMetadata(string section);

        object GetMetadata(string section, string key);
    }
}
