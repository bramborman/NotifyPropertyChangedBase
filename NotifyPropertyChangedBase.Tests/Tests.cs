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
using System.ComponentModel;

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
        private static readonly string[] invalidPropertyNames = new string[] { null, "", "\n\t   \v\r" };
        private static readonly object[] invalidInt32Values = new object[] { null, true, 'x', "", (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private static readonly object[] invalidNullableInt32Values = new object[] { true, 'x', "", (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private static readonly object[] invalidUInt32Values = new object[] { null, true, 'x', "", (byte)0, (sbyte)0, 0, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private static readonly object[] invalidTestValues = new object[] { true, 'x', "", (byte)0, (sbyte)0, 0, 0U, 0L, 0UL, 0F, 0M, 0D };
        private static readonly List<TypeData> typeDataCollection = new List<TypeData>()
        {
            new TypeData(typeof(int), new Tuple<object, object>(0, 0), value => (int)value + 1, invalidInt32Values),
            new TypeData(typeof(int?), new Tuple<object, object>(null, 0), value => value == null ? 0 : ((int?)value).Value + 1,  invalidNullableInt32Values),
            new TypeData(typeof(uint), new Tuple<object, object>(0U, 0U), value => (uint)value + 1, invalidUInt32Values),
            new TypeData(typeof(ITest), new Tuple<object, object>(null, new Test()), value => new Test(), invalidTestValues),
            new TypeData(typeof(TestBase), new Tuple<object, object>(null, new Test()), value => new Test(), invalidTestValues),
            new TypeData(typeof(Test), new Tuple<object, object>(null, new Test()), value => new Test(), invalidTestValues)
        };
        
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
            Assert.ThrowsException<ArgumentNullException>(() => w.RegisterProperty("InvalidTypeArgument", null, 0));

            foreach (TypeData typeData in typeDataCollection)
            {
                // Valid arguments
                string propertyName1 = typeData.Type.Name + '1';
                w.RegisterProperty(propertyName1, typeData.Type, typeData.DefaultValues.Item1);
                Assert.AreEqual(typeData.DefaultValues.Item1, w.GetValue(propertyName1));

                string propertyName2 = typeData.Type.Name + '2';
                w.RegisterProperty(propertyName2, typeData.Type, typeData.DefaultValues.Item2);
                Assert.AreEqual(typeData.DefaultValues.Item2, w.GetValue(propertyName2));

                // Invalid default values
                AllThrows<object, ArgumentException>(typeData.InvalidValues, invalidValue =>
                {
                    w.RegisterProperty(typeData.Type.Name + "_InvalidDefaultValue", typeData.Type, invalidValue);
                });
            }

            // Registering another property with a name of already registered property
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty(typeDataCollection[0].Type.Name + '1', typeof(int), 0));
        }

        [TestMethod]
        public void GetSetForceSetValueTest()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.GetValue(invalidPropertyName));
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.SetValue(null, invalidPropertyName));
            AllThrows<string, ArgumentException>(invalidPropertyNames, invalidPropertyName => w.ForceSetValue(null, invalidPropertyName));

            // Not registered property
            Assert.ThrowsException<ArgumentException>(() => w.GetValue("NotRegisteredProperty"));
            Assert.ThrowsException<ArgumentException>(() => w.SetValue(null, "NotRegisteredProperty"));
            Assert.ThrowsException<ArgumentException>(() => w.ForceSetValue(null, "NotRegisteredProperty"));
            
            bool propertyChangedEventInvoked = false;
            string propertyName = null;

            w.PropertyChanged += (sender, e) =>
            {
                Assert.AreEqual(propertyName, e.PropertyName);
                Assert.AreEqual(w, sender);

                propertyChangedEventInvoked = true;
            };

            foreach (TypeData typeData in typeDataCollection)
            {
                propertyName = typeData.Type.Name;
                Test(true);
                propertyName = typeData.Type.Name + "_NoPropertyChangedEvent";
                Test(false);

                // Invalid value
                AllThrows<object, ArgumentException>(typeData.InvalidValues, invalidValue => w.SetValue(invalidValue, propertyName));
                AllThrows<object, ArgumentException>(typeData.InvalidValues, invalidValue => w.ForceSetValue(invalidValue, propertyName));
                
                void Test(bool isPropertyChangedEventInvokingEnabled)
                {
                    w.IsPropertyChangedEventInvokingEnabled = isPropertyChangedEventInvokingEnabled;
                    RunTest(typeData.DefaultValues.Item1, '1');
                    RunTest(typeData.DefaultValues.Item2, '2');

                    void RunTest(object defaultValue, char propertyNameSuffix)
                    {
                        propertyName += propertyNameSuffix;

                        object value = defaultValue;
                        w.RegisterProperty(propertyName, typeData.Type, value);
                        Assert.AreEqual(value, w.GetValue(propertyName));
                        CheckPropertyChangedInvoked(false);

                        value = typeData.GetNewValue(value);
                        Assert.AreNotEqual(value, w.GetValue(propertyName));
                        CheckPropertyChangedInvoked(false);

                        w.SetValue(value, propertyName);
                        Assert.AreEqual(value, w.GetValue(propertyName));
                        CheckPropertyChangedInvoked(isPropertyChangedEventInvokingEnabled);

                        w.SetValue(defaultValue, propertyName);
                        Assert.AreEqual(defaultValue, w.GetValue(propertyName));
                        CheckPropertyChangedInvoked(isPropertyChangedEventInvokingEnabled);

                        value = typeData.GetNewValue(value);
                        w.ForceSetValue(value, propertyName);
                        Assert.AreEqual(value, w.GetValue(propertyName));
                        CheckPropertyChangedInvoked(isPropertyChangedEventInvokingEnabled);

                        w.ForceSetValue(defaultValue, propertyName);
                        Assert.AreEqual(defaultValue, w.GetValue(propertyName));
                        CheckPropertyChangedInvoked(isPropertyChangedEventInvokingEnabled);

                        void CheckPropertyChangedInvoked(bool expectedValue)
                        {
                            Assert.AreEqual(expectedValue, propertyChangedEventInvoked);
                            propertyChangedEventInvoked = false;
                        }
                    }
                }
            }
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

        private sealed class TypeData
        {
            public Type Type { get; }
            public Tuple<object, object> DefaultValues { get; }
            public Func<object, object> GetNewValue { get; }
            public object[] InvalidValues { get; }

            public TypeData(Type type, Tuple<object, object> defaultValues, Func<object, object> getNewValue, object[] invalidValues)
            {
                Type = type;
                DefaultValues = defaultValues;
                GetNewValue = getNewValue;
                InvalidValues = invalidValues;
            }
        }
    }
}
