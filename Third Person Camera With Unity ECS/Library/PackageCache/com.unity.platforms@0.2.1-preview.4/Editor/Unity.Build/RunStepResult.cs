using System;
using System.ComponentModel;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// Holds the result of the execution of a <see cref="Build.RunStep"/>.
    /// </summary>
    public sealed class RunStepResult : IDisposable
    {
        /// <summary>
        /// Determine if the execution of the <see cref="Build.RunStep"/> succeeded.
        /// </summary>
        public bool Succeeded { get; internal set; }

        /// <summary>
        /// Determine if the execution of the <see cref="Build.RunStep"/> failed.
        /// </summary>
        public bool Failed { get => !Succeeded; }

        /// <summary>
        /// The message resulting from the execution of this <see cref="Build.RunStep"/>.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// The <see cref="Build.BuildConfiguration"/> used to run this <see cref="Build.RunStep"/>.
        /// </summary>
        public BuildConfiguration BuildConfiguration { get; internal set; }

        /// <summary>
        /// The <see cref="Build.RunStep"/> that was executed.
        /// </summary>
        public RunStep RunStep { get; internal set; }

        /// <summary>
        /// The running process resulting from running the <see cref="Build.RunStep"/>.
        /// </summary>
        public IRunInstance RunInstance { get; internal set; }

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="result">Instance of <see cref="RunStepResult"/>.</param>
        public static implicit operator bool(RunStepResult result) => result.Succeeded;

        /// <summary>
        /// Create a new instance of <see cref="RunStepResult"/> that represent a successful execution.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used by the <see cref="Build.RunStep"/>.</param>
        /// <param name="step">The <see cref="Build.RunStep"/> that was executed.</param>
        /// <param name="instance">The <see cref="IRunInstance"/> resulting from running this <see cref="Build.RunStep"/>.</param>
        /// <returns>A new <see cref="RunStepResult"/> instance.</returns>
        public static RunStepResult Success(BuildConfiguration config, RunStep step, IRunInstance instance) => new RunStepResult
        {
            Succeeded = true,
            BuildConfiguration = config,
            RunStep = step,
            RunInstance = instance
        };

        /// <summary>
        /// Create a new instance of <see cref="RunStepResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used by the <see cref="Build.RunStep"/>.</param>
        /// <param name="step">The <see cref="Build.RunStep"/> that was executed.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="RunStepResult"/> instance.</returns>
        public static RunStepResult Failure(BuildConfiguration config, RunStep step, string message) => new RunStepResult
        {
            Succeeded = false,
            Message = message,
            BuildConfiguration = config,
            RunStep = step,
            RunInstance = null
        };

        internal static RunStepResult Exception(BuildConfiguration config, RunStep step, Exception exception) => new RunStepResult
        {
            Succeeded = false,
            Message = exception.Message + "\n" + exception.StackTrace,
            BuildConfiguration = config,
            RunStep = step,
            RunInstance = null
        };

        public void LogResult()
        {
            if (Succeeded)
            {
                // Disabled logging successful run result until we decide if its useful
                //Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, BuildConfiguration, ToString());
            }
            else
            {
                Debug.LogError(ToString(), BuildConfiguration);
            }
        }

        public override string ToString()
        {
            var name = BuildConfiguration.name;
            var what = !string.IsNullOrEmpty(name) ? $" {name.ToHyperLink()}" : string.Empty;

            if (Succeeded)
            {
                return $"Run{what} successful.";
            }
            else
            {
                return $"Run{what} failed.\n{Message}";
            }
        }

        public void Dispose()
        {
            if (RunInstance != null)
            {
                RunInstance.Dispose();
            }
        }

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            TypeConstruction.SetExplicitConstructionMethod(() => { return new RunStepResult(); });
        }

        internal RunStepResult() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings { get; internal set; }
    }
}
