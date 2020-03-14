using System;

namespace Unity.Build
{
    /// <summary>
    /// Attribute to configure various properties of a <see cref="RunStep"/> type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RunStepAttribute : Attribute
    {
        /// <summary>
        /// Name of the <see cref="RunStep"/> displayed in <see cref="BuildPipeline"/> inspector, searcher menu and log console.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category name of the <see cref="RunStep"/> displayed in the searcher menu.
        /// </summary>
        public string Category { get; set; }
    }
}
