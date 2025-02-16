# Register CQRS services

To use the library you have to register all necessary services.

The library provides an extension method `AddCqrs` that
registers all necessary services including command and query dispatchers. It also automatically registers all command
and query handlers in the assembly.

> [!WARNING]
> Currently the library does not support registering handlers from multiple assemblies.
> You have to have all handlers in the same assembly where the `AddCqrs` method is called.

To register the services, add the following code to your `Program.cs` file:

```csharp
builder.Services.AddCqrs();
```

To register all handlers from the assembly in the DI container `Scrutor` library is used. The library scans the assembly
for all classes that implement `ICommandHandler` or `IQueryHandler` interfaces and registers them in the DI container.

# Register event services

Events are optional and if your don't plan to use them, you don't have to register them.

Events are used to notify other parts of the application that something has happened.
The library provides an extension method `AddApplicationEvents` that
registers all necessary services for events to work. It also automatically registers all event handlers in the assembly.

> [!WARNING]
> Currently the library does not support registering event handlers from multiple assemblies.
> You have to have all event handlers in the same assembly where the `AddApplicationEvents` method is called.

To register event services, add the following code to your `Program.cs` file:

```csharp
builder.Services.AddApplicationEvents();
```

To register all handlers from the assembly in the DI container `Scrutor` library is used. The library scans the assembly
for all classes that implement `IApplicationEventHandler` interfaces and registers them in the DI container.
