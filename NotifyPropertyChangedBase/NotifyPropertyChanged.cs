﻿// MIT License
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !NET_40
using System.Runtime.CompilerServices;
#endif

namespace NotifyPropertyChangedBase
{
    /// <summary>
    /// Abstract base class implementing the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyData> backingStore = new Dictionary<string, PropertyData>();

        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="PropertyChanged"/> event should be invoked
        /// when a property changes. The default value is <c>true</c>.
        /// </summary>
        protected bool IsPropertyChangedEventInvokingEnabled { get; set; }
        /// <summary>
        /// Gets or sets the value indicating whether registered <see cref="PropertyChangedCallbackHandler"/>s should be invoked
        /// when a property changes. The default value is <c>true</c>.
        /// </summary>
        protected bool IsPropertyChangedCallbackInvokingEnabled { get; set; }

        /// <summary>
        /// Implementation of the <see cref="INotifyPropertyChanged.PropertyChanged"/> event. Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChanged"/> class.
        /// </summary>
        protected NotifyPropertyChanged()
        {
            IsPropertyChangedCallbackInvokingEnabled = true;
            IsPropertyChangedEventInvokingEnabled = true;
        }

        /// <summary>
        /// Registers a new property in the actual instance of <see cref="NotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="name">Name of the registered property.</param>
        /// <param name="type">Type of the registered property.</param>
        /// <param name="defaultValue">Default value of the registered property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="name"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Value of <paramref name="defaultValue"/> cannot be assigned to a property of type specified in the <paramref name="type"/> parameter.
        ///     </para>
        ///     <para>
        ///         Instance already contains a registered property named the same as specified in parameter <paramref name="name"/>.
        ///     </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="type"/> is <c>null</c>.</exception>
        protected void RegisterProperty(string name, Type type, object defaultValue)
        {
            RegisterProperty(name, type, defaultValue, null);
        }

        /// <summary>
        /// Registers a new property in the actual instance of <see cref="NotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="name">Name of the registered property.</param>
        /// <param name="type">Type of the registered property.</param>
        /// <param name="defaultValue">Default value of the registered property.</param>
        /// <param name="propertyChangedCallback">Callback invoked right after the registered property changes.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="name"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Value of <paramref name="defaultValue"/> cannot be assigned to a property of type specified in the <paramref name="type"/> parameter.
        ///     </para>
        ///     <para>
        ///         Instance already contains a registered property named the same as specified in parameter <paramref name="name"/>.
        ///     </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="type"/> is <c>null</c>.</exception>
        protected void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            Helpers.ValidateStringNotNullOrWhiteSpace(name, nameof(name));
            Helpers.ValidateObjectNotNull(type, nameof(type));
            ValidateValueForType(defaultValue, type);

            if (backingStore.ContainsKey(name))
            {
                throw new ArgumentException($"This class already contains registered property named '{name}'.");
            }

            backingStore.Add(name, new PropertyData(defaultValue, type, propertyChangedCallback));
        }

        /// <summary>
        /// Registers the <paramref name="propertyChangedCallback"/> to be invoked when the property with the specified name changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyChangedCallback"><see cref="PropertyChangedCallbackHandler"/> to be registered.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Actual instance does not contain registered property with the specified name.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyChangedCallback"/> is <c>null</c>.
        ///     </para>
        /// </exception>
        protected void RegisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            Helpers.ValidateObjectNotNull(propertyChangedCallback, nameof(propertyChangedCallback));
            GetPropertyData(propertyName, nameof(propertyName)).PropertyChangedCallback += propertyChangedCallback;
        }

        /// <summary>
        /// Unregisters the <paramref name="propertyChangedCallback"/> so it will NOT be invoked when the property with the specified name changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyChangedCallback"><see cref="PropertyChangedCallbackHandler"/> to be unregistered.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Actual instance does not contain registered property with the specified name.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyChangedCallback"/> is <c>null</c>.
        ///     </para>
        /// </exception>
        protected void UnregisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            Helpers.ValidateObjectNotNull(propertyChangedCallback, nameof(propertyChangedCallback));
            GetPropertyData(propertyName, nameof(propertyName)).PropertyChangedCallback -= propertyChangedCallback;
        }

        /// <summary>
        /// Returns the current value of a registered property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Value of the property.</returns>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Actual instance does not contain registered property with the specified name.
        ///     </para>
        /// </exception>
#if NET_40
        protected object GetValue(string propertyName)
#else
        protected object GetValue([CallerMemberName]string propertyName = null)
#endif
        {
            return GetPropertyData(propertyName, nameof(propertyName)).Value;
        }

        /// <summary>
        /// Sets new value to a registered property even if it is equal to its current value and invokes the <see cref="PropertyChangedCallbackHandler"/>
        /// for the property if specified before and if the value of <see cref="IsPropertyChangedCallbackInvokingEnabled"/> is <c>true</c>
        /// and also invokes the <see cref="PropertyChanged"/> event if value of <see cref="IsPropertyChangedEventInvokingEnabled"/> is <c>true</c>.
        /// </summary>
        /// <param name="value">New value for the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Value cannot be assigned to the property with the specified name because of its type.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Actual instance does not contain registered property with the specified name.
        ///     </para>
        /// </exception>
