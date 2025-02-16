# Register CQRS services

To use the library you have to register all necessary services.

The library provides an extension method `AddCqrs` that
registers all necessary services including command and query dispatchers. It also automatically registers all command,
query and event handlers in the assembly.

> [!WARNING]
> Currently the library does not support registering handlers from multiple assemblies.
> You have to have all handlers in the same assembly where the `AddCqrs` method is called.

To register the services, add the following code to your `Program.cs` file:

```csharp
builder.Services.AddCqrs();
```

To register all handlers from the assembly in the DI container `Scrutor` library is used. The library scans the assembly
for all classes that implement `ICommandHandler`, `IQueryHandler` or `IApplicationEventHandler` interfaces and registers
them in the DI container.