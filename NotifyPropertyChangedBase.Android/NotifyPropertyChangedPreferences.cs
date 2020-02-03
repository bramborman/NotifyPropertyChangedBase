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
    [Obsolete("This class uses the deprecated Android.Preferences.PreferenceManager.")]
    public abstract class NotifyPropertyChangedPreferences : NotifyPropertyChanged, IDisposable
    {
        private readonly Dictionary<string, (string Name, object DefaultValue, Type Type)> _propertyData = new Dictionary<string, (string, object, Type)>();
        private readonly Dictionary<string, string> _nameKeyDictionary = new Dictionary<string, string>();
        private readonly ISharedPreferences _preferences;
        private readonly ISharedPreferencesEditor _editor;
        private readonly OnSharedPreferenceChangeListener _listener;

        private bool _isDisposed = false;

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
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

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
            ThrowIfDisposed();
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
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be white space or null.", nameof(key));
            }

            if (_propertyData.ContainsKey(key))
            {
                throw new ArgumentException($"This instance already contains a registered property with key '{key}'.");
            }

            RegisterProperty(name, type, GetPreferencesValue(key, defaultValue, type), propertyChangedCallback);
            _propertyData.Add(key, (name, defaultValue, type));
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
            ThrowIfDisposed();

            if (_nameKeyDictionary.ContainsKey(propertyName))
            {
                string key = _nameKeyDictionary[propertyName];
                SetPreferencesValue(key, GetValue(propertyName), _propertyData[key].Type);
            }

            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Saves values of all preference properties to shared preferences.
        /// </summary>
        protected void SaveAllToPreferences()
        {
            ThrowIfDisposed();

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
            ThrowIfDisposed();

            // GetValue will check the propertyName's value for us - no need
            // to check if the property name exists here
            // TODO: Or do we need to not throw an exception when getting _nameKeyDictionary value?
            string key = _nameKeyDictionary[propertyName];
            SetPreferencesValue(key, GetValue(propertyName), _propertyData[key].Type);
        }

        private void Listener_SharedPreferenceChanged(string key)
        {
            ThrowIfDisposed();

            if (_propertyData.ContainsKey(key))
            {
                (string Name, object DefaultValue, Type Type) propertyData = _propertyData[key];
                SetValue(GetPreferencesValue(key, propertyData.DefaultValue, propertyData.Type), propertyData.Name);
            }
        }

        private object GetPreferencesValue(string key, object defaultValue, Type type)
        {
            if (type == typeof(bool))
            {
                return _preferences.GetBoolean(key, (bool)defaultValue);
            }
            else if (type == typeof(int))
            {
                return _preferences.GetInt(key, (int)defaultValue);
            }
            else if (type == typeof(string))
            {
                return _preferences.GetString(key, (string)defaultValue);
            }
            else if (type == typeof(ICollection<string>))
            {
                return _preferences.GetStringSet(key, (ICollection<string>)defaultValue);
            }
            else if (type == typeof(float))
            {
                return _preferences.GetFloat(key, (float)defaultValue);
            }
            else if (type == typeof(long))
            {
                return _preferences.GetLong(key, (long)defaultValue);
            }

            throw new ArgumentException($"Invalid type: '{type}'.");
        }

        private void SetPreferencesValue(string key, object newValue, Type type)
        {
            if (type == typeof(bool))
            {
                _editor.PutBoolean(key, (bool)newValue);
            }
            else if (type == typeof(int))
            {
                _editor.PutInt(key, (int)newValue);
            }
            else if (type == typeof(string))
            {
                _editor.PutString(key, (string)newValue);
            }
            else if (type == typeof(ICollection<string>))
            {
                _editor.PutStringSet(key, (ICollection<string>)newValue);
            }
            else if (type == typeof(float))
            {
                _editor.PutFloat(key, (float)newValue);
            }
            else if (type == typeof(long))
            {
                _editor.PutLong(key, (long)newValue);
            }
            else
            {
                throw new ArgumentException($"Invalid type: '{type}'.");
            }

            _editor.Apply();
        }

        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                _preferences.Dispose();
                _editor.Dispose();
                _listener.Dispose();

                _propertyData.Clear();
                _nameKeyDictionary.Clear();
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NotifyPropertyChangedPreferences()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
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
