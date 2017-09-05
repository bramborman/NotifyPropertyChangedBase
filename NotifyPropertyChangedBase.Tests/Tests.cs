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
using System.Collections.Generic;

namespace NotifyPropertyChangedBase.Tests
{
    [TestClass]
    public sealed class Tests
    {
        // U = uint
        // L = long
        // UL = ulong
        // F = float
        // M = decimal
        // D = double
        private readonly string[] invalidPropertyNames = new string[] { null, "", "\n\t   \v\r" };
        private readonly object[] invalidInt32Values = new object[] { null, true, 'x', "", (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private readonly object[] invalidNullableInt32Values = new object[] { true, 'x', "", (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private readonly object[] invalidTestValues = new object[] { true, 'x', "", (byte)0, (sbyte)0, 0, 0U, 0L, 0UL, 0F, 0M, 0D };

        [TestMethod]
        public void ConstructorTest()
        {
            Wrapper w = new Wrapper();
            Assert.IsTrue(w.IsPropertyChangedCallbackInvokingEnabled);
            Assert.IsTrue(w.IsPropertyChangedEventInvokingEnabled);
        }

        [TestMethod]
        public void RegisterPropertyTest()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.RegisterProperty(invalidPropertyName, typeof(int), 0));

            // Invalid type argument
            Assert.ThrowsException<ArgumentNullException>(() => w.RegisterProperty("P", null, 0));

            // Invalid default value
            AllThrows<object, ArgumentException>(invalidInt32Values, invalidInt32Value => w.RegisterProperty("P", typeof(int), invalidInt32Value));
            AllThrows<object, ArgumentException>(invalidNullableInt32Values, invalidNullableInt32Value => w.RegisterProperty("P", typeof(int?), invalidNullableInt32Value));
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty("P", typeof(uint), -1));
            AllThrows<object, ArgumentException>(invalidTestValues, invalidTestValue => w.RegisterProperty("P", typeof(ITest), invalidTestValue));
            AllThrows<object, ArgumentException>(invalidTestValues, invalidTestValue => w.RegisterProperty("P", typeof(TestBase), invalidTestValue));
            AllThrows<object, ArgumentException>(invalidTestValues, invalidTestValue => w.RegisterProperty("P", typeof(Test), invalidTestValue));

            const string PROP_INT32 = "Int32";

            // Valid default values
            w.RegisterProperty(PROP_INT32, typeof(int), 0);
            w.RegisterProperty("Nullable1", typeof(int?), null);
            w.RegisterProperty("Nullable2", typeof(int?), 0);
            w.RegisterProperty("ITest", typeof(ITest), null);
            w.RegisterProperty("ITest2", typeof(ITest), new Test());
            w.RegisterProperty("TestBase", typeof(TestBase), null);
            w.RegisterProperty("TestBase2", typeof(TestBase), new Test());
            w.RegisterProperty("Test", typeof(Test), null);
            w.RegisterProperty("Test2", typeof(Test), new Test());

            // Registering another property with the same name
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty(PROP_INT32, typeof(int), 0));
        }

        [TestMethod]
        public void GetSetForceSetValueTest()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.GetValue(invalidPropertyName));
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.SetValue(invalidPropertyName));
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.ForceSetValue(invalidPropertyName));

            // Not registered property
            Assert.ThrowsException<ArgumentException>(() => w.GetValue("1"));
            Assert.ThrowsException<ArgumentException>(() => w.SetValue("1"));
            Assert.ThrowsException<ArgumentException>(() => w.ForceSetValue("1"));

            // Actual Get/Set/ForceSetValue tests
            Test<int>("Int32", typeof(int), value => value + 1, invalidInt32Values);
            Test<int?>("NullableInt32", typeof(int?), value => value == null ? 0 : value.Value + 1, invalidNullableInt32Values);
            Test<Test>("ITest", typeof(ITest), value => new Test(), invalidTestValues);
            Test<Test>("TestBase", typeof(TestBase), value => new Test(), invalidTestValues);
            Test<Test>("Test", typeof(Test), value => new Test(), invalidTestValues);

            void Test<TValue>(string propertyName, Type propertyType, Func<TValue, TValue> getNewValue, IEnumerable<object> invalidValues)
            {
                TValue value = getNewValue(default(TValue));
                w.RegisterProperty(propertyName, propertyType, value);
                Assert.AreEqual(value, w.GetValue(propertyName));

                value = getNewValue(value);
                Assert.AreNotEqual(value, w.GetValue(propertyName));

                w.SetValue(value, propertyName);
                Assert.AreEqual(value, w.GetValue(propertyName));

                w.SetValue(default(TValue), propertyName);
                Assert.AreEqual(default(TValue), w.GetValue(propertyName));

                value = getNewValue(value);
                w.ForceSetValue(value, propertyName);
                Assert.AreEqual(value, w.GetValue(propertyName));
                
                w.ForceSetValue(default(TValue), propertyName);
                Assert.AreEqual(default(TValue), w.GetValue(propertyName));

                // Invalid value
                AllThrows<object, ArgumentException>(invalidValues, invalidValue => w.SetValue(invalidValue, propertyName));
                AllThrows<object, ArgumentException>(invalidValues, invalidValue => w.ForceSetValue(invalidValue, propertyName));
            }
        }

        [TestMethod]
        public void PropertyChangedEventTest()
        {
            const string PROP = "Int32";

            bool propertyChangedCalled = false;
            int value = 0;
            Wrapper w = new Wrapper();

            w.PropertyChanged += (sender, e) =>
            {
                Assert.AreEqual(PROP, e.PropertyName);
                Assert.AreEqual(w, sender);

                propertyChangedCalled = true;
            };

            w.RegisterProperty(PROP, typeof(int), value);

            w.SetValue(value, PROP);
            Assert.IsFalse(propertyChangedCalled);

            w.ForceSetValue(value, PROP);
            Assert.IsTrue(propertyChangedCalled);
            propertyChangedCalled = false;

            value++;
            w.SetValue(value, PROP);
            Assert.IsTrue(propertyChangedCalled);
            propertyChangedCalled = false;

            value++;
            w.ForceSetValue(value, PROP);
            Assert.IsTrue(propertyChangedCalled);
            propertyChangedCalled = false;
        }

        private void AllThrows<TObject, TException>(IEnumerable<TObject> collection, Action<TObject> action) where TException : Exception
        {
            foreach (TObject item in collection)
            {
                Assert.ThrowsException<TException>(() => action(item));
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
