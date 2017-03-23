using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !NET_40
using System.Runtime.CompilerServices;
#endif

namespace NotifyPropertyChangedBase
{
    // Using try-catch since it's faster than if conditions when there's no problem
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyData> backingStore = new Dictionary<string, PropertyData>();

        public event PropertyChangedEventHandler PropertyChanged;

        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the 'type' parameter
        protected void RegisterProperty(string name, Type type, object defaultValue)
        {
            RegisterProperty(name, type, defaultValue, null);
        }

        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the 'type' parameter
        protected void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedCallbackEventHandler propertyChangedAction)
        {
            Helpers.ValidateNotNullOrWhiteSpace(name, nameof(name));
            Helpers.ValidateNotNull(type, nameof(type));

            if (defaultValue == null)
            {
                if (type.GetIsValueType() && type.Name != "Nullable`1")
                {
                    throw new ArgumentException($"The type '{type}' is not a nullable type.");
                }
            }
            else
            {
                if (defaultValue.GetType() != type && !defaultValue.GetType().GetIsSubclassOf(type))
                {
                    throw new ArgumentException($"The value in the '{nameof(defaultValue)}' parameter cannot be assigned to property of the specified type ({type})");
                }
            }

            try
            {
                backingStore.Add(name, new PropertyData(defaultValue, type, propertyChangedAction));
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

#if NET_40
        protected void ForceSetValue(object value, string propertyName)
#else
        protected void ForceSetValue(object value, [CallerMemberName]string propertyName = null)
#endif
        {
            SetValue(value, propertyName, true);
        }

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
            try
            {
                PropertyData propertyData = backingStore[propertyName];

                if (propertyData.Type != value.GetType())
                {
                    throw new ArgumentException($"The type of {nameof(value)} is not the same as the type of the '{propertyName}' property.");
                }

                // Calling Equals calls the overriden method even when the current type is object
                if (!propertyData.Value.Equals(value) || forceSetValue)
                {
                    object oldValue = propertyData.Value;
                    propertyData.Value = value;

                    propertyData.PropertyChangedAction?.Invoke(this, new PropertyChangedCallbackEventArgs(oldValue, value));
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
            internal PropertyChangedCallbackEventHandler PropertyChangedAction { get; }

            internal PropertyData(object defaultValue, Type type, PropertyChangedCallbackEventHandler propertyChangedAction)
            {
                Value = defaultValue;
                Type  = type;
                PropertyChangedAction = propertyChangedAction;
            }
        }
    }

    public delegate void PropertyChangedCallbackEventHandler(NotifyPropertyChanged sender, PropertyChangedCallbackEventArgs e);

    public sealed class PropertyChangedCallbackEventArgs
    {
        public bool Handled { get; set; }
        public object OldValue { get; }
        public object NewValue { get; }

        public PropertyChangedCallbackEventArgs(object oldValue, object newValue)
        {
            Handled  = false;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
