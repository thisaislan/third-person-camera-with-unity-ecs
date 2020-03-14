using Unity.Serialization.Json;

namespace Unity.Build
{
    internal sealed class BuildJsonVisitor : JsonVisitor
    {
        public BuildJsonVisitor()
        {
            AddAdapter(new BuildJsonVisitorAdapter(this));
        }

        protected override string GetTypeInfo<TProperty, TContainer, TValue>(TProperty property, ref TContainer container, ref TValue value)
        {
            return value?.GetType().GetFullyQualifedAssemblyTypeName();
        }
    }
}
