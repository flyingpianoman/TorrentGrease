# Blazor hacks
Since blazor is still in preview, there are some hacks required to get everything to work (especially combined with vs docker support)
Since docker support handles release builds differently than debug builds they often have their own implementation of the hacks
FYI release builds follow the whole dockerfile, debug builds only use the base img of the dockerfile, copies a remote debugger, mounts the sources, builds them on your machine, starts the container and attaches to the remote debugger

## Issue 'Debug not working since the client project isn't mounted'
See also https://github.com/aspnet/AspNetCore/issues/9704#issuecomment-544272050 
### Debug fix
Mount client proj by adding a 'docker-compose.vs.debug.yml' to the docker-compose project and adding a volume mount to /Client

Since aspnetcore won't know to look at '/TorrentGrease.Client', we need to inform it. This is configured in the `BlazorWasmOnDocker.Client.blazor.config` file. It's currently configured to work on the host machine (the windows path to this repo). We'll replace the path so that it'll work for the container setup.
We did so by adding the following code to the `Program.cs` of the host project:

```
#if DEBUG
    const string blazorConfigPath = @"/app/bin/Debug/netcoreapp3.1/TorrentGrease.Client.blazor.config";
    var blazorConfig = File.ReadAllText(blazorConfigPath);
    blazorConfig = Regex.Replace(blazorConfig, @"[a-zA-Z]:[\/\\].+?[\/\\]TorrentGrease.Client[\/\\]", "/Client/")
        .Replace('\\', '/');
    File.WriteAllText(blazorConfigPath, blazorConfig);
#endif
```
# Getting gRPC client running in blazor
## HTTP2 for gRPC and HTTP1 for blazor & health endpoint
`Http1AndHttp2` as kestrel listener options isn't working. So we listen on 2 ports; an http1 listener for blazor & health enpoint and an http2 listener for grpc

## Use gRPC-web
JS and WASM doesn't support HTTP2 calls atm, to get around this the grpc-web protocal was defined: https://grpc.io/blog/state-of-grpc-web/
For ASPNet core we host a gRPC-web proxy (build by KnowitSolutions) and use a CallInvoker in our clients. See also this [example repo](https://github.com/Rora/aspnetcore-apis/tree/master/BlazorGrpcCodeFirst)
