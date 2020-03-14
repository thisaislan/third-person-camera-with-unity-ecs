using UnityEditor;

namespace Unity.Build.Internals
{
    internal static class BuildContextInternals
    {
        internal static BuildConfiguration GetBuildConfiguration(BuildContext context)
        {
            return context.BuildConfiguration;
        }

        internal static string GetBuildConfigurationGUID(BuildContext context)
        {
            var assetPath = AssetDatabase.GetAssetPath(context.BuildConfiguration);
            return AssetDatabase.AssetPathToGUID(assetPath);
        }
    }
}
