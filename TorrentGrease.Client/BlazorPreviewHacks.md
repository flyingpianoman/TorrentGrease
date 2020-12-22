# Blazor hacks
Since blazor is still in preview, there are some hacks required to get everything to work (especially combined with vs docker support)
Since docker support handles release builds differently than debug builds they often have their own implementation of the hacks
FYI release builds follow the whole dockerfile, debug builds only use the base img of the dockerfile, copies a remote debugger, mounts the sources, builds them on your machine, starts the container and attaches to the remote debugger

# Getting gRPC client running in blazor
## HTTP2 for gRPC and HTTP1 for blazor & health endpoint
`Http1AndHttp2` as kestrel listener options isn't working. So we listen on 2 ports; an http1 listener for blazor & health endpoint and an http2 listener for grpc

## Use gRPC-web
JS and WASM don't support HTTP2 calls atm, to get around this the grpc-web protocal was defined: https://grpc.io/blog/state-of-grpc-web/
For ASPNet core we host a gRPC-web proxy (build by KnowitSolutions) and use a CallInvoker in our clients. See also this [example repo](https://github.com/Rora/aspnetcore-apis/tree/master/BlazorGrpcCodeFirst)
