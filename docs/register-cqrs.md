# Register CQRS services

To use the library you have to register all necessary services.

The library provides an extension method `AddCqrs` that
registers all necessary services including command and query dispatchers. It also automatically registers all command,
query and event handlers in the assembly.

To register the services, add the following code to your `Program.cs` file:

```csharp
builder.Services.AddCqrs();
```

To register all handlers from the assembly in the DI container `Scrutor` library is used. The library scans the assembly
for all classes that implement `ICommandHandler`, `IQueryHandler` or `IApplicationEventHandler` interfaces and registers
them in the DI container.

> [!WARNING]
> By default the library registers handlers from the same assembly where the `AddCqrs` method is called.
> To change the assembly that has to be scanned for handlers, or to provide multiple assemblies call `AddCqrs` with
> assemblies list `AddCqrs(myAssembly1, myAssembly2, ...)`
