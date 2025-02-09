How to publish package:
Change package version in `.csproj` file, then run
```
    dotnet pack --configuration Release
    
    // replace 1.0.0 to your version
    dotnet nuget push "bin/Release/Teniry.Cqrs.1.0.0.nupkg" --source "github"
```

More information:
https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry


To hide your username and token, move whole section of `nuget.config`
```
<packageSourceCredentials>
    <github>
        <add key="Username" value="you_username" />
        <add key="ClearTextPassword" value="your_application_token" />
    </github>
</packageSourceCredentials>
```
to `.nuget/NuGet/NuGet.Config` to your user directory on your PC,
and put your password and token instead of placeholders

More information:
https://learn.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior#config-file-locations-and-uses
