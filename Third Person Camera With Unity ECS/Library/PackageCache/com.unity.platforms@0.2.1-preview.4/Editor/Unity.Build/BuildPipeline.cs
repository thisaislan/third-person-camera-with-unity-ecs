using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PropertyAttribute = Unity.Properties.PropertyAttribute;

namespace Unity.Build
{
    /// <summary>
    /// Stores <see cref="BuildStep"/> (or <see cref="BuildPipeline"/>) and <see cref="Build.RunStep"/>
    /// instructions to be executed when building or running this pipeline.
    /// </summary>
    [HideInInspector]
    public sealed class BuildPipeline : ScriptableObjectPropertyContainer<BuildPipeline>, IBuildStep
    {
        /// <summary>
        /// The list of <see cref="BuildStep"/> (or <see cref="BuildPipeline"/>) to be executed when calling <see cref="Build"/>.
        /// </summary>
        [Property] public List<IBuildStep> BuildSteps = new List<IBuildStep>();

        /// <summary>
        /// The <see cref="Build.RunStep"/> to be executed when calling <see cref="Run"/>.
        /// </summary>
        [Property] public RunStep RunStep;

        /// <summary>
        /// Event fired when a <see cref="BuildPipeline"/> is about to start building.
        /// </summary>
        internal static event Action<BuildPipeline, BuildConfiguration> BuildStarted;

        /// <summary>
        /// Event fired when a <see cref="BuildPipeline"/> finished building.
        /// </summary>
        internal static event Action<BuildPipelineResult> BuildCompleted;

        /// <summary>
        /// File extension for <see cref="BuildPipeline"/> assets.
        /// </summary>
        public const string AssetExtension = ".buildpipeline";

