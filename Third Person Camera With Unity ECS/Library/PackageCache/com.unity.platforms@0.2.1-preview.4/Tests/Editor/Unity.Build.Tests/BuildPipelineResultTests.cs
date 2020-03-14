using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Build.Tests
{
    class BuildPipelineResultTests : BuildTestsBase
    {
        [Test]
        public void TryGetBuildStepResult()
        {
            var pipeline = BuildPipeline.CreateInstance((p) =>
            {
                p.BuildSteps.Add(new TestBuildStep1());
                p.BuildSteps.Add(new TestBuildStep2());
                p.BuildSteps.Add(new TestBuildStep3());
            });
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestRequiredComponentA()));
            var pipelineResult = pipeline.Build(config, mutator: (context) =>
            {
                context.SetValue(new TestBuildStep1.Data { Value = nameof(TestBuildStep1) });
                // Here we make TestStep2 fails by not providing its data
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });
            Assert.That(pipelineResult.Succeeded, Is.False);
            Assert.That(pipelineResult.TryGetBuildStepResult(pipeline.BuildSteps[0], out var stepResult), Is.True);
            Assert.That(stepResult.Succeeded, Is.True);
            Assert.That(pipelineResult.TryGetBuildStepResult(pipeline.BuildSteps[1], out stepResult), Is.True);
            Assert.That(stepResult.Succeeded, Is.False);
            Assert.That(pipelineResult.TryGetBuildStepResult(pipeline.BuildSteps[2], out var _), Is.False);
        }

        [Test]
        public void OperatorBool_WhenBuildSucceeds_IsTrue()
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
            Assert.That((bool)result, Is.True);
        }

        [Test]
        public void OperatorBool_WhenBuildFails_IsFalse()
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
                // Here we make TestStep2 fails by not providing its data
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });
            Assert.That(result.Succeeded, Is.False);
            Assert.That((bool)result, Is.False);
        }

        [Test]
        public void LogResult_WhenBuildSucceeds()
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

            LogAssert.Expect(LogType.Log, new Regex("Build succeeded after .*\\."));
            result.LogResult();
        }

        [Test]
        public void LogResult_WhenBuildFails()
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
                // Here we make TestStep2 fails by not providing its data
                context.SetValue(new TestBuildStep3.Data { Value = nameof(TestBuildStep3) });
            });
            Assert.That(result.Succeeded, Is.False);

            LogAssert.Expect(LogType.Error, new Regex("Build failed in .* after .*\\..*"));
            result.LogResult();
        }

        [Test]
        public void LogResult_SupportFormattingCharacters()
        {
            var pipeline = BuildPipeline.CreateInstance();
            var config = BuildConfiguration.CreateInstance();

            var resultSuccess = BuildPipelineResult.Success(pipeline, config);
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Log, new Regex(@"Build succeeded after .+\."));
                resultSuccess.LogResult();
            });

            var resultFailure = BuildPipelineResult.Failure(pipeline, config, @"{}{{}}{0}{s}%s%%\s±@£¢¤¬¦²³¼½¾");
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, new Regex(@"Build failed after .+\.\n.+"));
                resultFailure.LogResult();
            });
        }
    }
}
