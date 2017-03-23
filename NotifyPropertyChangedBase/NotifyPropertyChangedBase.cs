using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !NET_40
using System.Runtime.CompilerServices;
#endif

namespace NotifyPropertyChangedBase
{
    public delegate void PropertyChangedAction(object oldValue, object newValue);

    // Using try-catch since it's faster than if conditions when there's no problem
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyData> backingStore = new Dictionary<string, PropertyData>();

        public event PropertyChangedEventHandler PropertyChanged;

        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the type parameter
        protected void RegisterProperty(string name, Type type, object defaultValue)
        {
            RegisterProperty(name, type, defaultValue, null);
        }

        // Make changes in the NotifyPropertyChangedBase.Uap.rd file if you change something with the type parameter
        protected void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedAction propertyChangedAction)
        {
            bool isNullableOfT = type.Name == "Nullable`1";

            Helpers.ValidateNotNullOrWhiteSpace(name, nameof(name));
            Helpers.ValidateNotNull(type, nameof(type));

            if (defaultValue == null && type.GetIsValueType() && !isNullableOfT)
            {
                throw new ArgumentException($"The type '{type}' is not a nullable type.");
            }

            try
            {
                // For example:
                // RegisterProperty("Foo", typeof(double), 1);
                // In this case the defaultValue type is Int32 and not Double so we change it
                if (!isNullableOfT)
                {
                    defaultValue = Convert.ChangeType(defaultValue, type);
                }
                
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
            // Using try-catch since it's faster than if conditions when there's no problem
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
                    throw new ArgumentException($"The type of {nameof(newValue)} is not the same as the type of the '{propertyName}' property.");
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
