using System;
using Unity.Properties;
using UnityEditor;

namespace Unity.Build
{
    /// <summary>
    /// Holds the result of the execution of a <see cref="Build.BuildStep"/>.
    /// </summary>
    public sealed class BuildStepResult
    {
        /// <summary>
        /// Determine if the execution of the <see cref="Build.BuildStep"/> succeeded.
        /// </summary>
        [Property] public bool Succeeded { get; internal set; }

        /// <summary>
        /// Determine if the execution of the <see cref="Build.BuildStep"/> failed.
        /// </summary>
        public bool Failed { get => !Succeeded; }

        /// <summary>
        /// The message resulting from the execution of this <see cref="Build.BuildStep"/>.
        /// </summary>
        [Property] public string Message { get; internal set; }

        /// <summary>
        /// Duration of the execution of this <see cref="Build.BuildStep"/>.
        /// </summary>
        [Property] public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// The <see cref="Build.BuildStep"/> that was executed.
        /// </summary>
        [Property] public BuildStep BuildStep { get; internal set; }

        /// <summary>
        /// Description of the <see cref="Build.BuildStep"/>.
        /// </summary>
        [Property] public string Description => BuildStep.Description;

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="result">Instance of <see cref="BuildStepResult"/>.</param>
        public static implicit operator bool(BuildStepResult result) => result.Succeeded;

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> from a <see cref="UnityEditor.Build.Reporting.BuildReport"/>.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <param name="report">The report that was generated.</param>
        public BuildStepResult(BuildStep step, UnityEditor.Build.Reporting.BuildReport report)
        {
            Succeeded = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            Message = Failed ? report.summary.ToString() : null;
            BuildStep = step;
        }

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> that represent a successful execution.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public static BuildStepResult Success(BuildStep step) => new BuildStepResult
        {
            Succeeded = true,
            BuildStep = step
        };

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public static BuildStepResult Failure(BuildStep step, string message) => new BuildStepResult
        {
            Succeeded = false,
            Message = message,
            BuildStep = step
        };

        internal static BuildStepResult Exception(BuildStep step, Exception exception) => new BuildStepResult
        {
            Succeeded = false,
            Message = exception.Message + "\n" + exception.StackTrace,
            BuildStep = step
        };

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            TypeConstruction.SetExplicitConstructionMethod(() => { return new BuildStepResult(); });
        }

        internal BuildStepResult() { }
    }
}
