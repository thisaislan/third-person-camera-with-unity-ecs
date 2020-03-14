using NUnit.Framework;

namespace Unity.Build.Tests
{
    class BuildContextTests : BuildTestsBase
    {
        class TestA { }
        class TestB { }

        [Test]
        public void HasValue()
        {
            var context = new BuildContext();
            context.SetValue(new TestA());
            Assert.That(context.HasValue<TestA>(), Is.True);
            Assert.That(context.HasValue<TestB>(), Is.False);
        }

        [Test]
        public void GetValue()
        {
            var context = new BuildContext();
            var value = new TestA();
            context.SetValue(value);
            Assert.That(context.GetValue<TestA>(), Is.EqualTo(value));
        }

        [Test]
        public void GetValue_WhenValueDoesNotExist_IsNull()
        {
            var context = new BuildContext();
            Assert.That(context.GetValue<TestA>(), Is.Null);
        }

        [Test]
        public void GetOrCreateValue()
        {
            var context = new BuildContext();
            Assert.That(context.GetOrCreateValue<TestA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestA>(), Is.True);
            Assert.That(context.GetValue<TestA>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateValue_WhenValueExist_DoesNotThrow()
        {
            var context = new BuildContext();
            context.SetValue(new TestA());
            Assert.DoesNotThrow(() => context.GetOrCreateValue<TestA>());
        }

        [Test]
        public void SetValue()
        {
            var context = new BuildContext();
            context.SetValue(new TestA());
            Assert.That(context.HasValue<TestA>(), Is.True);
            Assert.That(context.GetValue<TestA>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(1));
        }

        [Test]
        public void SetValue_SkipObjectType()
        {
            var context = new BuildContext();
            Assert.DoesNotThrow(() => context.SetValue(new object()));
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void SetValue_SkipNullValues()
        {
            var context = new BuildContext();
            Assert.DoesNotThrow(() => context.SetValue<object>(null));
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void SetValue_WhenValueExist_OverrideValue()
        {
            var context = new BuildContext();
            var instance1 = new TestA();
            var instance2 = new TestA();

            context.SetValue(instance1);
            Assert.That(context.Values, Is.EqualTo(new[] { instance1 }));

            context.SetValue(instance2);
            Assert.That(context.Values, Is.EqualTo(new[] { instance2 }));
        }

        [Test]
        public void RemoveValue()
        {
            var context = new BuildContext();
            context.SetValue(new TestA());
            Assert.That(context.Values.Length, Is.EqualTo(1));
            Assert.That(context.RemoveValue<TestA>(), Is.True);
            Assert.That(context.Values.Length, Is.Zero);
        }
    }
}
