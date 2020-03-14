using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Build.Tests
{
    class BuildStepResultTests : BuildTestsBase
    {
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
            Assert.That(result.BuildStepsResults.Select(r => (bool)r), Is.EqualTo(new[] { true, true, true, true, true, true }));
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
            Assert.That(result.BuildStepsResults.Select(r => (bool)r), Is.EqualTo(new[] { true, false, false, true }));
        }

        [Test]
        public void LogResult_SupportFormattingCharacters()
        {
            var config = BuildConfiguration.CreateInstance();
            var step = new TestRunStep();

            var resultFailure = RunStepResult.Failure(config, step, @"{}{{}}{0}{s}%s%%\s±@£¢¤¬¦²³¼½¾");
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, new Regex(@"Run.* failed\.\n.+"));
                resultFailure.LogResult();
            });
        }
    }
}
