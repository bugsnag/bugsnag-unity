using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface IFeatureFlagStore
    {
        void AddFeatureFlag(string name, string variant = null);
        void AddFeatureFlags(FeatureFlag[] featureFlags);
        void ClearFeatureFlag(string name);
        void ClearFeatureFlags();
    }
}
