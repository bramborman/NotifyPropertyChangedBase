// ---------------------------------------------------------------------------------------
// <copyright file="NotifyPropertyChangedPreferences.cs" company="Marian Dolinský">
// Copyright (c) Marian Dolinský. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Preferences;

namespace NotifyPropertyChangedBase.Android
{
    /// <summary>
    /// Abstract base class extending <see cref="NotifyPropertyChanged"/> of Android shared preferences functionality.
    /// </summary>
    public abstract class NotifyPropertyChangedPreferences : NotifyPropertyChanged
    {
        private readonly Dictionary<string, (string name, object defaultValue)> _propertyData = new Dictionary<string, (string, object)>();
        private readonly Dictionary<string, string> _nameKeyDictionary = new Dictionary<string, string>();
        private readonly ISharedPreferences _preferences;
        private readonly ISharedPreferencesEditor _editor;
        private readonly OnSharedPreferenceChangeListener _listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedPreferences"/> class using default shared preferences.
        /// </summary>
        /// <param name="context">The context where the preferences will be obtained.</param>
        protected NotifyPropertyChangedPreferences(Context context)
            : this(context, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedPreferences"/> class using shared preferences of the given name.
        /// </summary>
        /// <param name="context">The context where the preferences will be obtained.</param>
        /// <param name="sharedPrerefencesName">The name of the shared preferences to use.</param>
        protected NotifyPropertyChangedPreferences(Context context, string sharedPrerefencesName)
            : this(context, sharedPrerefencesName, 0)
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
            _preferences = context.GetSharedPreferences(sharedPrerefencesName ?? PreferenceManager.GetDefaultSharedPreferencesName(context), fileCreationMode);
            _editor = _preferences.Edit();

            _listener = new OnSharedPreferenceChangeListener(Listener_SharedPreferenceChanged);
            // We don't need to unregister this later since this reference is held as a weak reference
            _preferences.RegisterOnSharedPreferenceChangeListener(_listener);
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

            if (_propertyData.ContainsKey(key))
            {
                throw new ArgumentException($"This instance already contains a registered property with key '{key}'.");
            }

            RegisterProperty(name, type, GetPreferencesValue(key, defaultValue), propertyChangedCallback);
            _propertyData.Add(key, (name, defaultValue));
            _nameKeyDictionary.Add(name, key);
        }

        /// <summary>
        /// Invokes the <see cref="NotifyPropertyChanged.PropertyChanged"/> event and if the changed property
        /// is synced with shared preferences, this method syncs it.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <c>null</c> or white space.</exception>
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_nameKeyDictionary.ContainsKey(propertyName))
            {
                SetPreferencesValue(_nameKeyDictionary[propertyName], GetValue(propertyName));
            }

            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Saves values of all preference properties to shared preferences.
        /// </summary>
        protected void SaveAllToPreferences()
        {
            foreach (string name in _nameKeyDictionary.Keys)
            {
                SaveToPreferences(name);
            }
        }

        /// <summary>
        /// Saves a value of the registered preferences property to shared preferences.
        /// </summary>
        /// <param name="propertyName">The name of a registered property whose value should be saved.</param>
        protected void SaveToPreferences(string propertyName)
        {
            // GetValue will check the propertyName's value for us - no need
            // to check if the property name exists here
            SetPreferencesValue(_nameKeyDictionary[propertyName], GetValue(propertyName));
        }

        private void Listener_SharedPreferenceChanged(string key)
        {
            if (_propertyData.ContainsKey(key))
            {
                SetValue(GetPreferencesValue(key, _propertyData[key].defaultValue), _propertyData[key].name);
            }
        }

        private object GetPreferencesValue(string key, object defaultValue)
        {
            Type valueType = defaultValue?.GetType();

            if (valueType == typeof(bool))
            {
                return _preferences.GetBoolean(key, (bool)defaultValue);
            }
            else if (valueType == typeof(int))
            {
                return _preferences.GetInt(key, (int)defaultValue);
            }
            else if (valueType == typeof(string) || valueType == null)
            {
                return _preferences.GetString(key, (string)defaultValue);
            }
            else if (valueType == typeof(float))
            {
                return _preferences.GetFloat(key, (float)defaultValue);
            }
            else if (valueType == typeof(long))
            {
                return _preferences.GetLong(key, (long)defaultValue);
            }
            else if (valueType == typeof(ICollection<string>))
            {
                return _preferences.GetStringSet(key, (ICollection<string>)defaultValue);
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
                _editor.PutBoolean(key, (bool)newValue);
            }
            else if (valueType == typeof(int))
            {
                _editor.PutInt(key, (int)newValue);
            }
            else if (valueType == typeof(string) || valueType == null)
            {
                _editor.PutString(key, (string)newValue);
            }
            else if (valueType == typeof(float))
            {
                _editor.PutFloat(key, (float)newValue);
            }
            else if (valueType == typeof(long))
            {
                _editor.PutLong(key, (long)newValue);
            }
            else if (valueType == typeof(ICollection<string>))
            {
                _editor.PutStringSet(key, (ICollection<string>)newValue);
            }
            else
            {
                throw new Exception($"Invalid type of {nameof(newValue)}.");
            }

            _editor.Apply();
        }


        private sealed class OnSharedPreferenceChangeListener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
        {
            private readonly Action<string> _sharedPreferenceChanged;

            public OnSharedPreferenceChangeListener(Action<string> sharedPreferenceChanged)
            {
                _sharedPreferenceChanged = sharedPreferenceChanged;
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                _sharedPreferenceChanged(key);
            }
        }
    }
}
