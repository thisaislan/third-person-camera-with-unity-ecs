using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    using BuildConfigurationElement = HierarchicalComponentContainerElement<BuildConfiguration, IBuildComponent, IBuildComponent>;

    [CustomEditor(typeof(BuildConfigurationScriptedImporter))]
    sealed class BuildConfigurationScriptedImporterEditor : ScriptedImporterEditor
    {
        static class ClassNames
        {
            public const string BaseClassName = nameof(BuildConfiguration);
            public const string Dependencies = BaseClassName + "__asset-dependencies";
            public const string Header = BaseClassName + "__asset-header";
            public const string HeaderLabel = BaseClassName + "__asset-header-label";
            public const string BuildAction = BaseClassName + "__build-action";
            public const string BuildDropdown = BaseClassName + "__build-dropdown";
            public const string AddComponent = BaseClassName + "__add-component-button";
        }

        struct BuildAction
        {
            public string Name;
            public Action<BuildConfiguration> Action;
        }

        static readonly BuildAction k_Build = new BuildAction
        {
            Name = "Build",
            Action = config => config.Build().LogResult()
        };

        static readonly BuildAction k_BuildAndRun = new BuildAction
        {
            Name = "Build and Run",
            Action = (config) =>
            {
                var buildResult = config.Build();
                buildResult.LogResult();
                if (buildResult.Failed)
                {
                    return;
                }

                using (var runResult = config.Run())
                {
                    runResult.LogResult();
                }
            }
        };

        static readonly BuildAction k_Run = new BuildAction
        {
            Name = "Run",
            Action = (config) =>
            {
                using (var result = config.Run())
                {
                    result.LogResult();
                }
            }
        };

        // Needed because properties don't handle root collections well.
        class DependenciesWrapper
        {
            public List<BuildConfiguration> Dependencies;
        }

        const string k_CurrentActionKey = "BuildAction-CurrentAction";

        bool m_LastEditState;
        BindableElement m_BuildConfigurationRoot;
        readonly DependenciesWrapper m_DependenciesWrapper = new DependenciesWrapper();

        protected override bool needsApplyRevert { get; } = true;
        public override bool showImportedObject { get; } = false;
        BuildAction CurrentBuildAction => BuildActions[CurrentActionIndex];

        static List<BuildAction> BuildActions { get; } = new List<BuildAction>
        {
            k_Build,
            k_BuildAndRun,
            k_Run,
        };

        static int CurrentActionIndex
        {
            get => EditorPrefs.HasKey(k_CurrentActionKey) ? EditorPrefs.GetInt(k_CurrentActionKey) : BuildActions.IndexOf(k_BuildAndRun);
            set => EditorPrefs.SetInt(k_CurrentActionKey, value);
        }

        protected override Type extraDataType => typeof(BuildConfiguration);

        protected override void InitializeExtraDataInstance(UnityEngine.Object extraData, int targetIndex)
        {
            var target = targets[targetIndex];
            if (target == null || !target)
            {
                return;
            }

            var assetImporter = target as AssetImporter;
            if (assetImporter == null || !assetImporter)
            {
                return;
            }

            var config = extraData as BuildConfiguration;
            if (config == null || !config)
            {
                return;
            }

            if (BuildConfiguration.DeserializeFromPath(config, assetImporter.assetPath))
            {
                config.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
            }
        }

        protected override void OnHeaderGUI()
        {
            // Intentional
            //base.OnHeaderGUI();
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (target == null || !target)
                {
                    continue;
                }

                var assetImporter = target as AssetImporter;
                if (assetImporter == null || !assetImporter)
                {
                    continue;
                }

                var config = extraDataTargets[i] as BuildConfiguration;
                if (config == null || !config)
                {
                    continue;
                }

                config.SerializeToPath(assetImporter.assetPath);
            }
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (target == null || !target)
                {
                    continue;
                }

                var assetImporter = target as AssetImporter;
                if (assetImporter == null || !assetImporter)
                {
                    continue;
                }

                var config = extraDataTargets[i] as BuildConfiguration;
                if (config == null || !config)
                {
                    continue;
                }

                if (BuildConfiguration.DeserializeFromPath(config, assetImporter.assetPath))
                {
                    config.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            m_BuildConfigurationRoot = new BindableElement();
            m_BuildConfigurationRoot.AddStyleSheetAndVariant(ClassNames.BaseClassName);

            Refresh(m_BuildConfigurationRoot);

            root.contentContainer.Add(m_BuildConfigurationRoot);
            root.contentContainer.Add(new IMGUIContainer(ApplyRevertGUI));
            return root;
        }

        void Refresh(BindableElement root)
        {
            root.Clear();

            var config = extraDataTarget as BuildConfiguration;
            if (config == null)
            {
                return;
            }

            m_LastEditState = AssetDatabase.IsOpenForEdit(config);
            var openedForEditUpdater = UIUpdaters.MakeBinding(config, root);
            openedForEditUpdater.OnPreUpdate += updater =>
            {
                if (!updater.Source)
                {
                    return;
                }
                m_LastEditState = AssetDatabase.IsOpenForEdit(updater.Source);
            };
            root.binding = openedForEditUpdater;

            RefreshHeader(root, config);
            RefreshDependencies(root, config);
            RefreshComponents(root, config);
        }

        void RefreshHeader(BindableElement root, BuildConfiguration config)
        {
            var headerRoot = new VisualElement();
            headerRoot.AddToClassList(ClassNames.Header);
            root.Add(headerRoot);

            // Refresh Name Label
            var nameLabel = new Label(config.name);
            nameLabel.AddToClassList(ClassNames.HeaderLabel);
            headerRoot.Add(nameLabel);

            var labelUpdater = UIUpdaters.MakeBinding(config, nameLabel);
            labelUpdater.OnUpdate += (binding) =>
            {
                if (binding.Source != null && binding.Source)
                {
                    binding.Element.text = binding.Source.name;
                }
            };
            nameLabel.binding = labelUpdater;

            // Refresh Build&Run Button
            var dropdownButton = new VisualElement();
            dropdownButton.style.flexDirection = FlexDirection.Row;
            dropdownButton.style.justifyContent = Justify.FlexEnd;
            nameLabel.Add(dropdownButton);

            var dropdownActionButton = new Button { text = BuildActions[CurrentActionIndex].Name };
            dropdownActionButton.AddToClassList(ClassNames.BuildAction);
            dropdownActionButton.clickable = new Clickable(() => CurrentBuildAction.Action(assetTarget as BuildConfiguration));

            dropdownActionButton.SetEnabled(true);
            dropdownButton.Add(dropdownActionButton);

            var actionUpdater = UIUpdaters.MakeBinding(this, dropdownActionButton);
            actionUpdater.OnUpdate += (binding) =>
            {
                if (binding.Source != null && binding.Source)
                {
                    binding.Element.text = CurrentBuildAction.Name;
                }
            };
            dropdownActionButton.binding = actionUpdater;

            var dropdownActionPopup = new PopupField<BuildAction>(BuildActions, CurrentActionIndex, a => string.Empty, a => a.Name);
            dropdownActionPopup.AddToClassList(ClassNames.BuildDropdown);
            dropdownActionPopup.RegisterValueChangedCallback(evt =>
            {
                CurrentActionIndex = BuildActions.IndexOf(evt.newValue);
                dropdownActionButton.clickable = new Clickable(() => CurrentBuildAction.Action(assetTarget as BuildConfiguration));
            });
            dropdownButton.Add(dropdownActionPopup);

            // Refresh Asset Field
            var assetField = new ObjectField { objectType = typeof(BuildConfiguration) };
            assetField.Q<VisualElement>(className: "unity-object-field__selector").SetEnabled(false);
            assetField.SetValueWithoutNotify(config);
            headerRoot.Add(assetField);

            var assetUpdater = UIUpdaters.MakeBinding(config, assetField);
            assetField.SetEnabled(m_LastEditState);
            assetUpdater.OnPreUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            assetField.binding = assetUpdater;
        }

        void RefreshDependencies(BindableElement root, BuildConfiguration config)
        {
            m_DependenciesWrapper.Dependencies = FilterDependencies(config, config.Dependencies).ToList();

            var dependencyElement = new PropertyElement();
            dependencyElement.AddToClassList(ClassNames.BaseClassName);
            dependencyElement.SetTarget(m_DependenciesWrapper);
            dependencyElement.OnChanged += element =>
            {
                config.Dependencies.Clear();
                config.Dependencies.AddRange(FilterDependencies(config, m_DependenciesWrapper.Dependencies));
                Refresh(root);
            };
            dependencyElement.SetEnabled(m_LastEditState);
            root.Add(dependencyElement);

            var foldout = dependencyElement.Q<Foldout>();
            foldout.AddToClassList(ClassNames.Dependencies);
            foldout.Q<Toggle>().AddToClassList(BuildConfigurationElement.ClassNames.Header);
            foldout.contentContainer.AddToClassList(BuildConfigurationElement.ClassNames.Fields);

            var dependencyUpdater = UIUpdaters.MakeBinding(config, dependencyElement);
            dependencyUpdater.OnPreUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            dependencyElement.binding = dependencyUpdater;
        }

        IEnumerable<BuildConfiguration> FilterDependencies(BuildConfiguration config, IEnumerable<BuildConfiguration> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                if (dependency == null || !dependency || dependency == config || dependency.HasDependency(config))
                {
                    yield return null;
                }
                else
                {
                    yield return dependency;
                }
            }
        }

        void RefreshComponents(BindableElement root, BuildConfiguration config)
        {
            // Refresh Components
            var componentRoot = new BindableElement();
            var components = config.GetComponents();
            foreach (var component in components)
            {
                componentRoot.Add(GetComponentElement(config, component));
            }
            componentRoot.SetEnabled(m_LastEditState);
            root.Add(componentRoot);

            var componentUpdater = UIUpdaters.MakeBinding(config, componentRoot);
            componentUpdater.OnUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            componentRoot.binding = componentUpdater;

            // Refresh Add Component Button
            var addComponentButton = new Button();
            addComponentButton.AddToClassList(ClassNames.AddComponent);
            addComponentButton.RegisterCallback<MouseUpEvent>(evt =>
            {
                var databases = new[]
                {
                    TypeSearcherDatabase.GetBuildConfigurationDatabase(new HashSet<Type>(BuildConfiguration.GetAvailableTypes(type => !IsShown(type)).Concat(components.Select(c => c.GetType()))))
                };

                var searcher = new Searcher(databases, new AddTypeSearcherAdapter("Add Component"));
                var editorWindow = EditorWindow.focusedWindow;
                var button = evt.target as Button;

                SearcherWindow.Show(editorWindow, searcher, AddType,
                    button.worldBound.min + Vector2.up * 15.0f, a => { },
                    new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top, SearcherWindow.Alignment.Horizontal.Left));
            });
            addComponentButton.SetEnabled(m_LastEditState);
            root.contentContainer.Add(addComponentButton);

            var addComponentButtonUpdater = UIUpdaters.MakeBinding(config, addComponentButton);
            addComponentButtonUpdater.OnPreUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            addComponentButton.binding = addComponentButtonUpdater;
        }

        static bool IsShown(Type t) => t.GetCustomAttribute<HideInInspector>() == null;

        bool AddType(SearcherItem arg)
        {
            if (!(arg is TypeSearcherItem typeItem))
            {
                return false;
            }

            var config = extraDataTarget as BuildConfiguration;
            if (config == null)
            {
                return false;
            }

            var type = typeItem.Type;
            config.SetComponent(type, TypeConstruction.Construct<IBuildComponent>(type));
            Refresh(m_BuildConfigurationRoot);
            return true;

        }

        VisualElement GetComponentElement(BuildConfiguration container, object component)
        {
            var componentType = component.GetType();
            var element = (VisualElement)Activator.CreateInstance(typeof(HierarchicalComponentContainerElement<,,>)
                .MakeGenericType(typeof(BuildConfiguration), typeof(IBuildComponent), componentType), container, component);
            return element;
        }
    }
}
