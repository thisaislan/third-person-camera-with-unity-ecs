using System;

namespace Unity.Build
{
    /// <summary>
    /// Attribute that can be added to a <see cref="BuildStep"/> class to declare which other steps must be run after that build step. 
    /// </summary>
    public sealed class BuildStepRunBeforeAttribute : BuildStepOrderAttribute
    {
        public BuildStepRunBeforeAttribute(Type type) : base(type)
        {
        }

        public override string ToString() => "before";
    }
}
