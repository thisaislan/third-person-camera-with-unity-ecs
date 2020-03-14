using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using PropertyAttribute = Unity.Properties.PropertyAttribute;

namespace Unity.Build.Classic
{
    [FormerlySerializedAs("Unity.Build.Common.ClassicBuildProfile, Unity.Build.Common")]
    public sealed class ClassicBuildProfile : IBuildPipelineComponent
    {
        UnityEditor.BuildTarget m_Target;
        List<string> m_ExcludedAssemblies;

        /// <summary>
        /// Retrieve <see cref="BuildTypeCache"/> for this build profile.
        /// </summary>
        public BuildTypeCache TypeCache { get; } = new BuildTypeCache();

        /// <summary>
        /// Gets or sets which <see cref="BuildTarget"/> this profile is going to use for the build.
        /// Used for building classic Unity standalone players.
        /// </summary>
        ///
        [Property]
        public UnityEditor.BuildTarget Target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                TypeCache.PlatformName = m_Target.ToString();
            }
        }

        /// <summary>
        /// Gets or sets which <see cref="BuildType"/> this profile is going to use for the build.
        /// </summary>
        [Property]
        public BuildType Configuration { get; set; } = BuildType.Develop;

        [Property]
        public BuildPipeline Pipeline { get; set; }

        public int SortingIndex => (int)m_Target;

        public bool SetupEnvironment()
        {
            if (m_Target == EditorUserBuildSettings.activeBuildTarget)
                return false;
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(m_Target), m_Target))
                throw new Exception($"Failed to switch active build target to {m_Target}");
            return true;
        }

        /// <summary>
        /// List of assemblies that should be explicitly excluded for the build.
        /// </summary>
        [Property, HideInInspector]
        public List<string> ExcludedAssemblies
        {
            get => m_ExcludedAssemblies;
            set
            {
                m_ExcludedAssemblies = value;
                TypeCache.ExcludedAssemblies = value;
            }
        }

        public ClassicBuildProfile()
        {
            Target = UnityEditor.BuildTarget.NoTarget;
            ExcludedAssemblies = new List<string>();
        }

        /// <summary>
        /// Gets the extension string for the target platform.
        /// </summary>
        /// <returns>The extension string.</returns>
        public string GetExecutableExtension()
        {
#pragma warning disable CS0618
            switch (m_Target)
            {
                case UnityEditor.BuildTarget.StandaloneOSX:
                case UnityEditor.BuildTarget.StandaloneOSXIntel:
                case UnityEditor.BuildTarget.StandaloneOSXIntel64:
                    return ".app";
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return ".exe";
                case UnityEditor.BuildTarget.NoTarget:
                case UnityEditor.BuildTarget.StandaloneLinux64:
                case UnityEditor.BuildTarget.Stadia:
                    return string.Empty;
                case UnityEditor.BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        return "";
                    else if (EditorUserBuildSettings.buildAppBundle)
                        return ".aab";
                    else
                        return ".apk";
                case UnityEditor.BuildTarget.Lumin:
                    return ".mpk";
                case UnityEditor.BuildTarget.iOS:
                case UnityEditor.BuildTarget.tvOS:
                default:
                    return "";
            }
#pragma warning restore CS0618
        }
    }
}
