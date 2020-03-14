using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using PropertyAttribute = Unity.Properties.PropertyAttribute;

namespace Unity.Build
{
    /// <summary>
    /// Holds the results of the execution of a <see cref="Build.BuildPipeline"/>.
    /// </summary>
    public sealed class BuildPipelineResult
    {
        /// <summary>
        /// Determine if the execution of the <see cref="Build.BuildPipeline"/> succeeded.
        /// </summary>
        [Property] public bool Succeeded { get; internal set; }

        /// <summary>
        /// Determine if the execution of the <see cref="Build.BuildPipeline"/> failed.
        /// </summary>
        public bool Failed { get => !Succeeded; }

        /// <summary>
        /// The message resulting from the execution of this <see cref="Build.BuildPipeline"/>.
        /// </summary>
        [Property] public string Message { get; internal set; }

        /// <summary>
        /// The total duration of the <see cref="Build.BuildPipeline"/> execution.
        /// </summary>
        [Property] public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// The <see cref="Build.BuildPipeline"/> that was run.
        /// </summary>
        [Property] public BuildPipeline BuildPipeline { get; internal set; }

        /// <summary>
        /// The <see cref="Build.BuildConfiguration"/> used throughout the execution of the <see cref="Build.BuildPipeline"/>.
        /// </summary>
        [Property] public BuildConfiguration BuildConfiguration { get; internal set; }

        /// <summary>
        /// A list of <see cref="BuildStepResult"/> collected during the <see cref="Build.BuildPipeline"/> execution for each <see cref="IBuildStep"/>.
        /// </summary>
        [Property] public List<BuildStepResult> BuildStepsResults { get; internal set; } = new List<BuildStepResult>();

        /// <summary>
        /// Get the <see cref="BuildStepResult"/> for the specified <see cref="IBuildStep"/>.
        /// </summary>
        /// <param name="buildStep">The build step to search for the result.</param>
        /// <param name="value">The <see cref="BuildStepResult"/> if found, otherwise default(<see cref="BuildStepResult"/>)</param>
        /// <returns><see langword="true"/> if the IBuildStep was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetBuildStepResult(IBuildStep buildStep, out BuildStepResult value)
        {
            foreach (var result in BuildStepsResults)
            {
                if (result.BuildStep == buildStep)
                {
                    value = result;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="result">Instance of <see cref="BuildPipelineResult"/>.</param>
        public static implicit operator bool(BuildPipelineResult result) => result.Succeeded;

        /// <summary>
        /// Create a new instance of <see cref="BuildPipelineResult"/> that represent a successful execution.
        /// </summary>
        /// <param name="config">The <see cref="Build.BuildConfiguration"/> used throughout this <see cref="Build.BuildPipeline"/> execution.</param>
        /// <returns>A new <see cref="BuildPipelineResult"/> instance.</returns>
        public static BuildPipelineResult Success(BuildPipeline pipeline, BuildConfiguration config) => new BuildPipelineResult
        {
            Succeeded = true,
            BuildPipeline = pipeline,
            BuildConfiguration = config
        };

        /// <summary>
        /// Create a new instance of <see cref="BuildPipelineResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="config">The <see cref="Build.BuildConfiguration"/> used throughout this <see cref="Build.BuildPipeline"/> execution.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="BuildPipelineResult"/> instance.</returns>
        public static BuildPipelineResult Failure(BuildPipeline pipeline, BuildConfiguration config, string message) => new BuildPipelineResult
        {
            Succeeded = false,
            Message = message,
            BuildPipeline = pipeline,
            BuildConfiguration = config
        };

        /// <summary>
        /// Output the log result to developer debug console.
        /// </summary>
        public void LogResult()
        {
            if (Succeeded)
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, BuildConfiguration, ToString());
            }
            else
            {
                Debug.LogError(ToString(), BuildConfiguration);
            }
        }

        /// <summary>
        /// Get the <see cref="BuildPipelineResult"/> as a string that can be used for logging.
        /// </summary>
        /// <returns>The <see cref="BuildPipelineResult"/> as a string.</returns>
        public override string ToString()
        {
            var name = BuildConfiguration.name;
            var what = !string.IsNullOrEmpty(name) ? $" {name.ToHyperLink()}" : string.Empty;

            if (Succeeded)
            {
                return $"Build{what} succeeded after {Duration.ToShortString()}.";
            }
            else
            {
                var result = BuildStepsResults.FirstOrDefault(r => r.Failed);
                if (result != null && result.Failed)
                {
                    return $"Build{what} failed in {result.BuildStep.Name} after {Duration.ToShortString()}.\n{Message}";
                }
                else
                {
                    return $"Build{what} failed after {Duration.ToShortString()}.\n{Message}";
                }
            }
        }

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            TypeConstruction.SetExplicitConstructionMethod(() => { return new BuildPipelineResult(); });
        }

        internal BuildPipelineResult() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings { get; internal set; }
    }
}
