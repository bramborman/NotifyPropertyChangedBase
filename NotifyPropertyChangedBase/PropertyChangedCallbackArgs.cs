// ---------------------------------------------------------------------------------------
// <copyright file="PropertyChangedCallbackArgs.cs" company="Marian Dolinský">
// Copyright (c) Marian Dolinský. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------------

namespace NotifyPropertyChangedBase
{
    /// <summary>
    /// Represents the callback that is invoked when a property registered in the <see cref="NotifyPropertyChanged"/> class changes.
    /// </summary>
    /// <param name="sender">Object that invoked this callback.</param>
    /// <param name="e">Callback data containing info about the changed property.</param>
    public delegate void PropertyChangedCallbackHandler(NotifyPropertyChanged sender, PropertyChangedCallbackArgs e);

    /// <summary>
    /// Callback data containing info about the changed property in the <see cref="PropertyChangedCallbackHandler"/>.
    /// </summary>
    public sealed class PropertyChangedCallbackArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the callback as handled.
        /// </summary>
        public bool Handled { get; set; }
        /// <summary>
        /// Gets the previous value of the changed property.
        /// </summary>
        public object OldValue { get; }
        /// <summary>
        /// Gets the current value of the changed property.
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedCallbackArgs"/> class.
        /// </summary>
        /// <param name="oldValue">Previous value of the changed property.</param>
        /// <param name="newValue">Current value of the changed property.</param>
        public PropertyChangedCallbackArgs(object oldValue, object newValue)
        {
            Handled = false;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
