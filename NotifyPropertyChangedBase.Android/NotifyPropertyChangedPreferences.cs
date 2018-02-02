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
    /// <summary>
    /// Abstract base class inheriting from <see cref="NotifyPropertyChanged"/> adding Android shared preferences functionality.
    /// </summary>
    public abstract class NotifyPropertyChangedPreferences : NotifyPropertyChanged
    {
        private readonly Dictionary<string, (string name, object defaultValue)> propertyData = new Dictionary<string, (string, object)>();
        private readonly Dictionary<string, string> nameKeyDictionary = new Dictionary<string, string>();
        private readonly ISharedPreferences preferences;
        private readonly ISharedPreferencesEditor editor;
        private readonly OnSharedPreferenceChangeListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedPreferences"/> class using default shared preferences.
        /// </summary>
        /// <param name="context">The context where the preferences will be obtained.</param>
        protected NotifyPropertyChangedPreferences(Context context) : this(context, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedPreferences"/> class using shared preferences of the given name.
        /// </summary>
        /// <param name="context">The context where the preferences will be obtained.</param>
        /// <param name="sharedPrerefencesName">The name of the shared preferences to use.</param>
        protected NotifyPropertyChangedPreferences(Context context, string sharedPrerefencesName) : this(context, sharedPrerefencesName, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedPreferences"/> class using shared preferences of the given name.
        /// </summary>
        /// <param name="context">The context where the preferences will be obtained.</param>
        /// <param name="sharedPrerefencesName">The name of shared preferences to use.</param>
        /// <param name="fileCreationMode">Mode used when obtaining the shared preferences.</param>
        protected NotifyPropertyChangedPreferences(Context context, string sharedPrerefencesName, FileCreationMode fileCreationMode)
        {
            preferences = context.GetSharedPreferences(sharedPrerefencesName ?? PreferenceManager.GetDefaultSharedPreferencesName(context), fileCreationMode);
            editor = preferences.Edit();

            listener = new OnSharedPreferenceChangeListener(Listener_SharedPreferenceChanged);
            // We don't need to unregister this later since this reference is held as a weak reference
            preferences.RegisterOnSharedPreferenceChangeListener(listener);
        }

        /// <summary>
        /// Registers a new property in the actual instance of <see cref="NotifyPropertyChangedPreferences"/>.
        /// This property is synced with used shared preferences using the key specified.
        /// </summary>
        /// <param name="key">Key used for syncing with shared preferences.</param>
        /// <param name="name">Name of the registered property.</param>
        /// <param name="type">Type of the registered property.</param>
        /// <param name="defaultValue">Default value of the registered property.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="key"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="name"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Value of <paramref name="defaultValue"/> cannot be assigned to a property of type specified in the <paramref name="type"/> parameter.
        ///     </para>
        ///     <para>
        ///         Instance already contains a registered property named the same as specified in parameter <paramref name="name"/>.
        ///     </para>
        ///     <para>
        ///         Instance already contains a registered property with the same key as specified in parameter <paramref name="key"/>.
        ///     </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="type"/> is <c>null</c>.</exception>
        protected void RegisterPreferencesProperty(string key, string name, Type type, object defaultValue)
        {
            RegisterPreferencesProperty(key, name, type, defaultValue, null);
        }

        /// <summary>
        /// Registers a new property in the actual instance of <see cref="NotifyPropertyChangedPreferences"/>.
        /// This property is synced with used shared preferences using the key specified.
        /// </summary>
        /// <param name="key">Key used for syncing with shared preferences.</param>
        /// <param name="name">Name of the registered property.</param>
        /// <param name="type">Type of the registered property.</param>
        /// <param name="defaultValue">Default value of the registered property.</param>
        /// <param name="propertyChangedCallback">Callback invoked right after the registered property changes.</param>
        /// <exception cref="ArgumentException">
        ///     <para>
        ///         Parameter <paramref name="key"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Parameter <paramref name="name"/> is <c>null</c> or white space.
        ///     </para>
        ///     <para>
        ///         Value of <paramref name="defaultValue"/> cannot be assigned to a property of type specified in the <paramref name="type"/> parameter.
        ///     </para>
        ///     <para>
        ///         Instance already contains a registered property named the same as specified in parameter <paramref name="name"/>.
        ///     </para>
        ///     <para>
        ///         Instance already contains a registered property with the same key as specified in parameter <paramref name="key"/>.
        ///     </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="type"/> is <c>null</c>.</exception>
        protected void RegisterPreferencesProperty(string key, string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be white space or null.", nameof(key));
            }

            if (propertyData.ContainsKey(key))
            {
                throw new ArgumentException($"This instance already contains a registered property with key '{key}'.");
            }

            RegisterProperty(name, type, GetPreferencesValue(key, defaultValue), propertyChangedCallback);
            propertyData.Add(key, (name, defaultValue));
            nameKeyDictionary.Add(name, key);
        }

        /// <summary>
        /// Invokes the <see cref="NotifyPropertyChanged.PropertyChanged"/> event and if the changed property
        /// is synced with shared preferences, this method syncs it.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c> or white space.</exception>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (nameKeyDictionary.ContainsKey(propertyName))
            {
                SetPreferencesValue(nameKeyDictionary[propertyName], GetValue(propertyName));
            }

            base.OnPropertyChanged(propertyName);
        }

        private void Listener_SharedPreferenceChanged(string key)
        {
            if (propertyData.ContainsKey(key))
            {
                SetValue(GetPreferencesValue(key, propertyData[key].defaultValue), propertyData[key].name);
            }
        }
        
        private object GetPreferencesValue(string key, object defaultValue)
        {
            Type valueType = defaultValue?.GetType();

            if (valueType == typeof(bool))
            {
                return preferences.GetBoolean(key, (bool)defaultValue);
            }
            else if (valueType == typeof(int))
            {
                return preferences.GetInt(key, (int)defaultValue);
            }
            else if (valueType == typeof(string) || valueType == null)
            {
                return preferences.GetString(key, (string)defaultValue);
            }
            else if (valueType == typeof(float))
            {
                return preferences.GetFloat(key, (float)defaultValue);
            }
            else if (valueType == typeof(long))
            {
                return preferences.GetLong(key, (long)defaultValue);
            }
            else if (valueType == typeof(ICollection<string>))
            {
                return preferences.GetStringSet(key, (ICollection<string>)defaultValue);
            }
            else
            {
                throw new Exception($"Invalid type of {nameof(defaultValue)}.");
            }
        }

        private void SetPreferencesValue(string key, object newValue)
        {
            Type valueType = newValue?.GetType();

            if (valueType == typeof(bool))
            {
                editor.PutBoolean(key, (bool)newValue);
            }
            else if (valueType == typeof(int))
            {
                editor.PutInt(key, (int)newValue);
            }
            else if (valueType == typeof(string) || valueType == null)
            {
                editor.PutString(key, (string)newValue);
            }
            else if (valueType == typeof(float))
            {
                editor.PutFloat(key, (float)newValue);
            }
            else if (valueType == typeof(long))
            {
                editor.PutLong(key, (long)newValue);
            }
            else if (valueType == typeof(ICollection<string>))
            {
                editor.PutStringSet(key, (ICollection<string>)newValue);
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