        /// <summary>
        /// Determine if this <see cref="BuildPipeline"/> can build.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used for the build.</param>
        /// <param name="reason">If <see cref="CanBuild"/> returns <see langword="false"/>, the reason why it fails.</param>
        /// <returns><see langword="true"/> if this <see cref="BuildPipeline"/> can build, otherwise <see langword="false"/>.</returns>
        public bool CanBuild(BuildConfiguration config, out string reason)
        {
            var steps = EnumerateBuildSteps();
            if (!IsBuildStepsComponentsValid(steps, config, out reason) || !IsBuildStepsOrderValid(steps, out reason))
            {
                reason = $"Cannot build {name.ToHyperLink()}:\n{reason}";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Build this <see cref="BuildPipeline"/>.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used for the build.</param>
        /// <param name="progress">Optional build progress that will be displayed when executing the build.</param>
        /// <param name="mutator">Optional mutator that can be used to modify the <see cref="BuildContext"/> before building.</param>
        /// <returns>The result of building this <see cref="BuildPipeline"/>.</returns>
        public BuildPipelineResult Build(BuildConfiguration config, BuildProgress progress = null, Action<BuildContext> mutator = null)
        {
            if (EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Building is not allowed while Unity is compiling.");
            }

            if (!CanBuild(config, out var reason))
            {
                return BuildPipelineResult.Failure(this, config, reason);
            }

            BuildStarted?.Invoke(this, config);
            using (var context = new BuildContext(this, config, progress, mutator))
            {
                var timer = Stopwatch.StartNew();
                var result = RunBuildSteps(context);
                timer.Stop();

                result.Duration = timer.Elapsed;

                var firstFailedBuildStep = result.BuildStepsResults.FirstOrDefault(r => r.Failed);
                if (firstFailedBuildStep != null)
                {
                    result.Succeeded = false;
                    result.Message = firstFailedBuildStep.Message;
                }

                BuildArtifacts.Store(result, context.Values.OfType<IBuildArtifact>().ToArray());
                BuildCompleted?.Invoke(result);
                return result;
            }
        }

        /// <summary>
        /// Determine if this <see cref="BuildPipeline"/> can run.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used for the build.</param>
        /// <param name="reason">If <see cref="CanRun"/> returns <see langword="false"/>, the reason why it fails.</param>
        /// <returns>The result of running this <see cref="BuildPipeline"/>.</returns>
        public bool CanRun(BuildConfiguration config, out string reason)
        {
            var result = BuildArtifacts.GetBuildResult(config);
            if (result == null)
            {
                reason = $"No build result found for {config.name.ToHyperLink()}.";
                return false;
            }

            if (result.Failed)
            {
                reason = $"Last build failed with error:\n{result.Message}";
                return false;
            }

            if (RunStep == null)
            {
                reason = $"No run step provided for {name.ToHyperLink()}.";
                return false;
            }

            if (!RunStep.CanRun(config, out reason))
            {
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>
        /// Run this <see cref="BuildPipeline"/>.
        /// This will attempt to run the build target produced from building this <see cref="BuildPipeline"/>.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>The result of running this <see cref="BuildPipeline"/>.</returns>
        public RunStepResult Run(BuildConfiguration config)
        {
            if (!CanRun(config, out var reason))
            {
                return RunStepResult.Failure(config, RunStep, reason);
            }

            try
            {
                return RunStep.Start(config);
            }
            catch (Exception exception)
            {
                return RunStepResult.Exception(config, RunStep, exception);
            }
        }

        protected override void Reset()
        {
            base.Reset();
            BuildSteps.Clear();
            RunStep = null;
        }

        protected override void Sanitize()
        {
            base.Sanitize();
            BuildSteps.RemoveAll(step => step == null);
        }

        /// <summary>
        /// Queues and builds multiple builds. For builds requiring explicit active Editor build target, this function also switches Editor build target before starting the build.
        /// That's why there's no return result here, because the build won't be executed immediately in some cases.
        /// </summary>
        internal static void BuildAsync(BuildBatchDescription buildBatchDescription)
        {
            var buildEntities = buildBatchDescription.BuildItems;
            // ToDo: when running multiple builds, should we stop at first failure?
            var buildPipelineResults = new BuildPipelineResult[buildEntities.Length];

            for (int i = 0; i < buildEntities.Length; i++)
            {
                var config = buildEntities[i].BuildConfiguration;
                var pipeline = config.GetBuildPipeline();
                if (!config.CanBuild(out var reason))
                {
                    buildPipelineResults[i] = BuildPipelineResult.Failure(pipeline, config, reason);
                }
                else
                {
                    buildPipelineResults[i] = null;
                }
            }

            var queue = BuildQueue.instance;
            for (int i = 0; i < buildEntities.Length; i++)
            {
                var config = buildEntities[i].BuildConfiguration;
                var pipeline = config.GetBuildPipeline();
                queue.QueueBuild(config, buildPipelineResults[i]);
            }

            queue.FlushBuilds(buildBatchDescription.OnBuildCompleted);
        }

        /// <summary>
        /// Cancels and clear the build queue. It also stops switching editor targets, so the target which was set last will remain.
        /// </summary>
        internal static void CancelBuildAsync()
        {
            BuildQueue.instance.Clear();
        }

        BuildPipelineResult RunBuildSteps(BuildContext context)
        {
            var timer = new Stopwatch();
            var status = context.BuildPipelineStatus;
            var title = context.BuildProgress?.Title ?? string.Empty;

            // Setup build steps list
            var cleanupSteps = new Stack<BuildStep>();
            var enabledSteps = EnumerateBuildSteps().Where(step => step.IsEnabled(context)).ToArray();

            // Run build steps and stop on first failure of any kind
            for (var i = 0; i < enabledSteps.Length; ++i)
            {
                var step = enabledSteps[i];

                // Update build progress
                var cancelled = context.BuildProgress?.Update($"{title} (Step {i + 1} of {enabledSteps.Length})", step.Description + "...", (float)i / enabledSteps.Length) ?? false;
                if (cancelled)
                {
                    status.Succeeded = false;
                    status.Message = $"{title} was cancelled.";
                    break;
                }

                // Add step to cleanup stack only if it overrides implementation
                if (step.GetType().GetMethod(nameof(BuildStep.CleanupBuildStep)).DeclaringType != typeof(BuildStep))
                {
                    cleanupSteps.Push(step);
                }

                // Run build step
                try
                {
                    timer.Restart();
                    var result = step.RunBuildStep(context);
                    timer.Stop();

                    // Update build step duration
                    result.Duration = timer.Elapsed;

                    // Add build step result to pipeline status
                    status.BuildStepsResults.Add(result);

                    // Stop execution for normal build steps after failure
                    if (!result.Succeeded)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    // Add build step exception to pipeline status, and stop executing build steps
                    status.BuildStepsResults.Add(BuildStepResult.Exception(step, exception));
                    break;
                }
            }

            // Execute cleanup even if there are failures in build steps.
            // * In opposite order of build steps that ran
            // * Can't be cancelled; cleanup step must run
            foreach (var step in cleanupSteps)
            {
                // Update build progress
                context.BuildProgress?.Update($"{title} (Cleanup)", step.Description + "...", 1.0F);

                // Run cleanup step
                try
                {
                    timer.Restart();
                    var result = step.CleanupBuildStep(context);
                    timer.Stop();

                    // Update cleanup step duration
                    result.Duration = timer.Elapsed;

                    // Add cleanup step result to pipeline status
                    status.BuildStepsResults.Add(result);
                }
                catch (Exception exception)
                {
                    // Add cleanup step exception to pipeline status (not stopping execution)
                    status.BuildStepsResults.Add(BuildStepResult.Exception(step, exception));
                }
            }

            return status;
        }

        IEnumerable<BuildStep> EnumerateBuildSteps()
        {
            foreach (var step in BuildSteps)
            {
                if (step is BuildPipeline pipeline)
                {
                    foreach (var nestedStep in pipeline.EnumerateBuildSteps())
                    {
                        yield return nestedStep;
                    }
                }
                else
                {
                    yield return step as BuildStep;
                }
            }
        }

        static bool IsBuildStepsComponentsValid(IEnumerable<BuildStep> steps, BuildConfiguration config, out string reason)
        {
            foreach (var step in steps)
            {
                if (step.RequiredComponents != null)
                {
                    foreach (var type in step.RequiredComponents)
                    {
                        if (!typeof(IBuildComponent).IsAssignableFrom(type))
                        {
                            reason = $"Type '{type.Name}' is not a valid required component type. It must derive from {nameof(IBuildComponent)}.";
                            return false;
                        }

                        if (!config.HasComponent(type))
                        {
                            reason = $"Build configuration '{config.name}' is missing component '{type.Name}' which is required by build pipeline step '{step.Name}'";
                            return false;
                        }
                    }
                }

                if (step.OptionalComponents != null)
                {
                    foreach (var type in step.OptionalComponents)
                    {
                        if (!typeof(IBuildComponent).IsAssignableFrom(type))
                        {
                            reason = $"Type '{type.Name}' is not a valid optional component type. It must derive from {nameof(IBuildComponent)}.";
                            return false;
                        }
                    }
                }
            }

            reason = null;
            return true;
        }

        static bool IsBuildStepsOrderValid(IEnumerable<IBuildStep> steps, out string reason)
        {
            var reasons = new List<string>();
            for (int i = 0; i < steps.Count(); ++i)
            {
                if (!ValidateBuildStepPosition(steps, i, out var stepReason))
                {
                    var name = BuildStep.GetName(steps.ElementAt(i).GetType());
                    reasons.Add($"Build step {name}: {string.Join("\n", stepReason)}");
                }
            }

            if (reasons.Count > 0)
            {
                reason = string.Join("\n", reasons);
                return false;
            }

            reason = null;
            return true;
        }

        internal static bool ValidateBuildStepPosition(IEnumerable<IBuildStep> steps, int index, out string[] reasons)
        {
            var reasonsList = new List<string>();
            var attributes = steps.ElementAt(index).GetType().GetCustomAttributes<BuildStepOrderAttribute>().ToArray();

            foreach (var attribute in attributes)
            {
                List<IBuildStep> stepsSet = null;
                if (attribute.GetType() == typeof(BuildStepRunBeforeAttribute))
                {
                    stepsSet = steps.Skip(index).ToList();
                }
                else if (attribute.GetType() == typeof(BuildStepRunAfterAttribute))
                {
                    stepsSet = steps.Take(index).ToList();
                }
                else
                {
                    continue;
                }

                if (stepsSet.All(step => attribute.DependentStep != step.GetType()))
                {
                    reasonsList.Add($"    Build step {attribute.DependentStep.Name} must be run {attribute.ToString()} this build step.");
                }
            }

            reasons = reasonsList.Count > 0 ? reasonsList.ToArray() : null;
            return reasons == null;
        }
    }
}
