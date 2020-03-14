using NUnit.Framework;

namespace Unity.Build.Tests
{
    class BuildConfigurationTests : BuildTestsBase
    {
        [Test]
        public void CreateAsset()
        {
            const string assetPath = "Assets/" + nameof(BuildConfigurationTests) + BuildConfiguration.AssetExtension;
            Assert.That(BuildConfiguration.CreateAsset(assetPath), Is.Not.Null);
            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
        }

        [Test]
        public void GetBuildPipeline_IsEqualToPipeline()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.GetBuildPipeline(), Is.EqualTo(pipeline));
        }

        [Test]
        public void GetBuildPipeline_WithoutPipeline_IsNull()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.GetBuildPipeline(), Is.Null);
        }

        [Test]
        public void CanBuild_IsTrue()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanBuild(out var _), Is.True);
        }

        [Test]
        public void CanBuild_WithoutPipeline_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.CanBuild(out var _), Is.False);
        }

        [Test]
        public void Build_Succeeds()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
        }

        [Test]
        public void Build_WithoutPipeline_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);
        }

        [Test]
        public void CanRun_IsTrue()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
            Assert.That(config.CanRun(out var _), Is.True);
        }

        [Test]
        public void CanRun_WithoutBuild_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.CanRun(out var _), Is.False);
        }

        [Test]
        public void CanRun_WithFailedBuild_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);
            Assert.That(config.CanRun(out var _), Is.False);
        }

        [Test]
        public void CanRun_WithoutPipeline_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            config.RemoveComponent<TestPipelineComponent>();
            Assert.That(config.CanRun(out var _), Is.False);
        }

        [Test]
        public void CanRun_WithoutRunStep_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            pipeline.RunStep = null;
            Assert.That(config.CanRun(out var _), Is.False);
        }

        [Test]
        public void CanRun_WhenRunStepCannotRun_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepCannotRun());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);
            Assert.That(config.CanRun(out var _), Is.False);
        }

        [Test]
        public void Run_Succeeds()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.True);
            }
        }

        [Test]
        public void Run_WithoutBuild_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithFailedBuild_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();
            Assert.That(config.Build().Succeeded, Is.False);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithoutPipeline_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            config.RemoveComponent<TestPipelineComponent>();
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WithoutRunStep_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStep());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            pipeline.RunStep = null;
            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunStepCannotRun_IsFalse()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepCannotRun());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunStepFails_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepFailure());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }

        [Test]
        public void Run_WhenRunStepThrows_Fails()
        {
            var pipeline = BuildPipeline.CreateInstance(p => p.RunStep = new TestRunStepThrows());
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestPipelineComponent { Pipeline = pipeline }));
            Assert.That(config.Build().Succeeded, Is.True);

            using (var result = config.Run())
            {
                Assert.That(result.Succeeded, Is.False);
            }
        }
    }
}