#if NET_40
        protected void ForceSetValue(object value, string propertyName)
#else
        protected void ForceSetValue(object value, [CallerMemberName]string propertyName = null)
#endif
        {
            SetValue(value, propertyName, true);
        }

        /// <summary>
        /// Sets a new value to a registered property if it's not equal to its current value and invokes the <see cref="PropertyChangedCallbackHandler"/>
        /// for the property if specified before and if the value of <see cref="IsPropertyChangedCallbackInvokingEnabled"/> is <c>true</c>
        /// and also invokes the <see cref="PropertyChanged"/> event if value of <see cref="IsPropertyChangedEventInvokingEnabled"/> is <c>true</c>.
        /// </summary>
        /// <param name="value">New value for the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Value cannot be assigned to the property with the specified name because of its type.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Actual instance does not contain registered property with the specified name.
        ///     </para>
        /// </exception>
#if NET_40
        protected void SetValue(object value, string propertyName)
#else
        protected void SetValue(object value, [CallerMemberName]string propertyName = null)
#endif
        {
            SetValue(value, propertyName, false);
        }

        private void SetValue(object value, string propertyName, bool forceSetValue)
        {
            PropertyData propertyData = GetPropertyData(propertyName, nameof(propertyName));
            ValidateValueForType(value, propertyData.Type);
            
            // Calling Equals calls the overriden method even when the value is boxed
            bool? valuesEqual = propertyData.Value?.Equals(value);

            if (forceSetValue || (valuesEqual == null && !ReferenceEquals(value, null)) || valuesEqual == false)
            {
                object oldValue = propertyData.Value;
                propertyData.Value = value;

                if (IsPropertyChangedCallbackInvokingEnabled)
                {
                    propertyData.InvokePropertyChangedCallback(this, new PropertyChangedCallbackArgs(oldValue, value));
                }

                if (IsPropertyChangedEventInvokingEnabled)
                {
                    OnPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c> or white space.</exception>
#if NET_40
        protected virtual void OnPropertyChanged(string propertyName)
#else
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
#endif
        {
            Helpers.ValidateStringNotNullOrWhiteSpace(propertyName, nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void ValidateValueForType(object value, Type type)
        {
            if (value == null)
            {
                if (type.GetIsValueType() && Nullable.GetUnderlyingType(type) == null)
                {
                    throw new ArgumentException($"The type '{type}' is not a nullable type.");
                }
            }
            else
            {
                if (!type.GetIsAssignableFrom(value.GetType()))
                {
                    throw new ArgumentException($"The specified value cannot be assigned to a property of type ({type})");
                }
            }
        }

        private PropertyData GetPropertyData(string propertyName, string propertyNameParameterName)
        {
            Helpers.ValidateStringNotNullOrWhiteSpace(propertyName, propertyNameParameterName);
            
            try
            {
                return backingStore[propertyName];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"There is no registered property called '{propertyName}'.", propertyNameParameterName);
            }
        }

        private class PropertyData
        {
            internal object Value { get; set; }
            internal Type Type { get; }

            internal event PropertyChangedCallbackHandler PropertyChangedCallback;

            internal PropertyData(object defaultValue, Type type, PropertyChangedCallbackHandler propertyChangedCallback)
            {
                Value = defaultValue;
                Type = type;

                PropertyChangedCallback += propertyChangedCallback;
            }

            internal void InvokePropertyChangedCallback(NotifyPropertyChanged sender, PropertyChangedCallbackArgs e)
            {
                PropertyChangedCallback?.Invoke(sender, e);
            }
        }
    }

    /// <summary>
    /// Represents the callback that is invoked when a property registered in the <see cref="NotifyPropertyChanged"/> class changes.
    /// </summary>
    /// <param name="sender">Object that invoked this callback.</param>
    /// <param name="e">Callback data containing info about the changed property.</param>
    public delegate void PropertyChangedCallbackHandler(NotifyPropertyChanged sender, PropertyChangedCallbackArgs e);

    /// <summary>
    /// Callback data containing info about the changed property in the <see cref="PropertyChangedCallbackHandler"/>.
    /// </summary>
    public sealed class PropertyChangedCallbackArgs
    {
        /// <summary>
        /// Gets or sets a value that marks the callback as handled.
        /// </summary>
        public bool Handled { get; set; }
        /// <summary>
        /// Gets the previous value of the changed property.
        /// </summary>
        public object OldValue { get; }
        /// <summary>
        /// Gets the current value of the changed property.
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedCallbackArgs"/> class.
        /// </summary>
        /// <param name="oldValue">Previous value of the changed property.</param>
        /// <param name="newValue">Current value of the changed property.</param>
        public PropertyChangedCallbackArgs(object oldValue, object newValue)
        {
            Handled = false;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
