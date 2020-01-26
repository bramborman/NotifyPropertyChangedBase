// ---------------------------------------------------------------------------------------
// <copyright file="Tests.cs" company="Marian Dolinský">
// Copyright (c) Marian Dolinský. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        private static readonly string[] InvalidPropertyNames = new string[] { null, string.Empty, "\n\t   \v\r" };
        private static readonly object[] InvalidInt32Values = new object[] { null, true, 'x', string.Empty, (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private static readonly object[] InvalidNullableInt32Values = new object[] { true, 'x', string.Empty, (byte)0, (sbyte)0, 0U, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private static readonly object[] InvalidUInt32Values = new object[] { null, true, 'x', string.Empty, (byte)0, (sbyte)0, 0, 0L, 0UL, 0F, 0M, 0D, new Test() };
        private static readonly object[] InvalidTestValues = new object[] { true, 'x', string.Empty, (byte)0, (sbyte)0, 0, 0U, 0L, 0UL, 0F, 0M, 0D };
        private static readonly TypeData[] TypeDataCollection =
        {
            new TypeData(typeof(int), new object[] { 0 }, value => (int)value + 1, InvalidInt32Values),
            new TypeData(typeof(int?), new object[] { null, 0 }, value => value == null ? 0 : ((int?)value).Value + 1,  InvalidNullableInt32Values),
            new TypeData(typeof(uint), new object[] { 0U }, value => (uint)value + 1, InvalidUInt32Values),
            new TypeData(typeof(ITest), new object[] { null, new Test() }, value => new Test(), InvalidTestValues),
            new TypeData(typeof(TestBase), new object[] { null, new Test() }, value => new Test(), InvalidTestValues),
            new TypeData(typeof(Test), new object[] { null, new Test() }, value => new Test(), InvalidTestValues)
        };

        [TestMethod]
        public void ConstructorTest()
        {
            Wrapper w = new Wrapper();
            Assert.IsTrue(w.IsPropertyChangedEventInvokingEnabled);
            Assert.IsTrue(w.IsPropertyChangedCallbackInvokingEnabled);
        }

        [TestMethod]
        public void RegisterPropertyTest()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            AllThrows<string, ArgumentException>(InvalidPropertyNames, invalidPropertyName => w.RegisterProperty(invalidPropertyName, typeof(int), 0));

            // Invalid type argument
            Assert.ThrowsException<ArgumentNullException>(() => w.RegisterProperty("InvalidTypeArgument", null, 0));

            foreach (TypeData typeData in TypeDataCollection)
            {
                // Valid arguments
                for (int i = 0; i < typeData.DefaultValues.Length; i++)
                {
                    string propertyName = typeData.Type.Name + i;
                    object defaultValue = typeData.DefaultValues[i];

                    w.RegisterProperty(propertyName, typeData.Type, defaultValue);
                    Assert.AreEqual(defaultValue, w.GetValue(propertyName));
                }

                // Invalid default values
                AllThrows<object, ArgumentException>(typeData.InvalidValues, invalidValue =>
                {
                    w.RegisterProperty(typeData.Type.Name + "_InvalidDefaultValue", typeData.Type, invalidValue);
                });
            }

            // Registering another property with a name of already registered property
            Assert.ThrowsException<ArgumentException>(() => w.RegisterProperty(TypeDataCollection[0].Type.Name + '0', typeof(int), 0));
        }

        [TestMethod]
        public void WorkingWithPropertiesTest()
        {
            Wrapper w = new Wrapper();

            // Invalid property name
            AllThrows<string, ArgumentException>(InvalidPropertyNames, invalidPropertyName => w.GetValue(invalidPropertyName));
            AllThrows<string, ArgumentException>(InvalidPropertyNames, invalidPropertyName => w.SetValue(null, invalidPropertyName));
            AllThrows<string, ArgumentException>(InvalidPropertyNames, invalidPropertyName => w.ForceSetValue(null, invalidPropertyName));

            // Not registered property
            Assert.ThrowsException<ArgumentException>(() => w.GetValue("NotRegisteredProperty"));
            Assert.ThrowsException<ArgumentException>(() => w.SetValue(null, "NotRegisteredProperty"));
            Assert.ThrowsException<ArgumentException>(() => w.ForceSetValue(null, "NotRegisteredProperty"));

            int propertyChangedEventInvokeCount = 0;
            int propertyChangedCallbackInvokeCount = 0;
            bool propertyChangedCallbackRegistered = true;
            string propertyName = null;
            object oldValue = null;
            object value = null;

            w.PropertyChanged += (sender, e) =>
            {
                Assert.AreEqual(w, sender);
                Assert.AreEqual(propertyName, e.PropertyName);

                propertyChangedEventInvokeCount++;
            };

            void PropertyChangedCallback(NotifyPropertyChanged sender, PropertyChangedCallbackArgs e)
            {
                Assert.AreEqual(w, sender);
                Assert.IsFalse(e.Handled);
                Assert.AreEqual(oldValue, e.OldValue);
                Assert.AreEqual(value, e.NewValue);

                propertyChangedCallbackInvokeCount++;
            }

            foreach (TypeData typeData in TypeDataCollection)
            {
                for (int i = 0; i < typeData.DefaultValues.Length; i++)
                {
                    object defaultValue = typeData.DefaultValues[i];

                    propertyName = typeData.Type.Name + i;
                    Test(false, true, defaultValue);
                    propertyName += "_Forced";
                    Test(true, true, defaultValue);

                    propertyName = typeData.Type.Name + i + "_NoPropertyChangedEvent";
                    Test(false, false, defaultValue);
                    propertyName += "_Forced";
                    Test(true, false, defaultValue);
                }

                // Invalid value
                AllThrows<object, ArgumentException>(typeData.InvalidValues, invalidValue => w.SetValue(invalidValue, propertyName));
                AllThrows<object, ArgumentException>(typeData.InvalidValues, invalidValue => w.ForceSetValue(invalidValue, propertyName));

                void Test(bool force, bool eventsEnabled, object defaultValue)
                {
                    w.IsPropertyChangedEventInvokingEnabled = eventsEnabled;
                    w.IsPropertyChangedCallbackInvokingEnabled = eventsEnabled;
                    propertyChangedCallbackRegistered = true;

                    oldValue = null;
                    value = defaultValue;

                    // Default value
                    w.RegisterProperty(propertyName, typeData.Type, value, PropertyChangedCallback);
                    Assert.AreEqual(value, w.GetValue(propertyName));
                    CheckEventsInvoked(false);

                    CheckNoChangeSet();

                    propertyChangedCallbackRegistered = false;
                    w.UnregisterPropertyChangedCallback(propertyName, PropertyChangedCallback);

                    // Changing value but not assigning it to property
                    SetValue(typeData.GetNewValue(value));
                    // Probably not needed but I like this ( ͡° ͜ʖ ͡°)
                    Assert.AreNotEqual(value, w.GetValue(propertyName));
                    CheckEventsInvoked(false);

                    // Assigning changed value
                    SetAndTest(eventsEnabled);

                    CheckNoChangeSet();

                    w.RegisterPropertyChangedCallback(propertyName, PropertyChangedCallback);
                    propertyChangedCallbackRegistered = true;

                    // Assigning default value e.g. null etc.
                    SetValue(defaultValue);
                    SetAndTest(eventsEnabled);

                    CheckNoChangeSet();

                    void SetValue(object newValue)
                    {
                        oldValue = value;
                        value = newValue;
                    }

                    void SetAndTest(bool shouldInvokeEvents)
                    {
                        if (force)
                        {
                            w.ForceSetValue(value, propertyName);
                        }
                        else
                        {
                            w.SetValue(value, propertyName);
                        }

                        Assert.AreEqual(value, w.GetValue(propertyName));
                        CheckEventsInvoked(shouldInvokeEvents);
                    }

                    void CheckNoChangeSet()
                    {
                        if (force)
                        {
                            // Has to be set to the same
                            // When force == true, it will call callback
                            oldValue = value;
                        }

                        // No change in value - invoking events only when forced
                        SetAndTest(force && eventsEnabled);
                    }

                    void CheckEventsInvoked(bool shouldInvokeEvents)
                    {
                        Assert.AreEqual(shouldInvokeEvents ? 1 : 0, propertyChangedEventInvokeCount);
                        propertyChangedEventInvokeCount = 0;

                        Assert.AreEqual(shouldInvokeEvents && propertyChangedCallbackRegistered ? 1 : 0, propertyChangedCallbackInvokeCount);
                        propertyChangedCallbackInvokeCount = 0;
                    }
                }
            }
        }

        [TestMethod]
        public void EventsTest()
        {
            Wrapper w = new Wrapper();

            AllThrows<string, ArgumentException>(InvalidPropertyNames, invalidPropertyName => w.OnPropertyChanged(invalidPropertyName));
            AllThrows<string, ArgumentException>(InvalidPropertyNames, invalidPropertyName => w.OnPropertyChangedCallback(0, 1, invalidPropertyName));

            w.OnPropertyChanged("NotRegisteredProperty");
            Assert.ThrowsException<ArgumentException>(() => w.OnPropertyChangedCallback(0, 1, "NotRegisteredProperty"));

            w.RegisterProperty("Property", typeof(bool), false);
            w.OnPropertyChanged("Property");
            w.OnPropertyChangedCallback(false, true, "Property");
        }

        private void AllThrows<TObject, TException>(IEnumerable<TObject> collection, Action<TObject> action)
            where TException : Exception
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
            public object[] DefaultValues { get; }
            public Func<object, object> GetNewValue { get; }
            public object[] InvalidValues { get; }

            public TypeData(Type type, object[] defaultValues, Func<object, object> getNewValue, object[] invalidValues)
            {
                Type = type;
                DefaultValues = defaultValues;
                GetNewValue = getNewValue;
                InvalidValues = invalidValues;
            }
        }
    }
}
