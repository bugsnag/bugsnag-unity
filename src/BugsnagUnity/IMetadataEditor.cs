using System;
using System.Collections.Generic;

namespace BugsnagUnity
{
    public interface IMetadataEditor
    {
        void AddMetadata(string section, string key, object value);

        void AddMetadata(string section, IDictionary<string, object> metadata);

        void ClearMetadata(string section);

        void ClearMetadata(string section, string key);

        IDictionary<string, object> GetMetadata(string section);

        object GetMetadata(string section, string key);
    }
}
