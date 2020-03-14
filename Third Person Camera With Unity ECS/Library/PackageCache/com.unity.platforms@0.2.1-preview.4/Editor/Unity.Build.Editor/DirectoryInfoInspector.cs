using JetBrains.Annotations;
using System.IO;
using Unity.Properties.Editor;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    [UsedImplicitly]
    sealed class DirectoryInfoInspector : IInspector<DirectoryInfo>
    {
        TextField m_TextField;

        public VisualElement Build(InspectorContext<DirectoryInfo> context)
        {
            m_TextField = new TextField(context.PrettyName);
            m_TextField.RegisterValueChangedCallback(evt =>
            {
                context.Data = new DirectoryInfo(evt.newValue);
            });

            return m_TextField;
        }

        public void Update(InspectorContext<DirectoryInfo> context)
        {
            m_TextField.SetValueWithoutNotify(context.Data.GetRelativePath());
        }
    }
}
