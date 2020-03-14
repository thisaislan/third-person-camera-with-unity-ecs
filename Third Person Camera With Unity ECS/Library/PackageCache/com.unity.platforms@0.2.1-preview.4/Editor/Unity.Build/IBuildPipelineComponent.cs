namespace Unity.Build
{
    /// <summary>
    /// Base interface for <see cref="BuildConfiguration"/> components that provides the <see cref="BuildPipeline"/>.
    /// </summary>
    public interface IBuildPipelineComponent : IBuildComponent
    {
        BuildPipeline Pipeline { get; set; }

        /// <summary>
        /// Returns index which is used for sorting builds when they're batch in build queue
        /// </summary>
        int SortingIndex { get; }

        /// <summary>
        /// Sets the editor environment before starting the build
        /// </summary>
        /// <returns>Returns true, when editor domain reload is required.</returns>
        bool SetupEnvironment();
    }
}
