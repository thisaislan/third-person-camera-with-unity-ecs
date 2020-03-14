using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Unity.Build.Common
{
    [FormerlySerializedAs("Unity.Build.Common.SceneList, Unity.Build.Common")]
    /// <summary>
    /// Contains information about the scenes for the build.
    /// </summary>
    public sealed class SceneList : IBuildComponent
    {
        /// <summary>
        /// Info for each scene included in the build.
        /// </summary>
        public struct SceneInfo
        {
            /// <summary>
            /// The scene asset identifier.
            /// </summary>
            [Property, AssetGuid(typeof(SceneAsset))]
            public GlobalObjectId Scene { get; set; }

            /// <summary>
            /// If true, the scene is auto loaded in the player.  The first scene is always auto loaded.
            /// </summary>
            [Property]
            public bool AutoLoad { get; set; }

            /// <summary>
            /// Get scene path.
            /// </summary>
            public string Path => AssetDatabase.GUIDToAssetPath(Scene.assetGUID.ToString());
        }

        /// <summary>
        /// If selected, the scenes currently open will be returned by the GetScenesPathsToLoad & GetScenePathsForBuild.
        /// </summary>
        [Property]
        public bool BuildCurrentScene { get; set; }

#pragma warning disable 618

        //Note: due to lack of auto conversion of data when the internal type of a container changes, this is needed.
        List<SceneInfo> _internalSceneInfos = new List<SceneInfo>();
        /// <summary>
        /// The list of scene infos for the build.
        /// </summary>
        [Property]
        public List<SceneInfo> SceneInfos
        {
            get
            {
                if (_internalSceneInfos == null || _internalSceneInfos.Count == 0 && Scenes != null && Scenes.Count > 0)
                {
                    for (int i = 0; i < Scenes.Count; i++)
                        _internalSceneInfos.Add(new SceneInfo() { Scene = Scenes[i], AutoLoad = i == 0 });
                    Scenes.Clear();
                }
                return _internalSceneInfos;
            }
            set
            {
                _internalSceneInfos = value;
            }
        }
#pragma warning restore 618

        /// <summary>
        /// Old data format for scenes.  
        /// </summary>
        [Property, UnityEngine.HideInInspector, AssetGuid(typeof(SceneAsset)), Obsolete("Use SceneInfos instead.")]
        public List<GlobalObjectId> Scenes { get; set; } = new List<GlobalObjectId>();

        /// <summary>
        /// Returns all scenes marked as auto load.  The first scene is always included.
        /// </summary>
        /// <returns>The array of scene paths to atuomatically load.</returns>
        public string[] GetScenePathsToLoad()
        {
            if (BuildCurrentScene)
                return GetScenePathsForBuild();
            var initialScenes = new List<string>();
            for (int i = 0; i < SceneInfos.Count; i++)
                if (i == 0 || SceneInfos[i].AutoLoad)
                    initialScenes.Add(AssetDatabase.GUIDToAssetPath(SceneInfos[i].Scene.assetGUID.ToString()));
            return initialScenes.ToArray();
        }

        /// <summary>
        /// Gets the scene paths that will be included in the build.
        /// </summary>
        /// <returns>The array of scene paths.</returns>
        public string[] GetScenePathsForBuild()
        {
            return GetSceneInfosForBuild().Select(s => s.Path).ToArray();
        }

        /// <summary>
        /// Gets the current scene infos for the build. If BuildCurrentScene is checked, all open scenes are returned.
        /// </summary>
        /// <returns>The array of SceneInfos.</returns>
        public SceneInfo[] GetSceneInfosForBuild()
        {
            if (BuildCurrentScene)
            {
                // Build a list of the root scenes
                var rootScenes = new List<SceneInfo>();
                for (int i = 0; i != EditorSceneManager.sceneCount; i++)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);

                    if (scene.isSubScene)
                        continue;
                    if (!scene.isLoaded)
                        continue;
                    if (EditorSceneManager.IsPreviewScene(scene))
                        continue;
                    if (string.IsNullOrEmpty(scene.path))
                        continue;
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                    rootScenes.Add(new SceneInfo() { AutoLoad = true, Scene = GlobalObjectId.GetGlobalObjectIdSlow(sceneAsset) });
                }

                return rootScenes.ToArray();
            }
            else
            {
                return SceneInfos.ToArray();
            }
        }
    }
}
