using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Editor
{
    [Serializable]
    internal class BuildInstructions
    {
        [SerializeField]
        bool m_Build;
        [SerializeField]
        bool m_Run;
        [SerializeField]
        string m_BuildConfigurationGuid;

        internal bool Build
        {
            set
            {
                m_Build = value;
            }
            get
            {
                return m_Build;
            }
        }

        internal bool Run
        {
            set
            {
                m_Run = value;
            }
            get
            {
                return m_Run;
            }
        }

        internal BuildConfiguration BuildConfiguration
        {
            set
            {
                m_BuildConfigurationGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
            }
            get
            {
                return AssetDatabase.LoadAssetAtPath<BuildConfiguration>(AssetDatabase.GUIDToAssetPath(m_BuildConfigurationGuid));
            }
        }
    }

    [Serializable]
    public class BuildManager : EditorWindow
    {
        static readonly string kSettingsPath = "UserSettings/BuildManagerSettings.asset";

        [SerializeField]
        private BuildManagerTreeState m_TreeState;
        private BuildManagerTreeView m_TreeView;
        [SerializeField]
        private List<BuildInstructions> m_BuildInstructions;


        [MenuItem("Window/Build/Manager")]
        static void Init()
        {
            BuildManager window = (BuildManager)EditorWindow.GetWindow(typeof(BuildManager));
            window.titleContent = new GUIContent("Build Manager");
            window.Show();
        }

        private void OnEnable()
        {
            if (File.Exists(kSettingsPath))
            {
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(kSettingsPath), this);
            }

            if (m_BuildInstructions == null)
            {
                m_BuildInstructions = new List<BuildInstructions>();
            }

            m_TreeState = BuildManagerTreeState.CreateOrInitializeTreeState(m_TreeState);
            m_TreeView = new BuildManagerTreeView(m_TreeState, RegenerateBuildItems);
        }

        private void OnDisable()
        {
            File.WriteAllText(kSettingsPath, EditorJsonUtility.ToJson(this));
        }

        private BuildInstructions GetOrCreateBuildConfigurationProperties(BuildConfiguration config)
        {
            var props = m_BuildInstructions.FirstOrDefault(m => m.BuildConfiguration == config);
            if (props != null)
                return props;
            props = new BuildInstructions() { BuildConfiguration = config, Build = true, Run = true };
            m_BuildInstructions.Add(props);
            return props;
        }

        private void DeleteBuildConfigurationProperties(BuildConfiguration config)
        {
            for (int i = 0; i < m_BuildInstructions.Count; i++)
            {
                if (m_BuildInstructions[i].BuildConfiguration == config)
                {
                    m_BuildInstructions.RemoveAt(i);
                    return;
                }
            }
        }

        private void RefreshProperties()
        {
            var paths = AssetDatabase.FindAssets($"t:{typeof(BuildConfiguration).FullName}");
            var allSettings = paths.Select(p => (BuildConfiguration)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(p), typeof(BuildConfiguration))).ToArray();
            foreach (var s in allSettings)
            {
                if (s.GetBuildPipeline() == null)
                {
                    DeleteBuildConfigurationProperties(s);
                    continue;
                }
                GetOrCreateBuildConfigurationProperties(s);
            }
        }

        List<BuildTreeViewItem> RegenerateBuildItems()
        {
            RefreshProperties();
            var settings = new List<BuildTreeViewItem>();
            foreach (var p in m_BuildInstructions)
            {
                settings.Add(new BuildTreeViewItem(0, p));
            }

            return settings;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            var rc = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniLabel, GUILayout.ExpandHeight(true), GUILayout.MinWidth(450));
            m_TreeView.OnGUI(rc);
            if (GUILayout.Button("Batch Build"))
            {
                BuildPipeline.BuildAsync(new BuildBatchDescription()
                {
                    BuildItems = m_BuildInstructions.Where(m => m.Build).Select(m => new BuildBatchItem() { BuildConfiguration = m.BuildConfiguration }).ToArray(),
                    OnBuildCompleted = OnBuildCompleted
                });
            }
            GUILayout.EndHorizontal();
        }

        void OnBuildCompleted(BuildPipelineResult[] results)
        {
            foreach (var r in results)
            {
                var props = GetOrCreateBuildConfigurationProperties(r.BuildConfiguration);
                if (props.Run)
                {
                    var runResult = r.BuildConfiguration.Run();
                    if (runResult.Failed)
                        Debug.LogError(runResult.Message);
                }
            }
        }
    }
}
