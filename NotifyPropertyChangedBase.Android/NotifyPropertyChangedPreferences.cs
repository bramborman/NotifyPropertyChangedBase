// MIT License
//
// Copyright (c) 2018 Marian Dolinský
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

using Android.Content;
using Android.Preferences;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NotifyPropertyChangedBase.Android
{
    public abstract class NotifyPropertyChangedPreferences : NotifyPropertyChanged
    {
        private readonly Dictionary<string, (string name, object defaultValue)> propertyData = new Dictionary<string, (string, object)>();
        private readonly Dictionary<string, string> nameKeyDictionary = new Dictionary<string, string>();
        private readonly ISharedPreferences preferences;
        private readonly ISharedPreferencesEditor editor;
        private readonly OnSharedPreferenceChangeListener listener;

        protected NotifyPropertyChangedPreferences(Context context) : this(context, null)
        {

        }

        protected NotifyPropertyChangedPreferences(Context context, string sharedPrerefencesName) : this(context, sharedPrerefencesName, 0)
        {

        }

        protected NotifyPropertyChangedPreferences(Context context, string sharedPrerefencesName, FileCreationMode fileCreationMode)
        {
            preferences = context.GetSharedPreferences(sharedPrerefencesName ?? PreferenceManager.GetDefaultSharedPreferencesName(context), fileCreationMode);
            editor = preferences.Edit();
            listener = new OnSharedPreferenceChangeListener(Listener_SharedPreferenceChanged);
            preferences.RegisterOnSharedPreferenceChangeListener(listener);
        }

        protected void RegisterAppStoreProperty(string key, string name, Type type, object defaultValue)
        {
            RegisterAppStoreProperty(key, name, type, defaultValue, null);
        }

        protected void RegisterAppStoreProperty(string key, string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            if (propertyData.ContainsKey(key))
            {
                throw new ArgumentException($"This class already contains a registered property with key '{key}'.");
            }

            RegisterProperty(name, type, GetAppStoreValue(key, defaultValue), propertyChangedCallback);
            propertyData.Add(key, (name, defaultValue));
            nameKeyDictionary.Add(name, key);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (nameKeyDictionary.ContainsKey(propertyName))
            {
                SetAppStoreValue(nameKeyDictionary[propertyName], GetValue(propertyName));
            }

            base.OnPropertyChanged(propertyName);
        }

        private void Listener_SharedPreferenceChanged(string key)
        {
            if (propertyData.ContainsKey(key))
            {
                SetValue(GetAppStoreValue(key, propertyData[key].defaultValue), propertyData[key].name);
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

        protected void Detach()
        {
            preferences.UnregisterOnSharedPreferenceChangeListener(listener);
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
