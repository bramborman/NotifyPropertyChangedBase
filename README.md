# NotifyPropertyChangedBase
[![NuGet](https://img.shields.io/nuget/v/NotifyPropertyChangedBase.svg)](https://www.nuget.org/packages/NotifyPropertyChangedBase/)
[![Build status](https://ci.appveyor.com/api/projects/status/jc9gcr4gldjr8nq6/branch/master?svg=true)](https://ci.appveyor.com/project/bramborman/notifypropertychangedbase/branch/master)
[![Code coverage](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase/branch/master/graph/badge.svg)](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase)
[![Issues](https://img.shields.io/github/issues/bramborman/NotifyPropertyChangedBase.svg)](https://github.com/bramborman/NotifyPropertyChangedBase/issues)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/bramborman/NotifyPropertyChangedBase/blob/master/LICENSE.md)

NotifyPropertyChangedBase provides a simple to use yet powerful base class `NotifyPropertyChanged` that implements the `INotifyPropertyChanged` interface. Whether you're writing UWP, Xamarin, WPF or any other app, it will help you work with data.

This is an open-source project so feel free to send a pull request or open an issue.

### Pre-release
[![MyGet](https://img.shields.io/myget/bramborman/vpre/NotifyPropertyChangedBase.svg)][MyGet]
[![Build status](https://ci.appveyor.com/api/projects/status/jc9gcr4gldjr8nq6/branch/dev?svg=true)](https://ci.appveyor.com/project/bramborman/notifypropertychangedbase/branch/dev)
[![Code coverage](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase/branch/dev/graph/badge.svg)](https://codecov.io/gh/bramborman/NotifyPropertyChangedBase/branch/dev)

If you want to get updates more frequently or test bugfixes and new features before they go into production and you don't mind the possibility of new bugs, you can use the pre-release version of NotifyPropertyChangedBase from MyGet.org.

[**Download it here**][MyGet]

[MyGet]: https://www.myget.org/feed/bramborman/package/nuget/NotifyPropertyChangedBase

## How to use it?
NotifyPropertyChangedBase library helps you use `INotifyPropertyChanged` interface without needing to write your own logic. The one and only thing you need to use is an abstract class `NotifyPropertyChanged`. So instead of worrying about backing stores - variables that hold data of properties - about compairing each property you will register your property and our class will do the rest.

It's usage is similar to usage of `DependencyObject`, that you may be familiar with from UWP or WPF, however it does **not** inherit from it nor it can be used only on UI thread - **you can access it from any thread you want**.

As I said before, to be able to use any property (and benefit from the advantages of NotifyPropertyChangedBase), you have to register it using the `RegisterProperty` method. It has three required parameters and one overload accepting fourth parameter:
   - name of the property (using it you'll be accessing its value and doing all the work with the property)
   - type of the property
   - default value of the property
   - (optional) property changed callback - a method that will be invoked after given property changes, but before the `PropertyChanged` event is invoked, providing you info about the previous value of the property and it's new - current value

> Please note that unlike `DependencyProperty.Register` the `RegisterProperty` method here does **not** return anything and you don't have to store anything. You're accessing the property only using it's name.

Here's a simple class using some advantages of NotifyPropertyChangedBase. It has two properties, `Bar` and `Greeting`. Both are backed by `NotifyPropertyChanged` class so anytime their value is changed, the `PropertyChanged` event is automatically invoked. Then you can get their value by calling `GetValue` and set it using `SetValue` and `ForceSetValue` methods (we'll talk about their difference later).

All these methods have an argument `propertyName` specifying which property are you working with. But you can fully omit these as the compiler will pass the name of property/method from which these methods are called from because the argument has the [`CallerMemberNameAttribute`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute) (does **not** apply to .NET 4.0 where the attribute is **not** available).

>I'm using the [`nameof`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/nameof) keyword but you can of course use just a string i.e. `"Bar"` etc. when working with properties.

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

This is just a simple example. Of course you could call `GetValue`, `SetValue` and `ForceSetValue` anywhere in the code, not only in the body of related properties but using `Bar = 5;` over `SetValue(5, nameof(Bar));` and so on seems much simpler to me.

`GetValue` and `SetValue` methods does simply what their name implies - get or set the value of a property with the given name but there's another similar method - `ForceSetValue`. The difference between `SetValue` and `ForceSetValue` is that the latter always sets the new value to a property however `SetValue` checks whether the old value and the new one are the same using the `Equals` method (you may want to [override it](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) to achieve the desired result on this check). `SetValue` only assigns the new value and invokes the `PropertyChanged` event and `PropertyChangedCallback` only if the two values are **not** equal.

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

