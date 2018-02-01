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
NotifyPropertyChangedBase library helps you use `INotifyPropertyChanged` interface without needing to write your own logic. The one and only thing you need to do is to make your models inherit from the abstract class `NotifyPropertyChanged`. So instead of worrying about backing stores - variables that hold data of properties - about compairing data or calling the `PropertyChanged` event you will register your property and this class will do the rest.

It's usage is very similar to the usage of `DependencyObject`, that you may be familiar with from UWP or WPF, however it does **not** inherit from it nor it can only be used on the UI thread. **You can access it from any thread you want**.

To benefit from the advantages of NotifyPropertyChangedBase, you have to register your property using the `RegisterProperty` method. It has three required parameters and one overload accepting fourth parameter:
   - name of the property (using it you'll be accessing its value and doing all the work with the property)
   - type of the property
   - default value of the property
   - (optional) property changed callback - a method that will be invoked after given property changes, but before the `PropertyChanged` event is invoked, providing you info about the previous value of the property and it's new - current value

> Please note that unlike `DependencyProperty.Register` the `RegisterProperty` method here does **not** return anything and you don't have to store anything. You're accessing the property using only it's name.

To get and set values of registered properties you'll use `GetValue` and `SetValue` or `ForceSetValue`. The difference between `SetValue` and `ForceSetValue` is that the latter **always** sets the new value to a property and invokes the `PropertyChanged` event and registered callbacks, **no matter whether the value is different from the current one**. However `SetValue` checks whether the old value and the new one are differet using the `Equals` method (you may want to [override it](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) to achieve the desired result on this check). `SetValue` assigns the new value and invokes the `PropertyChanged` event and registered callbacks **only** if the two values are **not** equal.

All these methods have an argument `propertyName` specifying which property are you working with but you can fully omit these as the compiler will pass the name of property/method from which these methods are called from because the argument has the [`CallerMemberNameAttribute`](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute). `SetValue` and `ForceSetValue` have one another argument that is passed before the `propertyName` containing the value to be set to given property.

There's also the `OnPropertyChanged` method, also having a `propertyName` argument with the same attribute, which you can use to invoke the `PropertyChanged` event manually.

#### PropertyChangedCallback
Besides the `PropertyChanged` event that is invoked when any of the properties changes, NotifyPropertyChangedBase provides the `PropertyChangedCallback` that is registered for each property independently. Registered callbacks are invoked **always** before the `PropertyChanged` event.

There are two ways to register them - using overloaded `RegisterProperty` method which accepts a delegate of type `PropertyChangedCallback` as the last argument or using `RegisterPropertyChangedCallback`. The latter is designed to add another callback any time after registering the property. You can also unregister a callback, registered using whichever of those two methods, using the `UnregisterPropertyChangedCallback` method.

The handlers of these callbacks get two values as arguments - `sender` which holds the reference to the class that invoked that certain callback and `e` of type `PropertyChangedCallbackArgs` which has the following properties:

   - `Handled` - indicates whether this certain callback has been handled. Default value is `false`.
   - `OldValue` - holds the previous value of the related property.
   - `NewValue` - holds the current value of the related property.

#### Having a control over everything
Using the `IsPropertyChangedEventInvokingEnabled` and `IsPropertyChangedCallbackInvokingEnabled` properties you can enable/disable invocation of the `PropertyChanged` event or registered callbacks. Their default value is `true` but setting them to `false` will disable the respected events so even the `ForceSetValue` method will **not** be invoking them.

> Setting `IsPropertyChangedCallbackInvokingEnabled` to `false` will **not** unregister already registered callbacks.

#### Example
Here's a simple class using some advantages of NotifyPropertyChangedBase. It has two properties, `Bar` and `Greeting`. Both are backed by `NotifyPropertyChanged` class so anytime their value is changed, the `PropertyChanged` event is automatically invoked.

```csharp
    using NotifyPropertyChangedBase;

    class Foo : NotifyPropertyChanged
    {
        public int Bar
        {
            get { return (int)GetValue(); }
            set { SetValue(value); }

            // These will do the same:
            // get { return (int)GetValue(nameof(Bar)); }
            // get { return (int)GetValue("Bar"); }
        }
        public string Greeting
        {
            get { return (string)GetValue(); }
            set { SetValue(value); }
        }
    
        public Foo()
        {
            // Property without a callback
            RegisterProperty(nameof(Bar), typeof(int), 0);
            // This will do the same:
            // RegisterProperty("Bar", typeof(int), 0);

            // Property with a callback
            RegisterProperty(nameof(Greeting), typeof(string), null, GreetingPropertyChanged);
        }

        private void GreetingPropertyChanged(NotifyPropertyChanged sender, PropertyChangedCallbackArgs e)
        {
            Console.WriteLine($"Value of Greeting changed from '{e.OldValue}' to '{e.NewValue}'");
        }
    }
```

>I'm using the [`nameof`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/nameof) keyword but you can of course use just a string i.e. `"Bar"` etc. when working with properties.

This is just a simple example. Of course you can call `GetValue`, `SetValue` and `ForceSetValue` anywhere in the code, **not** only in the body of related properties but using `Bar = 5;` over `SetValue(5, nameof(Bar));` and so on seems much simpler to me.

#### Structure of the `NotifyPropertyChanged` class
All the members of this class (except of the `PropertyChanged` event) are `protected` so only derived classes can use them.
