using Unity.Properties;
using Unity.Serialization;
using Unity.Serialization.Json;
using UnityEditor;

namespace Unity.Build
{
    internal sealed class BuildJsonVisitorAdapter : JsonVisitorAdapter,
        IVisitAdapter<IBuildStep>,
        IVisitAdapter<RunStep>
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            // NOTE: Types conversion registrations have to happen in InitializeOnLoadMethod,
            // otherwise they could be registered too late and some conversions would fail silently.
            TypeConversion.Register<SerializedStringView, IBuildStep>(view => BuildStep.Deserialize(view.ToString()));
            TypeConversion.Register<SerializedStringView, RunStep>(view => RunStep.Deserialize(view.ToString()));
        }

        public BuildJsonVisitorAdapter(JsonVisitor visitor) : base(visitor) { }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref IBuildStep value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, IBuildStep>
        {
            AppendJsonString(property, BuildStep.Serialize(value));
            return VisitStatus.Override;
        }

        public VisitStatus Visit<TProperty, TContainer>(IPropertyVisitor visitor, TProperty property, ref TContainer container, ref RunStep value, ref ChangeTracker changeTracker)
            where TProperty : IProperty<TContainer, RunStep>
        {
            AppendJsonString(property, RunStep.Serialize(value));
            return VisitStatus.Override;
        }
    }
}
