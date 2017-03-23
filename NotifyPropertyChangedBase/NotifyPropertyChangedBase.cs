using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !NET_40
using System.Runtime.CompilerServices;
#endif

namespace NotifyPropertyChangedBase
{
    public delegate void PropertyChangedAction(object oldValue, object newValue);

    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyData> backingStore = new Dictionary<string, PropertyData>();

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void RegisterProperty(string name, Type type, object defaultValue)
        {
            RegisterProperty(name, type, defaultValue, null);
        }

        protected void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedAction propertyChangedAction)
        {
            ValidateNotNullOrWhiteSpace(name, nameof(name));
            ValidateNotNull(type, nameof(type));

            // Using try-catch since it's faster than if conditions when there's no problem
            try
            {
                backingStore.Add(name, new PropertyData(type.Name == "Nullable`1" ? defaultValue : Convert.ChangeType(defaultValue, type), type, propertyChangedAction));
            }
            catch (Exception exception)
            {
                if (backingStore.ContainsKey(name))
                {
                    throw new ArgumentException($"This class already contains registered property named {name}.");
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
            // Using try-catch since it's faster than if conditions when there's no problem
            try
            {
                return backingStore[propertyName].Value;
            }
            catch (Exception exception)
            {
                ValidateNotNullOrWhiteSpace(propertyName, nameof(propertyName));
                ValidatePropertyName(propertyName);

                throw exception;
            }
        }

#if NET_40
        protected void ForceSetValue(object newValue, string propertyName)
#else
        protected void ForceSetValue(object newValue, [CallerMemberName]string propertyName = null)
#endif
        {
            SetValue(newValue, propertyName, true);
        }

#if NET_40
        protected void SetValue(object newValue, string propertyName)
#else
        protected void SetValue(object newValue, [CallerMemberName]string propertyName = null)
#endif
        {
            SetValue(newValue, propertyName, false);
        }

        private void SetValue(object newValue, string propertyName, bool forceSetValue)
        {
            // Using try-catch since it's faster than if conditions when there's no problem
            try
            {
                PropertyData propertyData = backingStore[propertyName];

                if (propertyData.Type != newValue.GetType())
                {
                    throw new ArgumentException($"The type of {nameof(newValue)} is not the same as the type of {propertyName} property.");
                }

                // Calling Equals calls the overriden method even when the current type is object
                if (!propertyData.Value.Equals(newValue) || forceSetValue)
                {
                    object oldValue = propertyData.Value;
                    propertyData.Value = newValue;

                    propertyData.PropertyChangedAction?.Invoke(oldValue, newValue);
                    OnPropertyChanged(propertyName);
                }
            }
            catch (Exception exception)
            {
                ValidateNotNullOrWhiteSpace(propertyName, nameof(propertyName));
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
            ValidateNotNullOrWhiteSpace(propertyName, nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValidatePropertyName(string propertyName)
        {
            if (!backingStore.ContainsKey(propertyName))
            {
                throw new ArgumentException($"There is no registered property called {propertyName}.");
            }
        }

        private void ValidateNotNull(object obj, string parameterName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private void ValidateNotNullOrWhiteSpace(string str, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Value cannot be white space or null.", parameterName);
            }
        }

        private class PropertyData
        {
            internal object Value { get; set; }
            internal Type Type { get; }
            internal PropertyChangedAction PropertyChangedAction { get; }

            internal PropertyData(object defaultValue, Type type, PropertyChangedAction propertyChangedAction)
            {
                Value = defaultValue;
                Type  = type;
                PropertyChangedAction = propertyChangedAction;
            }
        }
    }
}
