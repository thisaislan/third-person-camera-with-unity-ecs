using Newtonsoft.Json.Linq;

namespace Unity.Build.DotsRuntime
{
    public interface IDotsRuntimeBuildModifier : IBuildComponent
    {
        void Modify(JObject settingsJObject);
    }
}
