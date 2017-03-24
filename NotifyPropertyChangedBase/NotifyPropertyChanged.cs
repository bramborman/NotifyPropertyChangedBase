using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !NET_40
using System.Runtime.CompilerServices;
#endif

namespace NotifyPropertyChangedBase
{
    /// <summary>
    /// Abstract class implementing the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    // Using try-catch since it's faster than if conditions when there's no problem
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyData> backingStore = new Dictionary<string, PropertyData>();

        /// <summary>
        /// Implementation of the <see cref="INotifyPropertyChanged.PropertyChanged"/> event. Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Registers a new property for the actual instance of <see cref="NotifyPropertyChanged"/>.
        /// </summary>
        /// <param name="name">Name of the registered property.</param>
        /// <param name="type">Type of the registered property.</param>
        /// <param name="defaultValue">Default value of the registered property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="name"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="defaultValue"/> is <c>null</c> while <paramref name="type"/> is non-nullable value type.
        ///     </para>
        ///     <para>
        ///         Value of <paramref name="defaultValue"/> cannot be assigned to property of type in <paramref name="type"/>.
        ///     </para>
        ///     <para>
        ///         Instance already contains registered property named as specified in parameter <paramref name="name"/>.
        ///     </para>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c></exception>
        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the 'type' parameter
        protected void RegisterProperty(string name, Type type, object defaultValue)
        {
            RegisterProperty(name, type, defaultValue, null);
        }

        /// <summary>
        /// Registers a new property for the actual instance of <see cref="NotifyPropertyChanged"/>.
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
        ///         Parameter <paramref name="defaultValue"/> is <c>null</c> while <paramref name="type"/> is non-nullable value type.
        ///     </para>
        ///     <para>
        ///         Value of <paramref name="defaultValue"/> cannot be assigned to property of type in <paramref name="type"/>.
        ///     </para>
        ///     <para>
        ///         Instance already contains registered property named as specified in parameter <paramref name="name"/>.
        ///     </para>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c></exception>
        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the 'type' parameter
        protected void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            Helpers.ValidateNotNullOrWhiteSpace(name, nameof(name));
            Helpers.ValidateNotNull(type, nameof(type));

            bool isNullable1 = type.Name == "Nullable`1";

            if (defaultValue == null)
            {
                if (type.GetIsValueType() && !isNullable1)
                {
                    throw new ArgumentException($"The type '{type}' is not a nullable type.");
                }
            }
            else
            {
                Type defaultValueType = defaultValue.GetType();

                if (defaultValueType != type && !defaultValueType.GetIsSubclassOf(type) && !(isNullable1 && type.ContainsGenericParameterConstraint(defaultValueType)))
                {
                    throw new ArgumentException($"The value in the '{nameof(defaultValue)}' parameter cannot be assigned to property of the specified type ({type})");
                }
            }

            try
            {
                backingStore.Add(name, new PropertyData(defaultValue, type, propertyChangedCallback));
            }
            catch (Exception exception)
            {
                if (backingStore.ContainsKey(name))
                {
                    throw new ArgumentException($"This class already contains registered property named '{name}'.");
                }

                throw exception;
            }
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
        ///         Instance does not contain registered property with name specified in <paramref name="propertyName"/>
        ///     </para>
        /// </exception>
#if NET_40
        protected object GetValue(string propertyName)
#else
        protected object GetValue([CallerMemberName]string propertyName = null)
#endif
        {
            try
            {
                return backingStore[propertyName].Value;
            }
            catch (Exception exception)
            {
                Helpers.ValidateNotNullOrWhiteSpace(propertyName, nameof(propertyName));
                ValidatePropertyName(propertyName);

                throw exception;
            }
        }

        /// <summary>
        /// Sets new value to a registered property even if it is equal and invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="value">New value for the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         The type of <paramref name="value"/> is not the same as the type of property specified in <paramref name="propertyName"/>.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Instance does not contain registered property with name specified in <paramref name="propertyName"/>
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
        /// Sets new value to a registered property if it's not equal and invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="value">New value for the property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         The type of <paramref name="value"/> is not the same as the type of property specified in <paramref name="propertyName"/>.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="propertyName"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Instance does not contain registered property with name specified in <paramref name="propertyName"/>
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
            PropertyData propertyData = backingStore[propertyName];

            if (propertyData.Type != value.GetType())
            {
                throw new ArgumentException($"The type of {nameof(value)} is not the same as the type of the '{propertyName}' property.");
            }

            try
            {
                // Calling Equals calls the overriden method even when the current type is object
                if (!propertyData.Value.Equals(value) || forceSetValue)
                {
                    object oldValue     = propertyData.Value;
                    propertyData.Value  = value;

                    propertyData.PropertyChangedCallback?.Invoke(this, new PropertyChangedCallbackArgs(oldValue, value));
                    OnPropertyChanged(propertyName);
                }
            }
            catch (Exception exception)
            {
                Helpers.ValidateNotNullOrWhiteSpace(propertyName, nameof(propertyName));
                ValidatePropertyName(propertyName);

                throw exception;
            }
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c> or white space.</exception>
#if NET_40
        protected void OnPropertyChanged(string propertyName)
#else
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
#endif
        {
            Helpers.ValidateNotNullOrWhiteSpace(propertyName, nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValidatePropertyName(string propertyName)
        {
            if (!backingStore.ContainsKey(propertyName))
            {
                throw new ArgumentException($"There is no registered property called '{propertyName}'.");
            }
        }

        private class PropertyData
        {
            internal object Value { get; set; }
            internal Type Type { get; }
            internal PropertyChangedCallbackHandler PropertyChangedCallback { get; }

            internal PropertyData(object defaultValue, Type type, PropertyChangedCallbackHandler propertyChangedCallback)
            {
                Value = defaultValue;
                Type  = type;
                PropertyChangedCallback = propertyChangedCallback;
            }
        }
    }

    /// <summary>
    /// Represents the callback that is invoked when a registered property value of the <see cref="NotifyPropertyChanged"/> class changes before the <see cref="NotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="sender">Instance of the <see cref="NotifyPropertyChanged"/> class that invoked this callback.</param>
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
            Handled  = false;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
