using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NotifyPropertyChangedBase.Tests
{
    [TestClass]
    public sealed class NotifyPropertyChangedTests
    {
        [TestMethod]
        public void RegisterPropertyTests()
        {
            Wrapper w = new Wrapper();

            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty(null, typeof(int), 0));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("", typeof(int), 0));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("   ", typeof(int), 0));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("\n\t\v\r", typeof(int), 0));

            Assert.ThrowsException<ArgumentNullException>(() => w.RegisterProperty("P", null, 0));

            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(int), null));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(int), true));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(int), long.MaxValue));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(int), 'x'));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(int), ""));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(int), new Test()));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(uint), -1));

            const string PROP_1 = "1";
            w.RegisterProperty(PROP_1, typeof(int), 0);

            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty(PROP_1, typeof(int), 0));

            w.RegisterProperty("ITest", typeof(ITest), null);
            w.RegisterProperty("ITest2", typeof(ITest), new Test());
            w.RegisterProperty("TestBase", typeof(TestBase), null);
            w.RegisterProperty("TestBase2", typeof(TestBase), new Test());
            w.RegisterProperty("Test", typeof(Test), null);
            w.RegisterProperty("Test2", typeof(Test), new Test());
        }

        private interface ITest
        {
            int Property { get; set; }
        }

        private abstract class TestBase : ITest
        {
            public int Property { get; set; }
        }

        private sealed class Test : TestBase
        {

        }
    }
}
