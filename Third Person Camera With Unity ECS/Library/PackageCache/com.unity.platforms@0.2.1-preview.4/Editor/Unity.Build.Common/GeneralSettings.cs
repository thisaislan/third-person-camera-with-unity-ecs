using Unity.Properties;

namespace Unity.Build.Common
{
    [FormerlySerializedAs("Unity.Build.Common.GeneralSettings, Unity.Build.Common")]
    public sealed class GeneralSettings : IBuildComponent
    {
        public string ProductName = "Product Name";
        public string CompanyName = "Company Name";
    }
}
