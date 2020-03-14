using Unity.Properties;

namespace Unity.Build.Common
{
    /// <summary>
    /// Used for generating debugging environment for players, only usable if Unity source is available.
    /// </summary>
    [FormerlySerializedAs("Unity.Build.Common.InternalSourceBuildConfiguration, Unity.Build.Common")]
    public sealed class SourceBuildConfiguration : IBuildComponent
    {
        public bool Enabled;
    }
}
