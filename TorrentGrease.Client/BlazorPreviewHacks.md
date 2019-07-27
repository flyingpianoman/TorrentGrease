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
    <WriteLinesToFile File="$(BlazorMetadataFilePath)" Lines="/TorrentGrease.Client/bin/Debug/netstandard2.0/$(AssemblyName).dll" Overwrite="false" Encoding="Unicode" />
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

