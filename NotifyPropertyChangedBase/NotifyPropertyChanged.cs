// ---------------------------------------------------------------------------------------
// <copyright file="NotifyPropertyChanged.cs" company="Marian Dolinský">
// Copyright (c) Marian Dolinský. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NotifyPropertyChangedBase
{
    /// <summary>
    /// Abstract base class implementing the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyData> _backingStore = new Dictionary<string, PropertyData>();

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PropertyChanged"/> event should be invoked
        /// when a property changes. The default value is <c>true</c>.
        /// </summary>
        protected bool IsPropertyChangedEventInvokingEnabled { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether registered <see cref="PropertyChangedCallbackHandler"/>s should be invoked
        /// when a property changes. The default value is <c>true</c>.
        /// </summary>
        protected bool IsPropertyChangedCallbackInvokingEnabled { get; set; } = true;

        /// <summary>
        /// Implementation of the <see cref="INotifyPropertyChanged.PropertyChanged"/> event. Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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
            ThrowIfStringIsNullOrWhiteSpace(name, nameof(name));
            ThrowIfNull(type, nameof(type));
            ThrowIfNotAssignable(defaultValue, type, name);

            // Let's hope this try-catch performs better than if followed by .Add call (☞ﾟヮﾟ)☞
            try
            {
                _backingStore.Add(name, new PropertyData(defaultValue, type, propertyChangedCallback));
            }
            catch (ArgumentException) when (_backingStore.ContainsKey(name))
            {
                throw new ArgumentException($"This instance already contains a registered property named '{name}'.");
            }
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
        ///         Actual instance does not contain any registered property with the specified name.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyChangedCallback"/> is <c>null</c>.
        ///     </para>
        /// </exception>
        protected void RegisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            ThrowIfNull(propertyChangedCallback, nameof(propertyChangedCallback));
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
        ///         Actual instance does not contain any registered property with the specified name.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyChangedCallback"/> is <c>null</c>.
        ///     </para>
        /// </exception>
        protected void UnregisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            ThrowIfNull(propertyChangedCallback, nameof(propertyChangedCallback));
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
        ///         Actual instance does not contain any registered property with the specified name.
        ///     </para>
        /// </exception>
        protected object GetValue([CallerMemberName]string propertyName = null)
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
        ///         Actual instance does not contain any registered property with the specified name.
        ///     </para>
        /// </exception>
        protected void ForceSetValue(object value, [CallerMemberName]string propertyName = null)
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
        ///         Actual instance does not contain any registered property with the specified name.
        ///     </para>
        /// </exception>
        protected void SetValue(object value, [CallerMemberName]string propertyName = null)
        {
            SetValue(value, propertyName, false);
        }

        private void SetValue(object value, string propertyName, bool forceSetValue)
        {
            PropertyData propertyData = GetPropertyData(propertyName, nameof(propertyName));
            ThrowIfNotAssignable(value, propertyData.Type, propertyName);

            // Calling Equals calls the overriden method even when the value is boxed
            bool? valuesEqual = propertyData.Value?.Equals(value);

            if (forceSetValue || (valuesEqual == null && !(value is null)) || valuesEqual == false)
            {
                object oldValue = propertyData.Value;
                propertyData.Value = value;

                if (IsPropertyChangedCallbackInvokingEnabled)
                {
                    OnPropertyChangedCallback(oldValue, value, propertyName);
                }

                if (IsPropertyChangedEventInvokingEnabled)
                {
                    OnPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Invokes the PropertyChangedCallback for the given property.
        /// </summary>
        /// <param name="oldValue">Previous value of the changed property.</param>
        /// <param name="newValue">Current value of the changed property.</param>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Actual instance does not contain any registered property with the specified name.
        ///     </para>
        /// </exception>
        protected virtual void OnPropertyChangedCallback(object oldValue, object newValue, [CallerMemberName]string propertyName = null)
        {
            GetPropertyData(propertyName, nameof(propertyName)).PropertyChangedCallback?.Invoke(this, new PropertyChangedCallbackArgs(oldValue, newValue));
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c> or white space.</exception>
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            ThrowIfStringIsNullOrWhiteSpace(propertyName, nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private PropertyData GetPropertyData(string propertyName, string propertyNameParameterName)
        {
            ThrowIfStringIsNullOrWhiteSpace(propertyName, propertyNameParameterName);

            try
            {
                return _backingStore[propertyName];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException($"There is no registered property called '{propertyName}' in this instance.", propertyNameParameterName);
            }
        }

        private static void ThrowIfNotAssignable(object value, Type type, string propertyName)
        {
            if (value == null)
            {
                if (type.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(type) == null)
                {
                    throw new ArgumentException("Cannot assign a null value to a property of a non nullable type."
                                                + $"{Environment.NewLine}Property name: {propertyName}"
                                                + $"{Environment.NewLine}Property type: {type}");
                }
            }
            else
            {
                Type valueType = value.GetType();

                if (!type.GetTypeInfo().IsAssignableFrom(valueType.GetTypeInfo()))
                {
                    throw new ArgumentException($"Cannot assign a new value to a property because their types are not compatible."
                                                + $"{Environment.NewLine}Value type: {valueType}"
                                                + $"{Environment.NewLine}Property name: {propertyName}"
                                                + $"{Environment.NewLine}Property type: {type}");
                }
            }
        }

        private static void ThrowIfNull(object obj, string parameterName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ThrowIfStringIsNullOrWhiteSpace(string str, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Value cannot be white space or null.", parameterName);
            }
        }

        private sealed class PropertyData
        {
#pragma warning disable SA1401 // Fields should be private
            internal readonly Type Type;

            internal object Value;
            internal PropertyChangedCallbackHandler PropertyChangedCallback;
#pragma warning restore SA1401 // Fields should be private

            internal PropertyData(object defaultValue, Type type, PropertyChangedCallbackHandler propertyChangedCallback)
            {
                Type = type;

                Value = defaultValue;
                PropertyChangedCallback = propertyChangedCallback;
            }
        }
    }
}
