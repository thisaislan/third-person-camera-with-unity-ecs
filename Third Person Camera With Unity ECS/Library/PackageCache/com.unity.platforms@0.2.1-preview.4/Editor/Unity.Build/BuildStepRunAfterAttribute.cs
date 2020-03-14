using System;

namespace Unity.Build
{
    /// <summary>
    /// Attribute that can be added to a <see cref="BuildStep"/> class to declare which other steps must be run before that build step. 
    /// </summary>
    public sealed class BuildStepRunAfterAttribute : BuildStepOrderAttribute
    {
        public BuildStepRunAfterAttribute(Type type) : base(type)
        {
        }

        public override string ToString() => "after";
    }
}
