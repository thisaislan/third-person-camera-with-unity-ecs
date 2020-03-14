using Unity.Properties;
using UnityEngine;

namespace Unity.Build.Common
{
    [FormerlySerializedAs("Unity.Build.Common.GraphicsSettings, Unity.Build.Common")]
    public sealed class GraphicsSettings : IBuildComponent
    {
        public ColorSpace ColorSpace = ColorSpace.Uninitialized;
    }
}
