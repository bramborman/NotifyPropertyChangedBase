#region License
// MIT License
//
// Copyright (c) 2017 Marian Dolinský
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NotifyPropertyChangedBase.Tests
{
    [TestClass]
    public sealed class Tests
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

        [TestMethod]
        public void GetValueTests()
        {

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
