#region License
// MIT License
//
// Copyright (c) 2017 Marian Dolinský
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
#endregion

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
