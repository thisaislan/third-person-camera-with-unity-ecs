using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Unity.Build
{
    [ScriptedImporter(1, new[] { BuildConfiguration.AssetExtension
#pragma warning disable 618
        , BuildSettings.AssetExtension
#pragma warning restore 618
    })]
    sealed class BuildConfigurationScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = BuildConfiguration.CreateInstance();
            if (BuildConfiguration.DeserializeFromPath(asset, context.assetPath))
            {
                context.AddObjectToAsset("asset", asset/*, icon*/);
                context.SetMainObject(asset);

#pragma warning disable 618
                if (Path.GetExtension(context.assetPath) == BuildSettings.AssetExtension)
                {
                    Debug.LogWarning($"{context.assetPath.ToHyperLink()}: {BuildSettings.AssetExtension.SingleQuotes()} asset extension is obsolete: it has been renamed to {BuildConfiguration.AssetExtension.SingleQuotes()}. (RemovedAfter 2020-05-01)", asset);
                }
#pragma warning restore 618
            }
        }
    }
}
