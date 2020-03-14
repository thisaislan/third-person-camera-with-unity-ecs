namespace Unity.Build
{
    /// <summary>
    /// Can stores a set of hierarchical build components per type, which can be inherited or overridden using dependencies.
    /// </summary>
    public sealed class BuildConfiguration : HierarchicalComponentContainer<BuildConfiguration, IBuildComponent>
    {
        /// <summary>
        /// File extension for <see cref="BuildConfiguration"/> assets.
        /// </summary>
        public const string AssetExtension = ".buildconfiguration";

        /// <summary>
        /// Retrieve the <see cref="BuildPipeline"/> of this <see cref="BuildConfiguration"/>.
        /// </summary>
        /// <returns>The <see cref="BuildPipeline"/> if found, otherwise <see langword="null"/>.</returns>
        public BuildPipeline GetBuildPipeline()
        {
            if (TryGetComponent<IBuildPipelineComponent>(out var component))
            {
                var pipeline = component.Pipeline;
                return pipeline != null && pipeline ? pipeline : null;
            }
            return null;
        }

        /// <summary>
        /// Determine if the <see cref="BuildPipeline"/> of this <see cref="BuildConfiguration"/> can build.
        /// </summary>
        /// <param name="reason">If <see cref="CanBuild"/> returns <see langword="false"/>, the reason why it fails.</param>
        /// <returns>Whether or not the <see cref="BuildPipeline"/> can build.</returns>
        public bool CanBuild(out string reason)
        {
            var pipeline = GetBuildPipeline();
            if (pipeline == null)
            {
                reason = $"No valid build pipeline found for {name.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.";
                return false;
            }
            return pipeline.CanBuild(this, out reason);
        }

        /// <summary>
        /// Run the <see cref="BuildPipeline"/> of this <see cref="BuildConfiguration"/> to build the target.
        /// </summary>
        /// <returns>The result of the <see cref="BuildPipeline"/> build.</returns>
        public BuildPipelineResult Build()
        {
            var pipeline = GetBuildPipeline();
            if (!CanBuild(out var reason))
            {
                return BuildPipelineResult.Failure(pipeline, this, reason);
            }

            var what = !string.IsNullOrEmpty(name) ? $" {name}" : string.Empty;
            using (var progress = new BuildProgress($"Building{what}", "Please wait..."))
            {
                return pipeline.Build(this, progress);
            }
        }

        /// <summary>
        /// Determine if the <see cref="BuildPipeline"/> of this <see cref="BuildConfiguration"/> can run.
        /// </summary>
        /// <param name="reason">If <see cref="CanRun"/> returns <see langword="false"/>, the reason why it fails.</param>
        /// <returns>Whether or not the <see cref="BuildPipeline"/> can run.</returns>
        public bool CanRun(out string reason)
        {
            var pipeline = GetBuildPipeline();
            if (pipeline == null)
            {
                reason = $"No valid build pipeline found for {name.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.";
                return false;
            }
            return pipeline.CanRun(this, out reason);
        }

        /// <summary>
        /// Run the resulting target from building the <see cref="BuildPipeline"/> of this <see cref="BuildConfiguration"/>.
        /// </summary>
        /// <returns></returns>
        public RunStepResult Run()
        {
            var pipeline = GetBuildPipeline();
            if (!CanRun(out var reason))
            {
                return RunStepResult.Failure(this, pipeline?.RunStep, reason);
            }
            return pipeline.Run(this);
        }
    }
}
