using System;
using System.Collections.Generic;
using System.IO;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.Searcher;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    [CustomEditor(typeof(BuildPipelineScriptedImporter))]
    sealed class BuildPipelineScriptedImporterEditor : ScriptedImporterEditor
    {
        Label m_HeaderLabel;
        ReorderableList m_BuildStepsList;
        TextField m_RunStepTextInput;

        public override bool showImportedObject { get; } = false;

        protected override Type extraDataType => typeof(BuildPipeline);

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

            var pipeline = extraData as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return;
            }

            if (BuildPipeline.DeserializeFromPath(pipeline, assetImporter.assetPath))
            {
                pipeline.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
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

                var pipeline = extraDataTargets[i] as BuildPipeline;
                if (pipeline == null || !pipeline)
                {
                    continue;
                }

                pipeline.SerializeToPath(assetImporter.assetPath);
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

                var pipeline = extraDataTargets[i] as BuildPipeline;
                if (pipeline == null || !pipeline)
                {
                    continue;
                }

                if (BuildPipeline.DeserializeFromPath(pipeline, assetImporter.assetPath))
                {
                    pipeline.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = Assets.LoadVisualTreeAsset("BuildPipelineCustomInspector").CloneTree();
            root.AddStyleSheetAndVariant("BuildPipelineCustomInspector");
            Refresh(root);
            return root;
        }

        void Refresh(BindableElement root)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return;
            }

            RefreshHeader(root, pipeline);
            RefreshBuildSteps(root, pipeline);
            RefreshRunStep(root, pipeline);
        }

        void RefreshHeader(BindableElement root, BuildPipeline pipeline)
        {
            m_HeaderLabel = root.Q<Label>(className: "InspectorHeader__Label");
            m_HeaderLabel.text = pipeline.name + " (Build Pipeline Asset)";
        }

        void RefreshBuildSteps(BindableElement root, BuildPipeline pipeline)
        {
            var elements = pipeline.BuildSteps ?? new List<IBuildStep>();
            m_BuildStepsList = new ReorderableList(elements, typeof(IBuildStep), true, true, true, true);
            m_BuildStepsList.headerHeight = 3;
            m_BuildStepsList.onAddDropdownCallback = AddDropdownCallbackDelegate;
            m_BuildStepsList.drawElementCallback = ElementCallbackDelegate;
            m_BuildStepsList.drawHeaderCallback = HeaderCallbackDelegate;
            m_BuildStepsList.onReorderCallback = ReorderCallbackDelegate;
            m_BuildStepsList.onRemoveCallback = RemoveCallbackDelegate;
            m_BuildStepsList.drawFooterCallback = FooterCallbackDelegate;
            m_BuildStepsList.drawNoneElementCallback = DrawNoneElementCallback;
            m_BuildStepsList.elementHeightCallback = ElementHeightCallbackDelegate;

            root.Q<VisualElement>("BuildSteps__IMGUIContainer").Add(new IMGUIContainer(m_BuildStepsList.DoLayoutList));
            root.Q<VisualElement>("ApplyRevertButtons").Add(new IMGUIContainer(ApplyRevertGUI));
        }

        void RefreshRunStep(BindableElement root, BuildPipeline pipeline)
        {
            m_RunStepTextInput = root.Q<TextField>("RunStep__RunStepTypeName");
            m_RunStepTextInput.value = pipeline.RunStep?.Name ?? string.Empty;
            root.Q<Button>("RunStep__SelectButton").clickable.clickedWithEventInfo += OnRunStepSelectorClicked;
        }

        static string GetBuildStepDisplayName(Type type)
        {
            var name = BuildStep.GetName(type);
            var category = BuildStep.GetCategory(type);
            return !string.IsNullOrEmpty(category) ? $"{category}/{name}" : name;
        }

        static string GetRunStepDisplayName(Type type)
        {
            var name = RunStep.GetName(type);
            var category = RunStep.GetCategory(type);
            return !string.IsNullOrEmpty(category) ? $"{category}/{name}" : name;
        }

        bool AddStep(SearcherItem item)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return false;
            }

            if (item is TypeSearcherItem typeItem)
            {
                if (TypeConstruction.TryConstruct<IBuildStep>(typeItem.Type, out var step))
                {
                    pipeline.BuildSteps.Add(step);
                    return true;
                }
            }
            return false;
        }

        void AddDropdownCallbackDelegate(Rect buttonRect, ReorderableList list)
        {
            var databases = new[]
            {
                TypeSearcherDatabase.GetBuildStepsDatabase(new HashSet<Type>(BuildStep.GetAvailableTypes(type => !BuildStep.GetIsShown(type))), GetBuildStepDisplayName),
            };

            var searcher = new Searcher(databases, new AddTypeSearcherAdapter("Add Build Step"));
            var editorWindow = EditorWindow.focusedWindow;
            SearcherWindow.Show(
                editorWindow,
                searcher,
                AddStep,
                buttonRect.min + Vector2.up * 35.0f,
                a => { },
                new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top,
                    SearcherWindow.Alignment.Horizontal.Left)
            );
        }

        void HandleDragDrop(Rect rect, int index)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return;
            }

            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.ContextClick:

                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (IBuildStep step in DragAndDrop.objectReferences)
                        {
                            pipeline.BuildSteps.Insert(index, step);
                        }
                    }
                    break;
            }
        }

        void DrawNoneElementCallback(Rect rect)
        {
            ReorderableList.defaultBehaviours.DrawNoneElement(rect, false);
            HandleDragDrop(rect, 0);
        }

        void FooterCallbackDelegate(Rect rect)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return;
            }

            ReorderableList.defaultBehaviours.DrawFooter(rect, m_BuildStepsList);
            HandleDragDrop(rect, pipeline.BuildSteps.Count);
        }

        void ElementCallbackDelegate(Rect rect, int index, bool isActive, bool isFocused)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return;
            }

            var step = pipeline.BuildSteps[index];
            var labelRect = rect;
            if (!BuildPipeline.ValidateBuildStepPosition(pipeline.BuildSteps, index, out var reasons))
            {
                labelRect = new Rect(rect.x, rect.y, rect.width, m_BuildStepsList.elementHeight);
                for (var i = 0; i < reasons.Length; i++)
                {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y + (m_BuildStepsList.elementHeight + ReorderableList.Defaults.padding) * (i + 1), rect.width, m_BuildStepsList.elementHeight), reasons[i], MessageType.Error);
                }
            }

            if (step is BuildPipeline buildPipeline)
            {
                GUI.Label(labelRect, buildPipeline.name + " (Build Pipeline Asset)");
            }
            else if (step is BuildStep buildStep)
            {
                GUI.Label(labelRect, buildStep.Name);
            }

            HandleDragDrop(rect, index);
        }

        float ElementHeightCallbackDelegate(int index)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return m_BuildStepsList.elementHeight;
            }

            BuildPipeline.ValidateBuildStepPosition(pipeline.BuildSteps, index, out var reasons);
            return m_BuildStepsList.elementHeight + (reasons != null ? (m_BuildStepsList.elementHeight + ReorderableList.Defaults.padding) * reasons.Length : 0f);
        }

        void ReorderCallbackDelegate(ReorderableList list)
        {
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            HandleDragDrop(rect, 0);
        }

        void RemoveCallbackDelegate(ReorderableList list)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return;
            }

            if (pipeline.BuildSteps == null)
            {
                return;
            }

            if (pipeline.BuildSteps.Count <= list.index)
            {
                return;
            }

            pipeline.BuildSteps.RemoveAt(list.index);
        }

        void OnRunStepSelectorClicked(EventBase @event)
        {
            SearcherWindow.Show(
                EditorWindow.focusedWindow,
                new Searcher(
                    TypeSearcherDatabase.GetRunStepDatabase(new HashSet<Type>(RunStep.GetAvailableTypes(type => !RunStep.GetIsShown(type))), GetRunStepDisplayName),
                    new AddTypeSearcherAdapter("Select Run Script")),
                UpdateRunStep,
                @event.originalMousePosition + Vector2.up * 35.0f,
                a => { },
                new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top,
                                             SearcherWindow.Alignment.Horizontal.Left)
            );
        }

        bool UpdateRunStep(SearcherItem item)
        {
            var pipeline = extraDataTarget as BuildPipeline;
            if (pipeline == null || !pipeline)
            {
                return false;
            }

            if (item is TypeSearcherItem typeItem)
            {
                if (TypeConstruction.TryConstruct<RunStep>(typeItem.Type, out var step))
                {
                    pipeline.RunStep = step;
                    m_RunStepTextInput.value = step.Name ?? string.Empty;
                    return true;
                }
            }
            return false;
        }
    }
}
