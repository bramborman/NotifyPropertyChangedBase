using Android.Content;
using Android.Preferences;
using System;
using System.Collections.Generic;

namespace NotifyPropertyChangedBase.Android
{
    public abstract class AppStore : NotifyPropertyChanged
    {
        private readonly Dictionary<string, (string name, object defaultValue)> keyNameDictionary = new Dictionary<string, (string, object)>();
        private readonly ISharedPreferences preferences;
        private readonly ISharedPreferencesEditor editor;
        private readonly OnSharedPreferenceChangeListener listener;

        protected AppStore(Context context) : this(context, null)
        {

        }

        protected AppStore(Context context, string sharedPrerefencesName) : this(context, sharedPrerefencesName, 0)
        {

        }

        protected AppStore(Context context, string sharedPrerefencesName, FileCreationMode fileCreationMode)
        {
            preferences = context.GetSharedPreferences(sharedPrerefencesName ?? PreferenceManager.GetDefaultSharedPreferencesName(context), fileCreationMode);
            editor = preferences.Edit();
            listener = new OnSharedPreferenceChangeListener(Listener_SharedPreferenceChanged);
        }

        protected void RegisterAppStoreProperty(string key, string name, Type type, object defaultValue)
        {
            RegisterAppStoreProperty(key, name, type, defaultValue, null);
        }

        protected void RegisterAppStoreProperty(string key, string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            if (keyNameDictionary.ContainsKey(key))
            {
                throw new ArgumentException($"This class already contains a registered property with key '{key}'.");
            }

            RegisterProperty(name, type, defaultValue, propertyChangedCallback);
            keyNameDictionary.Add(key, (name, defaultValue));
        }

        private void Listener_SharedPreferenceChanged(string key)
        {
            if (keyNameDictionary.ContainsKey(key))
            {
                SetValue(GetAppStoreValue(key, keyNameDictionary[key].defaultValue), keyNameDictionary[key].name);
            }
        }

        private object GetAppStoreValue(string key, object defaultValue)
        {
            Type valueType = defaultValue?.GetType();

            if (valueType == typeof(bool))
            {
                return preferences.GetBoolean(key, (bool)defaultValue);
            }
            else if (valueType == typeof(string) || valueType == null)
            {
                return preferences.GetString(key, (string)defaultValue);
            }
            else if (valueType == typeof(int))
            {
                return preferences.GetInt(key, (int)defaultValue);
            }
            else if (valueType == typeof(float))
            {
                return preferences.GetFloat(key, (float)defaultValue);
            }
            else if (valueType == typeof(long))
            {
                return preferences.GetLong(key, (long)defaultValue);
            }
            else
            {
                throw new Exception($"Invalid type of {nameof(defaultValue)}.");
            }
        }

        private void SetAppStoreValue(string key, object newValue)
        {
            Type valueType = newValue?.GetType();

            if (valueType == typeof(bool))
            {
                editor.PutBoolean(key, (bool)newValue);
            }
            else if (valueType == typeof(string) || valueType == null)
            {
                editor.PutString(key, (string)newValue);
            }
            else if (valueType == typeof(int))
            {
                editor.PutInt(key, (int)newValue);
            }
            else if (valueType == typeof(float))
            {
                editor.PutFloat(key, (float)newValue);
            }
            else if (valueType == typeof(long))
            {
                editor.PutLong(key, (long)newValue);
            }
            else
            {
                throw new Exception($"Invalid type of {nameof(newValue)}.");
            }

            editor.Apply();
        }

        private sealed class OnSharedPreferenceChangeListener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
        {
            private readonly Action<string> sharedPreferenceChanged;

            public OnSharedPreferenceChangeListener(Action<string> sharedPreferenceChanged)
            {
                this.sharedPreferenceChanged = sharedPreferenceChanged;
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                sharedPreferenceChanged(key);
            }
        }
    }
}
