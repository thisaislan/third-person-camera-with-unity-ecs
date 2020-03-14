using NUnit.Framework;
using System;
using System.Linq;
using UnityEditor;

namespace Unity.Build.Tests
{
    class BuildStepTests : BuildTestsBase
    {
        [Test]
        public void Name_IsValid()
        {
            Assert.That(new TestBuildStep().Name, Is.EqualTo("this is a name"));
            Assert.That(new TestBuildStepWithRequirements().Name, Is.EqualTo(nameof(TestBuildStepWithRequirements)));
        }

        [Test]
        public void Description_IsValid()
        {
            Assert.That(new TestBuildStep().Description, Is.EqualTo("this is a description"));
            Assert.That(new TestBuildStepWithRequirements().Description, Is.EqualTo($"Running {BuildStep.GetName<TestBuildStepWithRequirements>()}"));
        }

        [Test]
        public void Category_IsValid()
        {
            Assert.That(new TestBuildStep().Category, Is.EqualTo("this is a category"));
            Assert.That(new TestBuildStepWithRequirements().Category, Is.EqualTo(string.Empty));
        }

        [Test]
        public void IsShown_IsFalse()
        {
            Assert.That(new TestBuildStep().IsShown, Is.False);
            Assert.That(new TestBuildStepWithRequirements().IsShown, Is.False);
        }

        [Test]
        public void CleanupBuildStep_DefaultImplementation_IsNotCalled()
        {
            var step = new TestBuildStep();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var result = pipeline.Build(config);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.BuildStepsResults.Count, Is.EqualTo(1));
        }

