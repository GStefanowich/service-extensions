# Service Extensions

This library is a simple extension of ServiceProviders and Dependency Injection

The main additional method is a common extension method that I use which replaces

```csharp
ActivatorUtilities.CreateInstance<T>(services, parameters)
```
with a simpler extension method call
```csharp
services.Create<T>();
```