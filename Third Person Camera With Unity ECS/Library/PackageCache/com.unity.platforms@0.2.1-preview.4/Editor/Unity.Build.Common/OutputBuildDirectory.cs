using Unity.Properties;

namespace Unity.Build.Common
{
    /// <summary>
    /// Overrides the default output directory of Builds/BuildConfiguration.name to an arbitrary location. 
    /// </summary>
    [FormerlySerializedAs("Unity.Build.Common.OutputBuildDirectory, Unity.Build.Common")]
    public class OutputBuildDirectory : IBuildComponent
    {
        public string OutputDirectory;
    }

    public static class BuildConfigurationExtensions
    {
        /// <summary>
        /// Get the output build directory for this <see cref="BuildConfiguration"/>.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <param name="config">This build config.</param>
        /// <returns>The output build directory.</returns>
        public static string GetOutputBuildDirectory(this BuildConfiguration config)
        {
            if (config.TryGetComponent<OutputBuildDirectory>(out var outBuildDir))
            {
                return outBuildDir.OutputDirectory;
            }
            return $"Builds/{config.name}";
        }
    }

    public static class BuildStepExtensions
    {
        /// <summary>
        /// Get the output build directory for this <see cref="BuildStep"/>.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <param name="step">This build step.</param>
        /// <param name="context">The build context used throughout this build.</param>
        /// <returns>The output build directory.</returns>
        public static string GetOutputBuildDirectory(this BuildStep step, BuildContext context)
        {
            if (step.HasOptionalComponent<OutputBuildDirectory>(context))
            {
                return step.GetOptionalComponent<OutputBuildDirectory>(context).OutputDirectory;
            }
            return $"Builds/{context.BuildConfiguration.name}";
        }
    }
}
