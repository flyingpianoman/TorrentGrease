# Blazor hacks
Since blazor is still in preview, there are some hacks required to get everything to work (especially combined with vs docker support)
Since docker support handles release builds differently than debug builds they often have their own implementation of the hacks
FYI release builds follow the whole dockerfile, debug builds only use the base img of the dockerfile, copies a remote debugger, mounts the sources, builds them on your machine, starts the container and attaches to the remote debugger

## Issue 'Debug not working since the client project isn't mounted'
### Debug fix
Mount client proj by adding a 'docker-compose.vs.debug.yml' to the docker-compose project and adding a volume mount to /TorrentGrease.Client

Since aspnetcore won't know to look at '/TorrentGrease.Client', we need to inform it. To do so I've added an env var named 'CLIENT_APP_ASSEMBLY_PATH_OVERRIDE' to 'docker-compose.vs.debug.yml' and added the following lines to 'Startup.cs'
```
  var clientAssemblyPathOverride = _config.GetValue<string>("CLIENT_APP_ASSEMBLY_PATH_OVERRIDE");
  if (!string.IsNullOrEmpty(clientAssemblyPathOverride))
  {
      app.UseClientSideBlazorFiles(clientAssemblyPathOverride);
  }
  else
  {
      app.UseClientSideBlazorFiles<Client.Startup>();
  }
```
### Release fix
Not needed, release works fine

## Issue https://github.com/aspnet/AspNetCore/issues/9704
### Debug fix
Added the following lines to TorrentGrease.Client.csproj
```
  <Target Name="FixBlazorConfigForDockerDebug" AfterTargets="GenerateBlazorMetadataFile" Condition="'$(Configuration)' == 'Debug'">
    <WriteLinesToFile File="$(BlazorMetadataFilePath)" Lines="/TorrentGrease.Client/TorrentGrease.Client.csproj" Overwrite="true" Encoding="Unicode" />
    <WriteLinesToFile File="$(BlazorMetadataFilePath)" Lines="/TorrentGrease.Client/bin/Debug/$(TargetFramework)/$(AssemblyName).dll" Overwrite="false" Encoding="Unicode" />
    <WriteLinesToFile File="$(BlazorMetadataFilePath)" Condition="'$(BlazorRebuildOnFileChange)'=='true'" Lines="autorebuild:true" Overwrite="false" Encoding="Unicode" />
    <WriteLinesToFile File="$(BlazorMetadataFilePath)" Condition="'$(BlazorEnableDebugging)'=='true'" Lines="debug:true" Overwrite="false" Encoding="Unicode" />
  </Target>
```

### Release fix
Added the following lines to the dockerfile in stage 'build'
```
  RUN rm -f /app/TorrentGrease.Client.blazor.config
  RUN echo "/app\n/app/TorrentGrease.Client.dll" > /app/TorrentGrease.Client.blazor.config
```

## Issue https://github.com/aspnet/Blazor/issues/376
### Debug fix
Renamed wwwwroot to dist_wwwwroot since it would otherwise be found by aspnetcore which we don't want
At startup we copy the content of dist_wwwroot to the dist dir located in the bin dir

See the following function & call in program.cs
```
  #if DEBUG
    MoveWwwrootToDistDir();
  #endif
```
### Release fix
Added the following line to the dockerfile, this copies the dist_wwwroot to the dist dir
``RUN cp -rf /src/TorrentGrease.Client/dist_wwwroot/* /app/dist``

# Getting GRPC client running in blazor
## Issue https://github.com/aspnet/AspNetCore/issues/5492 & https://github.com/mono/mono/issues/12604 Linker error 'Mono.Cecil.ResolutionException: Failed to resolve XXXXX'
Added `<BlazorLinkOnBuild>false</BlazorLinkOnBuild>` to the blazor csproj, possible workaround would be:
1. Publish client with linker disabled 
1. Build/publish again with linker enabled and using the previous output as `--search-path` for the second build/publish

## TypeLoadException on `System.ServiceModel.Primitives` at runtime
Added `<PackageReference Include="System.ServiceModel.Primitives" Version="4.6.0" />` to the client csproj

## HTTP2 for grpc and HTTP1 for blazor & health endpoint
`Http1AndHttp2` as kestrel listener options isn't working. So we listen on 2 ports; an http1 listener for blazor & health enpoint and an http2 listener for grpc

## Sadly, we can't call GRPc enpoints from the browser yet see:
https://github.com/grpc/grpc-dotnet/issues/99#issuecomment-491447512
https://grpc.io/blog/state-of-grpc-web/
'It is currently impossible to implement the HTTP/2 gRPC spec3 in the browser, as there is simply no browser API with enough fine-grained control over the requests.'

Only way to do this atm is to use a hosted proxy which listens to http1 and calls the http2 api