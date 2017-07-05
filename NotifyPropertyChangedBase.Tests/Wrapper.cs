using System;
using System.Runtime.CompilerServices;

namespace NotifyPropertyChangedBase.Tests
{
    internal sealed class Wrapper : NotifyPropertyChanged
    {
        internal new bool IsPropertyChangedCallbackInvokingEnabled
        {
            get { return base.IsPropertyChangedCallbackInvokingEnabled; }
            set { base.IsPropertyChangedCallbackInvokingEnabled = value; }
        }
        internal new bool IsPropertyChangedEventInvokingEnabled
        {
            get { return base.IsPropertyChangedEventInvokingEnabled; }
            set { base.IsPropertyChangedEventInvokingEnabled = value; }
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

        internal new object GetValue([CallerMemberName]string propertyName = null)
        {
            return base.GetValue(propertyName);
        }

        internal new void ForceSetValue(object value, [CallerMemberName]string propertyName = null)
        {
            base.ForceSetValue(value, propertyName);
        }

        internal new void SetValue(object value, [CallerMemberName]string propertyName = null)
        {
            base.SetValue(value, propertyName);
        }

        internal new void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}
