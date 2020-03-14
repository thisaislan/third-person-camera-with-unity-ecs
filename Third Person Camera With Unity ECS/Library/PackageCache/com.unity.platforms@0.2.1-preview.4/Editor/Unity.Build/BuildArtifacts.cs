using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// API for managing build artifacts.
    /// </summary>
    public static class BuildArtifacts
    {
        static readonly Dictionary<string, ArtifactData> s_ArtifactDataCache = new Dictionary<string, ArtifactData>();
        internal static string BaseDirectory => "Library/BuildArtifacts";

        class ArtifactData
        {
            public BuildPipelineResult Result;
            public IBuildArtifact[] Artifacts;
        }

        /// <summary>
        /// Get the value of the first <see cref="IBuildArtifact"/> that is assignable to type <see cref="Type"/>.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> that was used to store the <see cref="IBuildArtifact"/>.</param>
        /// <param name="type">The type of the <see cref="IBuildArtifact"/>.</param>
        /// <returns>The <see cref="IBuildArtifact"/> if found, <see langword="null"/> otherwise.</returns>
        public static IBuildArtifact GetBuildArtifact(BuildConfiguration config, Type type)
        {
            if (config == null || !config)
            {
                return null;
            }

            if (type == null || type == typeof(object))
            {
                return null;
            }

            if (!typeof(IBuildArtifact).IsAssignableFrom(type))
            {
                return null;
            }

            var artifactData = GetArtifactData(config);
            if (artifactData == null || artifactData.Artifacts == null || artifactData.Artifacts.Length == 0)
            {
                return null;
            }

            return artifactData.Artifacts.FirstOrDefault(a => type.IsAssignableFrom(a.GetType()));
        }

        /// <summary>
        /// Get the value of the first <see cref="IBuildArtifact"/> that is assignable to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IBuildArtifact"/>.</typeparam>
        /// <param name="config">The <see cref="BuildConfiguration"/> that was used to store the <see cref="IBuildArtifact"/>.</param>
        /// <returns>The <see cref="IBuildArtifact"/> if found, <see langword="null"/> otherwise.</returns>
        public static T GetBuildArtifact<T>(BuildConfiguration config) where T : class, IBuildArtifact => (T)GetBuildArtifact(config, typeof(T));

        /// <summary>
        /// Get the last <see cref="BuildPipelineResult"/> from building the <see cref="BuildConfiguration"/> specified.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> that was used to store the <see cref="IBuildArtifact"/>.</param>
        /// <returns>The <see cref="BuildPipelineResult"/> if found, <see langword="null"/> otherwise.</returns>
        public static BuildPipelineResult GetBuildResult(BuildConfiguration config) => GetArtifactData(config)?.Result;

        internal static void Store(BuildPipelineResult result, IBuildArtifact[] artifacts) => SetArtifactData(result, artifacts);

        internal static string GetArtifactPath(BuildConfiguration config) => GetArtifactsPath(GetBuildConfigurationName(config));

        internal static void Clear() => s_ArtifactDataCache.Clear();

        static string GetBuildConfigurationName(BuildConfiguration config)
        {
            var name = config.name;
            if (string.IsNullOrEmpty(name))
            {
                name = GlobalObjectId.GetGlobalObjectIdSlow(config).ToString();
            }
            return name;
        }

        static string GetArtifactsPath(string name) => Path.Combine(BaseDirectory, name + ".json").ToForwardSlash();

        static ArtifactData GetArtifactData(BuildConfiguration config)
        {
            if (config == null)
            {
                return null;
            }

            var name = GetBuildConfigurationName(config);
            var assetPath = GetArtifactsPath(name);
            if (!File.Exists(assetPath))
            {
                if (s_ArtifactDataCache.ContainsKey(name))
                {
                    s_ArtifactDataCache.Remove(name);
                }
                return null;
            }

            if (!s_ArtifactDataCache.TryGetValue(name, out var artifactData))
            {
                try
                {
                    artifactData = new ArtifactData();
                    using (var result = JsonSerialization.DeserializeFromPath(assetPath, ref artifactData))
                    {
                        if (!result.Succeeded)
                        {
                            var errors = result.AllEvents.Select(e => e.ToString());
                            LogDeserializeError(string.Join("\n", errors), artifactData, assetPath);
                            artifactData = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogDeserializeError(e.Message, artifactData, assetPath);
                    artifactData = null;
                }

                s_ArtifactDataCache.Add(name, artifactData);
            }

            return artifactData;
        }

        static void LogDeserializeError(string message, ArtifactData container, string assetPath)
        {
            var what = !string.IsNullOrEmpty(assetPath) ? assetPath.ToHyperLink() : $"memory container of type '{container.GetType().FullName}'";
            Debug.LogError($"Failed to deserialize {what}:\n{message}");
        }

        static void SetArtifactData(BuildPipelineResult result, IBuildArtifact[] artifacts)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.BuildConfiguration == null)
            {
                throw new ArgumentNullException(nameof(result.BuildConfiguration));
            }

            if (artifacts == null)
            {
                throw new ArgumentNullException(nameof(artifacts));
            }

            var name = GetBuildConfigurationName(result.BuildConfiguration);
            if (!s_ArtifactDataCache.TryGetValue(name, out var artifactData) || artifactData == null)
            {
                artifactData = new ArtifactData();
                s_ArtifactDataCache.Add(name, artifactData);
            }

            artifactData.Result = result;
            artifactData.Artifacts = artifacts;

            var assetPath = GetArtifactsPath(name);
            var assetDir = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(assetDir))
            {
                Directory.CreateDirectory(assetDir);
            }

            var json = JsonSerialization.Serialize(artifactData, new BuildJsonVisitor());
            File.WriteAllText(assetPath, json);
        }
    }
}
