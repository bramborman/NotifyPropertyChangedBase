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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NotifyPropertyChangedBase.Tests
{
    [TestClass]
    public sealed class Tests
    {
        //U = uint
        //L = long
        //UL = ulong
        //F = float
        //M = decimal
        //D = double

        private readonly string[] invalidPropertyNames = new string[] { null, "", "   ", "\n\t\v\r" };
        private readonly object[] invalidInt32Values = new object[] { null, true, 'x', (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        
        [TestMethod]
        public void RegisterPropertyTests()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            TestInvalidPropertyNames(invalidPropertyName => w.RegisterProperty(invalidPropertyName, typeof(int), 0));

            // Invalid type argument
            Assert.ThrowsException<ArgumentNullException>(() => w.RegisterProperty("P", null, 0));

            // Invalid default value
            TestInvalidInt32Values(invalidInt32Value => w.RegisterProperty("P", typeof(int), invalidInt32Value));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(uint), -1));

            // Valid arguments
            const string PROP_1 = "1";
            w.RegisterProperty(PROP_1, typeof(int), 0);

            // Registering another property with the same name
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty(PROP_1, typeof(int), 0));

            // Valid default values
            w.RegisterProperty("ITest", typeof(ITest), null);
            w.RegisterProperty("ITest2", typeof(ITest), new Test());
            w.RegisterProperty("TestBase", typeof(TestBase), null);
            w.RegisterProperty("TestBase2", typeof(TestBase), new Test());
            w.RegisterProperty("Test", typeof(Test), null);
            w.RegisterProperty("Test2", typeof(Test), new Test());
        }

        [TestMethod]
        public void GetSetForceSetValueTests()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            TestInvalidPropertyNames(invalidPropertyName => w.GetValue(invalidPropertyName));

            {
                const string PROP_1 = "1";

                int prop1Value = 0;
                w.RegisterProperty(PROP_1, typeof(int), prop1Value);
                Assert.AreEqual(prop1Value, w.GetValue(PROP_1));

                prop1Value++;
                Assert.AreNotEqual(prop1Value, w.GetValue(PROP_1));

                w.SetValue(prop1Value, PROP_1);
                Assert.AreEqual(prop1Value, w.GetValue(PROP_1));

                prop1Value++;
                w.ForceSetValue(prop1Value, PROP_1);
                Assert.AreEqual(prop1Value, w.GetValue(PROP_1));

                // Invalid value
                TestInvalidInt32Values(invalidInt32Value => w.SetValue(invalidInt32Value, PROP_1));
                TestInvalidInt32Values(invalidInt32Value => w.ForceSetValue(invalidInt32Value, PROP_1));
            }

            {
                const string PROP_2 = "2";

                Test prop2Value = new Test();
                w.RegisterProperty(PROP_2, typeof(Test), prop2Value);
                Assert.AreEqual(prop2Value, w.GetValue(PROP_2));

                prop2Value = new Test();
                Assert.AreNotEqual(prop2Value, w.GetValue(PROP_2));

                w.SetValue(prop2Value, PROP_2);
                Assert.AreEqual(prop2Value, w.GetValue(PROP_2));

                prop2Value = new Test();
                w.ForceSetValue(prop2Value, PROP_2);
                Assert.AreEqual(prop2Value, w.GetValue(PROP_2));
            }
        }

        private void TestInvalidPropertyNames(Action<string> action)
        {
            foreach (string invalidPropertyName in invalidPropertyNames)
            {
                Assert.ThrowsException<ArgumentException>(() => action(invalidPropertyName));
            }
        }

        private void TestInvalidInt32Values(Action<object> action)
        {
            foreach (object invalidInt32Value in invalidInt32Values)
            {
                Assert.ThrowsException<ArgumentException>(() => action(invalidInt32Value));
            }
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
