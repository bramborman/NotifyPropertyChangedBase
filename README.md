# NotifyPropertyChangedBase
[![NuGet](https://img.shields.io/nuget/v/NotifyPropertyChangedBase.svg)](https://www.nuget.org/packages/NotifyPropertyChangedBase/)
[![Build status](https://ci.appveyor.com/api/projects/status/jc9gcr4gldjr8nq6/branch/master?svg=true)](https://ci.appveyor.com/project/bramborman/notifypropertychangedbase/branch/master)
[![Code coverage](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase/branch/master/graph/badge.svg)](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase)
[![Issues](https://img.shields.io/github/issues/bramborman/NotifyPropertyChangedBase.svg)](https://github.com/bramborman/NotifyPropertyChangedBase/issues)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/bramborman/NotifyPropertyChangedBase/blob/master/LICENSE.md)

NotifyPropertyChangedBase provides a simple yet powerful base class `NotifyPropertyChanged` that implements the `INotifyPropertyChanged` interface. Whether you're writing UWP, Xamarin, WPF or any other app, it may help you work with data.

### Pre-release
[![MyGet](https://img.shields.io/myget/bramborman/vpre/NotifyPropertyChangedBase.svg)][MyGet]
[![Build status](https://ci.appveyor.com/api/projects/status/jc9gcr4gldjr8nq6/branch/dev?svg=true)](https://ci.appveyor.com/project/bramborman/notifypropertychangedbase/branch/dev)
[![Code coverage](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase/branch/dev/graph/badge.svg)](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase/branch/dev)

If you want to get updates more frequently and you don't mind the possibility of bugs, you can download the pre-release version of NotifyPropertyChangedBase from MyGet.org right [**here**][MyGet].

[MyGet]: https://www.myget.org/feed/bramborman/package/nuget/NotifyPropertyChangedBase

## How-to
NotifyPropertyChangedBase library consists of one namespace `NotifyPropertyChangedBase` which contains one class - `NotifyPropertyChanged`. This class implements the `INotifyPropertyChanged` interface and has some additional methods to help you use it. It's usage is similar to the usage of `DependencyObject` however it does not inherit from it nor it can be used only on UI thread - you can access it from any thread you want.

Here is a simple class that inherits from `NotifyPropertyChanged` class.
To be able to use any property, you have to register it using the `RegisterProperty` method. It has three required parameters:
   - name of the property (using it you'll be accessing its value and doing all the work with the property)
   - type of the property
   - default value of the property
   - (optional) property changed callback - a method that will be invoked when given property changes providing you info about the previous value of the property and it's new - current value

> Please note that unlike `DependencyProperty.Register` the `RegisterProperty` method here does not return anything and you don't have to store anything. You're accessing the property only using it's name.

    using NotifyPropertyChangedBase;

    class Foo : NotifyPropertyChanged
    {
        public int Bar
        {
            get { return (int)GetValue(); }
            set { SetValue(value); }
        }
        public string Greeting
        {
            get { return (string)GetValue(); }
            set { SetValue(value); }
        }
    
        public Foo()
        {
            RegisterProperty(nameof(Bar), typeof(int), 0);
            RegisterProperty(nameof(Greeting), typeof(string), null, GreetingPropertyChanged);
        }

        private void GreetingPropertyChanged(NotifyPropertyChanged sender, PropertyChangedCallbackArgs e)
        {
            Console.WriteLine($"Value of Greeting changed from '{e.OldValue}' to '{e.NewValue}'");
        }
    }

### Working with properties
In the first example we've been using the simpliest way of working with properties - `GetValue` and `SetValue` methods that works without/with one parameter and you don't have to specify the name of the property you're working with. That's because these methods, similarly to the `ForceSetValue` method that we'll be talking about later uses [`CallerMemberNameAttribute`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute) that does it for you. But you can also specify the name of the property you want to work with manually, for instance, `GetValue("Bar")` will return the actual value of the property `Bar`.

`GetValue` and `SetValue` methods does simply what their name implies - get or set the value of a property with the given name but there's another similar method - `ForceSetValue`. The difference between `SetValue` and `ForceSetValue` is that the latter always sets the new value to a property however `SetValue` checks whether the old value and the new one are the same using the `Equals` (you may want to [override it](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) to achieve the desired result on this check) and assigns the new value and tries to invoke the `PropertyChanged` event and `PropertyChangedCallback` only if the two values are not equal.

### Structure of the `NotifyPropertyChanged` class
All the members of this class are `protected` - only derived classes can use them.
    
    protected bool IsPropertyChangedCallbackInvokingEnabled { get; set; }
    protected bool IsPropertyChangedEventInvokingEnabled { get; set; }
    protected void RegisterProperty(string name, Type type, object defaultValue
    protected void RegisterProperty(string name, Type type, object defaultValue, PropertyChangedCallbackHandler propertyChangedCallback)
    protected void RegisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
    protected void UnregisterPropertyChangedCallback(string propertyName, PropertyChangedCallbackHandler propertyChangedCallback)
    protected object GetValue([CallerMemberName]string propertyName = null)
    protected void ForceSetValue(object value, [CallerMemberName]string propertyName = null)
    protected void SetValue(object value, [CallerMemberName]string propertyName = null)
    protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
