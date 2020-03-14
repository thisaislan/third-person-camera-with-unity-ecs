using System.IO;
using UnityEditor;

namespace Unity.Build.Editor
{
    public static class BuildPipelineMenuItem
    {
        public const string k_BuildPipelineMenu = "Assets/Create/Build/";
        const string k_CreateBuildPipelineAssetEmpty = k_BuildPipelineMenu + "Empty Build Pipeline";

        [MenuItem(k_CreateBuildPipelineAssetEmpty, true)]
        static bool CreateBuildPipelineAssetValidation()
        {
            return Directory.Exists(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        [MenuItem(k_CreateBuildPipelineAssetEmpty)]
        static void CreateBuildPipelineAsset()
        {
            Selection.activeObject = CreateAssetInActiveDirectory("Empty");
        }

        public static BuildPipeline CreateAssetInActiveDirectory(string prefix, params IBuildStep[] steps)
        {
            return BuildPipeline.CreateAssetInActiveDirectory(prefix + $"{nameof(BuildPipeline)}{BuildPipeline.AssetExtension}", (pipeline) =>
            {
                pipeline.BuildSteps.AddRange(steps);
            });
        }
    }
}
