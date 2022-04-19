
# Injector

with the injector you can link types with implementations and instances
also build new instances recursively.

### Usage

Using the [Injection class](Runtime/Injection.cs) 

you can access to the methods Get, Create and Register.


### Examples

Registering a class


```csharp
class Foo
{
   ...
}


using Injector;
....

Injection.Register<Foo>();

```


Linking a class with an interface



```csharp
public interface IBar
{
    ...
}
public class Foo : IBar
{
   ...
}


using Injector;

...

Injection.Register<IBar,Foo>();

```

Linking an instance with a type

```csharp
public interface IBar
{
    ...
}
public class Foo : IBar
{
   ...
}


using Injector;

...

Foo instance;

...

Injection.Register<Foo>(instance);

```
Precondition: There must be a previous register 
of the specified type with the Register method.

Create an instance with Create method in injector. 
Create always return a new instance from the  specified type and doesnt store the new 
instance on injector's cache.
Create doesnt modify the injector's memory.

```csharp
public interface IBar
{
    ...
}


using Injector;

...

IBar instance;

...

instance = Injection.Create<IBar>();

```

Precondition: There must be a previous register
of the specified type with the Register method.

Get an instance from the injector of a specified type

Get retrieves an instance associated with the specified type. If there isnt any, 
creates a new one an returns it and then stores it in cache.

```csharp
public interface IBar
{
    ...
}


using Injector;

...

IBar instance;

...

instance = Injection.Get<IBar>();

```
