using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Build.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using PropertyAttribute = Unity.Properties.PropertyAttribute;

namespace Unity.Build.Tests
{
    [TestFixture]
    class BuildPipelineTests : BuildTestsBase
    {
        [Test]
        public void CreateAsset()
        {
            const string assetPath = "Assets/" + nameof(BuildPipelineTests) + BuildPipeline.AssetExtension;
            Assert.That(BuildPipeline.CreateAsset(assetPath), Is.Not.Null);
            AssetDatabase.DeleteAsset(assetPath);
        }

        [Test]
        public void CanBuild_IsTrue()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestRequiredComponentA());
                c.SetComponent(new TestOptionalComponentA());
            });
            Assert.That(pipeline.CanBuild(config, out var _), Is.True);
        }

        [Test]
        public void CanBuild_WithMissingRequiredComponent_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestOptionalComponentA()));
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
        }

        [Test]
        public void CanBuild_WithInvalidRequiredComponent_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(new TestBuildStepWithInvalidRequiredComponent()));
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
        }

        [Test]
        public void CanBuild_WithMissingOptionalComponent_IsTrue()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            Assert.That(pipeline.CanBuild(config, out var _), Is.True);
        }

        [Test]
        public void CanBuild_WithInvalidOptionalComponent_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(new TestBuildStepWithInvalidOptionalComponent()));
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
        }

        [Test]
        public void CanBuild_WithInvalidBuildStepOrder_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
        }

        [Test]
        public void CanBuild_NestedPipeline_IsTrue()
        {
            var nested = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(nested));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestRequiredComponentA());
                c.SetComponent(new TestOptionalComponentA());
            });
            Assert.That(pipeline.CanBuild(config, out var _), Is.True);
        }

        [Test]
        public void CanBuild_NestedPipeline_WithMissingRequiredComponent_IsFalse()
        {
            var nested = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(nested));
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestOptionalComponentA()));
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
        }

        [Test]
        public void CanBuild_NestedPipeline_WithMissingOptionalComponent_IsTrue()
        {
            var nested = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(nested));
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            Assert.That(pipeline.CanBuild(config, out var _), Is.True);
        }

        [Test]
        public void CanBuild_NestedPipeline_WithInvalidBuildStepOrder_IsFalse()
        {
            var nested = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(nested));
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
        }

        [Test]
        public void Build_Succeeds()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            using (var progress = new BuildProgress("Building...", "Please wait!"))
            {
                var result = pipeline.Build(config, progress, (context) =>
                {
                    context.SetValue(new TestBuildStep1.Data { Value = nameof(TestBuildStep1) });
                    context.SetValue(new TestBuildStep2.Data { Value = nameof(TestBuildStep2) });
                    context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
                });
                Assert.That(result.Succeeded, Is.True);
                Assert.That(result.BuildStepsResults.Any(r => r.Failed), Is.False);
            }
        }

        [Test]
        public void Build_WithoutProgress_Succeeds()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            var result = pipeline.Build(config, null, (context) =>
            {
                context.SetValue(new TestBuildStep1.Data { Value = nameof(TestBuildStep1) });
                context.SetValue(new TestBuildStep2.Data { Value = nameof(TestBuildStep2) });
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.BuildStepsResults.Any(r => r.Failed), Is.False);
        }

        [Test]
        public void Build_WithoutMutator_Succeeds()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            using (var progress = new BuildProgress("Building...", "Please wait!"))
            {
                var result = pipeline.Build(config, progress);
                Assert.That(result.Succeeded, Is.True);
            }
        }

        //[Test]
        //public void Build_WhileUnityIsCompiling_Fails()
        //{
        //    //@TODO: How to test this?
        //}

        [Test]
        public void Build_WhenCanBuildIsFalse_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(new TestBuildStep1()));
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.CanBuild(config, out var _), Is.False);
            Assert.That(pipeline.Build(config).Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenBuildStepFails_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(new TestBuildStepFailure()));
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.False);
        }

        [Test]
        public void Build_WhenBuildStepThrows_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(new TestBuildStepThrows()));
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.False);
        }

        [Test]
        public void Build_DisabledBuildSteps_DoesNotRun()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2(enabled: false));
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            var result = pipeline.Build(config, mutator: (context) =>
            {
                context.SetValue(new TestBuildStep1.Data { Value = nameof(TestBuildStep1) });
                context.SetValue(new TestBuildStep2.Data { Value = nameof(TestBuildStep2) });
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.BuildStepsResults.Any(r => r.Failed), Is.False);

            var artifacts = BuildArtifacts.GetBuildArtifact<TestArtifactA>(config);
            Assert.That(artifacts, Is.Not.Null);
            Assert.That(artifacts.BuildStepsRan, Is.EqualTo(new List<string> { nameof(TestBuildStep1), nameof(TestBuildStep3) }));
            Assert.That(artifacts.CleanupStepsRan, Is.EqualTo(new List<string> { nameof(TestBuildStep3), nameof(TestBuildStep1) }));
        }

        [Test]
        public void WhenBuildSucceeds_AllBuildStepsAndCleanupStepsRan()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            var result = pipeline.Build(config, mutator: (context) =>
            {
                context.SetValue(new TestBuildStep1.Data { Value = nameof(TestBuildStep1) });
                context.SetValue(new TestBuildStep2.Data { Value = nameof(TestBuildStep2) });
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });
            var artifacts = BuildArtifacts.GetBuildArtifact<TestArtifactA>(config);
            Assert.That(artifacts, Is.Not.Null);
            Assert.That(artifacts.BuildStepsRan, Is.EqualTo(new List<string> { nameof(TestBuildStep1), nameof(TestBuildStep2), nameof(TestBuildStep3) }));
            Assert.That(artifacts.CleanupStepsRan, Is.EqualTo(new List<string> { nameof(TestBuildStep3), nameof(TestBuildStep2), nameof(TestBuildStep1) }));
        }

        [Test]
        public void WhenBuildFails_BuildStepsStopAtFailure_CleanupStepsStopAtFailure()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            var result = pipeline.Build(config, mutator: (context) =>
            {
                context.SetValue(new TestBuildStep1.Data { Value = nameof(TestBuildStep1) });
                // Here we make TestStep2 fails by not providing its data
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.BuildStepsResults.Any(r => r.Failed), Is.True);

            var artifacts = BuildArtifacts.GetBuildArtifact<TestArtifactA>(config);
            Assert.That(artifacts, Is.Not.Null);
            Assert.That(artifacts.BuildStepsRan, Is.EqualTo(new List<string> { nameof(TestBuildStep1), nameof(TestBuildStep2) }));
            Assert.That(artifacts.CleanupStepsRan, Is.EqualTo(new List<string> { nameof(TestBuildStep2), nameof(TestBuildStep1) }));
        }

        [Test]
        public void WhenNestedBuildPipelineFails_ParentBuildPipelineFails()
        {
            var nestedPipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(new TestBuildStepFailure()));
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(nestedPipeline));
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.False);
        }

        [Test]
        public void CanRun_IsTrue()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            Assert.That(pipeline.CanRun(config, out var _), Is.True);
        }

        [Test]
        public void CanRun_WithoutBuild_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.CanRun(config, out var _), Is.False);
        }

        [Test]
        public void CanRun_WithFailedBuild_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStepFailure());
                p.RunStep = new TestRunStep();
            });
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.False);
            Assert.That(pipeline.CanRun(config, out var _), Is.False);
        }

        [Test]
        public void CanRun_WithoutRunStep_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            Assert.That(pipeline.CanRun(config, out var _), Is.False);
        }

        [Test]
        public void CanRun_WhenRunStepCannotRun_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepCannotRun());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            Assert.That(pipeline.CanRun(config, out var _), Is.False);
        }

        [Test]
        public void Run_Succeeds()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            using (var result = pipeline.Run(config))
            {
                Assert.That(result.Succeeded, Is.True);
            }
        }

        [Test]
        public void Run_WhenCanRunIsFalse_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            Assert.That(pipeline.CanRun(config, out var _), Is.False);
            Assert.That(pipeline.Run(config).Succeeded, Is.False);
        }

        [Test]
        public void Run_WithoutBuild_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance();
            using (var runResult = pipeline.Run(config))
            {
                Assert.That(runResult.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithFailedBuild_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStepFailure());
                p.RunStep = new TestRunStep();
            });
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.False);
            using (var runResult = pipeline.Run(config))
            {
                Assert.That(runResult.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithoutRunStep_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            using (var result = pipeline.Run(config))
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunStepCannotRun_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepCannotRun());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            using (var result = pipeline.Run(config))
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunStepFails_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepFailure());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            using (var result = pipeline.Run(config))
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunStepThrows_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepThrows());
            var config = BuildConfiguration.CreateInstance();
            Assert.That(pipeline.Build(config).Succeeded, Is.True);
            using (var result = pipeline.Run(config))
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }
    }

    [TestFixture]
    class BuildPipelineExtensionTests
    {
        const string k_TestRootFolder = "Assets/Tests/BuildPipelineExtensions/";
        const string k_TestBuildBuildpipelineAssetPath = k_TestRootFolder + "TestBuildPipeline" + BuildPipeline.AssetExtension;
        const string k_TestBuildConfiguration32AssetPath = k_TestRootFolder + "TestBuildConfiguration32" + BuildConfiguration.AssetExtension;
        const string k_TestBuildConfiguration64AssetPath = k_TestRootFolder + "TestBuildConfiguration64" + BuildConfiguration.AssetExtension;
        const string k_TestsContainer = k_TestRootFolder + "Container.asset";

        [HideInInspector]
        sealed class FakeClassicBuildProfile : IBuildPipelineComponent
        {
            [Property]
            public BuildPipeline Pipeline { get; set; }

            public int SortingIndex => (int)Target;

            public bool SetupEnvironment()
            {
                if (Target == EditorUserBuildSettings.activeBuildTarget)
                    return false;
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(Target), Target))
                    throw new Exception($"Failed to switch active build target to {Target}");
                return true;
            }

            [Property]
            public UnityEditor.BuildTarget Target { get; set; }
        }

        [HideInInspector]
        class TestBuildProfileComponent : IBuildPipelineComponent
        {
            [Property] public BuildPipeline Pipeline { get; set; }
            public int SortingIndex => 0;
            public bool SetupEnvironment() => false;
        }

        [HideInInspector]
        sealed class TestBuildStepSuccess : BuildStep
        {
            public override Type[] OptionalComponents => new[] { typeof(OutputBuildDirectory) };

            public override BuildStepResult RunBuildStep(BuildContext context)
            {
                Directory.CreateDirectory(this.GetOutputBuildDirectory(context));
                var path = Path.Combine(this.GetOutputBuildDirectory(context), "Result.txt");
                File.WriteAllText(path, "success");
                return Success();
            }
        }

        // Note: To survive domain reload, the settings have to fields and serializables
        [SerializeField] BuildConfiguration m_SettingsWindows64;
        [SerializeField] BuildConfiguration m_SettingsWindows32;
        [SerializeField] BuildPipeline m_BuildPipeline;
        [SerializeField] UnityEditor.BuildTarget m_OriginalBuildTarget;
        [SerializeField] BuildTargetGroup m_OriginalBuildTargetGroup;
        [SerializeField] ResultContainer m_Container;

        private UnityEditor.BuildTarget Standalone32Target
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        return UnityEditor.BuildTarget.StandaloneWindows;
                    case RuntimePlatform.OSXEditor:
                        // No 32 bit target on OSX
                        return UnityEditor.BuildTarget.StandaloneOSX;
                    default:
                        throw new NotImplementedException("Please implement for " + Application.platform);
                }
            }
        }

        private UnityEditor.BuildTarget Standalone64Target
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        return UnityEditor.BuildTarget.StandaloneWindows64;
                    case RuntimePlatform.OSXEditor:
                        return UnityEditor.BuildTarget.StandaloneOSX;
                    default:
                        throw new NotImplementedException("Please implement for " + Application.platform);
                }
            }
        }

        [TearDown]
        public void Teardown()
        {
            BuildPipeline.CancelBuildAsync();
            AssetDatabase.DeleteAsset(k_TestRootFolder);
        }

        /// <summary>
        /// Test build case where Editor target switch is required
        /// Note: Currently we can only effectively test this on Windows, because we have there 32 & 64 bit targets, thus we'll need target switch.
        ///       On OSX, there's only 64 bit target, so no real target switch will be required
        /// </summary>
        /// <returns></returns>

        // Disable for now, this test seems to affect tests which are ran after , for ex.,
        //        VerifyNoCompilerErrors(0.005s) from com.unity.platforms.desktop\Tests\Editor\BasicTests.cs
        //---
        //System.Reflection.TargetException : Non-static method requires a target.
        //---
        //at System.Reflection.MonoMethod.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture)[0x0004c] in <437ba245d8404784b9fbab9b439ac908>:0 
        //  at System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters)[0x00000] in <437ba245d8404784b9fbab9b439ac908>:0 
        //  at NUnit.Framework.Internal.Reflect.InvokeMethod (System.Reflection.MethodInfo method, System.Object fixture, System.Object[] args)[0x0005e] in <59819be142c34115ade688f6962021f1>:0
        // https://fogbugz.unity3d.com/f/cases/1205240/
        // [UnityTest]
        public IEnumerator CanBuildMultipleBuildsWithActiveTargetSwitch()
        {
            m_BuildPipeline = BuildPipeline.CreateAsset(k_TestBuildBuildpipelineAssetPath,
                (p) =>
                {
                    p.BuildSteps.Add(new TestBuildStepSuccess());
                });

            m_SettingsWindows32 = BuildConfiguration.CreateAsset(k_TestBuildConfiguration32AssetPath, (bs) =>
            {
                bs.SetComponent(new FakeClassicBuildProfile()
                {
                    Target = Standalone32Target,
                    Pipeline = m_BuildPipeline
                });
            });

            m_SettingsWindows64 = BuildConfiguration.CreateAsset(k_TestBuildConfiguration64AssetPath, (bs) =>
            {
                bs.SetComponent(new FakeClassicBuildProfile()
                {
                    Target = Standalone64Target,
                    Pipeline = m_BuildPipeline
                });
            });

            m_Container = ResultContainer.CreateInstance<ResultContainer>();
            m_Container.Results = null;
            m_Container.Completed = false;
            AssetDatabase.CreateAsset(m_Container, k_TestsContainer);
            AssetDatabase.ImportAsset(k_TestsContainer, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            m_Container = AssetDatabase.LoadAssetAtPath<ResultContainer>(k_TestsContainer);

            m_OriginalBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            m_OriginalBuildTargetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(m_OriginalBuildTarget);

            // Leave this for testing purposes
            //if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, UnityEditor.BuildTarget.Android))
            //    yield return new RecompileScripts(false);

            var serttingsToBuilds = new[] { m_SettingsWindows32, m_SettingsWindows64 };
            BuildPipeline.BuildAsync(new BuildBatchDescription()
            {
                BuildItems = serttingsToBuilds.Select(m => new BuildBatchItem() { BuildConfiguration = m }).ToArray(),
                OnBuildCompleted = m_Container.SetCompleted
            });

            while (m_Container.Completed == false)
            {
                yield return new RecompileScripts(false);
            }

            Assert.IsTrue(EditorUserBuildSettings.activeBuildTarget == m_OriginalBuildTarget);
            Assert.IsTrue(m_Container.Results != null);
            Assert.IsTrue(m_Container.Results.Contains(m_SettingsWindows32.name + ", Success"));
            Assert.IsTrue(m_Container.Results.Contains(m_SettingsWindows64.name + ", Success"));
        }
    }
}
