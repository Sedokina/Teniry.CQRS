# Teniry.CQRS

Simple and lightweight CQRS implementation for .NET

# ✨ Features:

- Command and query segregation
- Automatic command and query handlers registration
- Command and query dispatching
- Event dispatching and handling
- Transactional command handlers
- Includes FluentValidation support for commands
- Extended with types to simplify queries and commands implementation
- Built using only .NET Dependency Injection*
- Use any database**
- Production ready

(*) Teniry.CQRS' command and query dispatchers are fully implemented using Dependency Injection,
and do not use any libraries like MediatR.

(**) Teniry.CQRS can be used with any database access library, such as Entity Framework, Dapper, etc. But it is
recommended to use it with Entity Framework Core, as it provides built-in transactional command handlers support.

# How to use

Check out [quickstart guide](https://github.com/Sedokina/Teniry.CQRS?tab=readme-ov-file#-quick-start) and detailed
documentation on [github](https://github.com/Sedokina/Teniry.CQRS)