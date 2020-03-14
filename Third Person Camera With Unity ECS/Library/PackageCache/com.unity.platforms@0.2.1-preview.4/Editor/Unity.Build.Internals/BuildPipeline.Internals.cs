using System;
using UnityEditor;

namespace Unity.Build.Internals
{
    internal static class BuildPipelineInternals
    {
        internal static event Action<BuildPipeline, BuildConfiguration> BuildStarted;
        internal static event Action<BuildPipelineResult> BuildCompleted;

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            BuildPipeline.BuildStarted += (pipeline, config) => BuildStarted?.Invoke(pipeline, config);
            BuildPipeline.BuildCompleted += (result) => BuildCompleted?.Invoke(result);
        }
    }
}
