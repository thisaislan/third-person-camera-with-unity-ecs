using System;

namespace Unity.Build
{
    /// <summary>
    /// Base attribute used for declaring dependencies between multiple <see cref="BuildStep"/> classes. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class BuildStepOrderAttribute : Attribute
    {
        public Type DependentStep { get; private set; }
        public BuildStepOrderAttribute(Type type)
        {
            DependentStep = type;
        }
    }
}
