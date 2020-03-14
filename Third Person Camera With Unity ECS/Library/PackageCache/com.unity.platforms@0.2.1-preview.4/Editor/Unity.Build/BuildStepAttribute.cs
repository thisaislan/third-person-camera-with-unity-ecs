using System;
using System.ComponentModel;

namespace Unity.Build
{
    /// <summary>
    /// Attribute to configure various properties of a <see cref="BuildStep"/> type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BuildStepAttribute : Attribute
    {
        /// <summary>
        /// Name of the <see cref="BuildStep"/> displayed in <see cref="BuildPipeline"/> inspector, searcher menu and log console.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the <see cref="BuildStep"/> displayed in the <see cref="BuildProgress"/> during a build.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category name of the <see cref="BuildStep"/> displayed in the searcher menu.
        /// </summary>
        public string Category { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("To hide a BuildStep in inspector or searcher menu, please use attribute [HideInInspector] instead. (RemovedAfter 2020-05-01)")]
        public enum Flags
        {
            None = 0,
            Hidden = 1
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("To hide a BuildStep in inspector or searcher menu, please use attribute [HideInInspector] instead. (RemovedAfter 2020-05-01)")]
        public Flags flags = Flags.None;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("description has been renamed to Description. (RemovedAfter 2020-05-01) (UnityUpgradable) -> Description")]
        public string description
        {
            get => Description;
            set => Description = value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("category has been renamed to Category. (RemovedAfter 2020-05-01) (UnityUpgradable) -> Category")]
        public string category
        {
            get => Category;
            set => Category = value;
        }
    }
}
