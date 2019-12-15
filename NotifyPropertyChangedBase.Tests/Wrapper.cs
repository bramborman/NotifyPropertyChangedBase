using System;

namespace NotifyPropertyChangedBase.Tests
{
    internal sealed class Wrapper : NotifyPropertyChanged
    {
        internal new bool IsPropertyChangedEventInvokingEnabled
        {
            get { return base.IsPropertyChangedEventInvokingEnabled; }
            set { base.IsPropertyChangedEventInvokingEnabled = value; }
        }
        internal new bool IsPropertyChangedCallbackInvokingEnabled
        {
            get { return base.IsPropertyChangedCallbackInvokingEnabled; }
            set { base.IsPropertyChangedCallbackInvokingEnabled = value; }
        }

        internal new void RegisterProperty(string name, Type type, object defaultValue)
        {
            base.RegisterProperty(name, type, defaultValue);
        }

        internal new void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            base.RegisterProperty(name, type, defaultValue, propertyChangedCallback);
        }

        internal new void RegisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            base.RegisterPropertyChangedCallback(propertyName, propertyChangedCallback);
        }

        internal new void UnregisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            base.UnregisterPropertyChangedCallback(propertyName, propertyChangedCallback);
        }

        internal new object GetValue(string propertyName)
        {
            return base.GetValue(propertyName);
        }

        internal new void ForceSetValue(object value, string propertyName)
        {
            base.ForceSetValue(value, propertyName);
        }

        internal new void SetValue(object value, string propertyName)
        {
            base.SetValue(value, propertyName);
        }

        internal new void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }

        internal new void OnPropertyChangedCallback(object oldValue, object newValue, string propertyName)
        {
            base.OnPropertyChangedCallback(oldValue, newValue, propertyName);
        }
    }
}