        [Test]
        public void CleanupBuildStep_DefaultImplementation_Throws()
        {
            var step = new TestBuildStep();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.CleanupBuildStep(context));
        }

        [Test]
        public void HasRequiredComponent_IsTrue()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestRequiredComponentA());
                c.SetComponent(new TestRequiredComponentB());
            });
            var context = new BuildContext(pipeline, config);
            Assert.That(step.HasRequiredComponent<TestRequiredComponentA>(context), Is.True);
            Assert.That(step.HasRequiredComponent<TestRequiredComponentB>(context), Is.True);
        }

        [Test]
        public void HasRequiredComponent_WithMissingRequiredComponent_IsFalse()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.That(step.HasRequiredComponent<TestRequiredComponentA>(context), Is.False);
            Assert.That(step.HasRequiredComponent<TestRequiredComponentB>(context), Is.False);
        }

        [Test]
        public void HasRequiredComponent_WithInvalidType_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.HasRequiredComponent<TestOptionalComponentA>(context));
            Assert.Throws<ArgumentNullException>(() => step.HasRequiredComponent(context, null));
            Assert.Throws<InvalidOperationException>(() => step.HasRequiredComponent(context, typeof(object)));
            Assert.Throws<InvalidOperationException>(() => step.HasRequiredComponent(context, typeof(TestInvalidComponent)));
        }

        [Test]
        public void GetRequiredComponent_IsValid()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestRequiredComponentA());
                c.SetComponent(new TestRequiredComponentB());
            });
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetRequiredComponent<TestRequiredComponentA>(context), Is.Not.Null);
            Assert.That(step.GetRequiredComponent<TestRequiredComponentB>(context), Is.Not.Null);
        }

        [Test]
        public void GetRequiredComponent_WithMissingRequiredComponent_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponent<TestRequiredComponentA>(context));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponent<TestRequiredComponentB>(context));
        }

        [Test]
        public void GetRequiredComponent_WithInvalidType_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponent<TestOptionalComponentA>(context));
            Assert.Throws<ArgumentNullException>(() => step.GetRequiredComponent(context, null));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponent(context, typeof(object)));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponent(context, typeof(TestInvalidComponent)));
        }

        [Test]
        public void GetRequiredComponents_IsValid()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestRequiredComponentA());
                c.SetComponent(new TestRequiredComponentB());
            });
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetRequiredComponents(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestRequiredComponentA), typeof(TestRequiredComponentB) }));
            Assert.That(step.GetRequiredComponents<TestRequiredComponentA>(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestRequiredComponentA) }));
            Assert.That(step.GetRequiredComponents<TestRequiredComponentB>(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestRequiredComponentB) }));
        }

        [Test]
        public void GetRequiredComponents_WithStepWithoutRequiredComponents_IsValid()
        {
            var step = new TestBuildStep();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetRequiredComponents(context), Is.Empty);
        }

        [Test]
        public void GetRequiredComponents_WithMissingRequiredComponent_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponents(context));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponents<TestRequiredComponentA>(context));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponents<TestRequiredComponentB>(context));
        }

        [Test]
        public void GetRequiredComponents_WithInvalidType_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponents<TestOptionalComponentA>(context));
            Assert.Throws<ArgumentNullException>(() => step.GetRequiredComponents(context, null));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponents(context, typeof(object)));
            Assert.Throws<InvalidOperationException>(() => step.GetRequiredComponents(context, typeof(TestInvalidComponent)));
        }

        [Test]
        public void HasOptionalComponent_IsTrue()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestOptionalComponentA());
                c.SetComponent(new TestOptionalComponentB());
            });
            var context = new BuildContext(pipeline, config);
            Assert.That(step.HasOptionalComponent<TestOptionalComponentA>(context), Is.True);
            Assert.That(step.HasOptionalComponent<TestOptionalComponentB>(context), Is.True);
        }

        [Test]
        public void HasOptionalComponent_WithMissingOptionalComponent_IsFalse()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.That(step.HasOptionalComponent<TestOptionalComponentA>(context), Is.False);
            Assert.That(step.HasOptionalComponent<TestOptionalComponentB>(context), Is.False);
        }

        [Test]
        public void HasOptionalComponent_WithInvalidType_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.HasOptionalComponent<TestRequiredComponentA>(context));
            Assert.Throws<ArgumentNullException>(() => step.HasOptionalComponent(context, null));
            Assert.Throws<InvalidOperationException>(() => step.HasOptionalComponent(context, typeof(object)));
            Assert.Throws<InvalidOperationException>(() => step.HasOptionalComponent(context, typeof(TestInvalidComponent)));
        }

        [Test]
        public void GetOptionalComponent_IsValid()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestOptionalComponentA());
                c.SetComponent(new TestOptionalComponentB());
            });
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetOptionalComponent<TestOptionalComponentA>(context), Is.Not.Null);
            Assert.That(step.GetOptionalComponent<TestOptionalComponentB>(context), Is.Not.Null);
        }

        [Test]
        public void GetOptionalComponent_WithMissingOptionalComponent_IsValid()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetOptionalComponent<TestOptionalComponentA>(context), Is.Not.Null);
            Assert.That(step.GetOptionalComponent<TestOptionalComponentB>(context), Is.Not.Null);
        }

        [Test]
        public void GetOptionalComponent_WithInvalidType_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.GetOptionalComponent<TestRequiredComponentA>(context));
            Assert.Throws<ArgumentNullException>(() => step.GetOptionalComponent(context, null));
            Assert.Throws<InvalidOperationException>(() => step.GetOptionalComponent(context, typeof(object)));
            Assert.Throws<InvalidOperationException>(() => step.GetOptionalComponent(context, typeof(TestInvalidComponent)));
        }

        [Test]
        public void GetOptionalComponents_IsValid()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance((c) =>
            {
                c.SetComponent(new TestOptionalComponentA());
                c.SetComponent(new TestOptionalComponentB());
            });
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetOptionalComponents(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestOptionalComponentA), typeof(TestOptionalComponentB) }));
            Assert.That(step.GetOptionalComponents<TestOptionalComponentA>(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestOptionalComponentA) }));
            Assert.That(step.GetOptionalComponents<TestOptionalComponentB>(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestOptionalComponentB) }));
        }

        [Test]
        public void GetOptionalComponents_WithMissingOptionalComponent_IsValid()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetOptionalComponents(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestOptionalComponentA), typeof(TestOptionalComponentB) }));
            Assert.That(step.GetOptionalComponents<TestOptionalComponentA>(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestOptionalComponentA) }));
            Assert.That(step.GetOptionalComponents<TestOptionalComponentB>(context).Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestOptionalComponentB) }));
        }

        [Test]
        public void GetOptionalComponents_WithInvalidType_Throws()
        {
            var step = new TestBuildStepWithRequirements();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.Throws<InvalidOperationException>(() => step.GetOptionalComponents<TestRequiredComponentA>(context));
            Assert.Throws<ArgumentNullException>(() => step.GetOptionalComponents(context, null));
            Assert.Throws<InvalidOperationException>(() => step.GetOptionalComponents(context, typeof(object)));
            Assert.Throws<InvalidOperationException>(() => step.GetOptionalComponents(context, typeof(TestInvalidComponent)));
        }

        [Test]
        public void GetOptionalComponents_WithStepWithoutOptionalComponents_IsValid()
        {
            var step = new TestBuildStep();
            var pipeline = BuildPipeline.CreateInstance(p => p.BuildSteps.Add(step));
            var config = BuildConfiguration.CreateInstance();
            var context = new BuildContext(pipeline, config);
            Assert.That(step.GetOptionalComponents(context), Is.Empty);
        }

        [Test]
        public void GetName_IsValid()
        {
            Assert.That(BuildStep.GetName<TestBuildStep>(), Is.EqualTo("this is a name"));
            Assert.That(BuildStep.GetName<TestBuildStepWithRequirements>(), Is.EqualTo(nameof(TestBuildStepWithRequirements)));
        }

        [Test]
        public void GetName_WithPipelineType_IsValid()
        {
            Assert.That(BuildStep.GetName<BuildPipeline>(), Is.EqualTo(nameof(BuildPipeline)));
        }

        [Test]
        public void GetName_WithInvalidType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => BuildStep.GetName(null));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetName(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetName(typeof(TestInvalidBuildStep)));
        }

        [Test]
        public void GetDescription_IsValid()
        {
            Assert.That(BuildStep.GetDescription<TestBuildStep>(), Is.EqualTo("this is a description"));
            Assert.That(BuildStep.GetDescription<TestBuildStepWithRequirements>(), Is.EqualTo($"Running {BuildStep.GetName<TestBuildStepWithRequirements>()}"));
        }

        [Test]
        public void GetDescription_WithPipelineType_IsValid()
        {
            Assert.That(BuildStep.GetDescription<BuildPipeline>(), Is.EqualTo($"Running {BuildStep.GetName<BuildPipeline>()}"));
        }

        [Test]
        public void GetDescription_WithInvalidType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => BuildStep.GetDescription(null));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetDescription(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetDescription(typeof(TestInvalidBuildStep)));
        }

        [Test]
        public void GetCategory_IsValid()
        {
            Assert.That(BuildStep.GetCategory<TestBuildStep>(), Is.EqualTo("this is a category"));
            Assert.That(BuildStep.GetCategory<TestBuildStepWithRequirements>(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCategory_WithPipelineType_IsValid()
        {
            Assert.That(BuildStep.GetCategory<BuildPipeline>(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetCategory_WithInvalidType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => BuildStep.GetCategory(null));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetCategory(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetCategory(typeof(TestInvalidBuildStep)));
        }

        [Test]
        public void GetIsShown_IsValid()
        {
            Assert.That(BuildStep.GetIsShown<TestBuildStep>(), Is.False);
            Assert.That(BuildStep.GetIsShown<TestBuildStepWithRequirements>(), Is.False);
        }

        [Test]
        public void GetIsShown_WithPipelineType_IsValid()
        {
            Assert.That(BuildStep.GetIsShown<BuildPipeline>(), Is.False);
        }

        [Test]
        public void GetIsShown_WithInvalidType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => BuildStep.GetIsShown(null));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetIsShown(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => BuildStep.GetIsShown(typeof(TestInvalidBuildStep)));
        }

        [Test]
        public void Serialize_BuildStep_IsValid()
        {
            var step = new TestBuildStep();
            var type = step.GetType();
            Assert.That(BuildStep.Serialize(step), Is.EqualTo(type.GetFullyQualifedAssemblyTypeName()));
        }

        [Test]
        public void Serialize_PipelineAsBuildStep_IsValid()
        {
            var assetPath = $"Assets/TestBuildPipeline{BuildPipeline.AssetExtension}";
            var pipeline = BuildPipeline.CreateAsset(assetPath);
            var pipelineId = GlobalObjectId.GetGlobalObjectIdSlow(pipeline);
            Assert.That(BuildStep.Serialize(pipeline), Is.EqualTo(pipelineId.ToString()));
            AssetDatabase.DeleteAsset(assetPath);
        }

        [Test]
        public void Serialize_InvalidBuildStep_IsNull()
        {
            Assert.That(BuildStep.Serialize(null), Is.Null);
        }

        [Test]
        public void Deserialize_BuildStep_IsValid()
        {
            var type = typeof(TestBuildStep);
            var step = BuildStep.Deserialize(type.GetFullyQualifedAssemblyTypeName());
            Assert.That(step, Is.Not.Null);
            Assert.That(step.GetType(), Is.EqualTo(type));
        }

        [Test]
        public void Deserialize_PipelineAsBuildStep_IsValid()
        {
            var assetPath = $"Assets/TestBuildPipeline{BuildPipeline.AssetExtension}";
            var pipeline = BuildPipeline.CreateAsset(assetPath);
            var pipelineId = GlobalObjectId.GetGlobalObjectIdSlow(pipeline);
            Assert.That(BuildStep.Deserialize(pipelineId.ToString()), Is.EqualTo(pipeline));
            AssetDatabase.DeleteAsset(assetPath);
        }

        [Test]
        public void Deserialize_InvalidJson_IsNull()
        {
            Assert.That(BuildStep.Deserialize(null), Is.Null);
            Assert.That(BuildStep.Deserialize(string.Empty), Is.Null);
            Assert.That(BuildStep.Deserialize("abc"), Is.Null);
        }

        [Test]
        public void GetAvailableTypes_IsValid()
        {
            var derivedTypes = FindAllDerivedTypes<IBuildStep>();
            Assert.That(BuildStep.GetAvailableTypes(), Is.EquivalentTo(derivedTypes));
            Assert.That(BuildStep.GetAvailableTypes(type => type == typeof(TestBuildStep)), Is.EquivalentTo(derivedTypes.Where(type => type == typeof(TestBuildStep))));
        }
    }
}
