#Architecture decision record
In this document we'll record all architecture decisions and the rationale behind them. Decisions are sorted by date (oldest a lowest).


## Testing strategy: UnitTests, IntegrationTests, SpecificationTests
TorrentGrease wil be covered by the following types of tests:
1. UnitTests: test code in isolation by mocking dependencies, will be used to test complex code.
1. IntegrationTests: test parts of the system to see if parts work well together. These tests run against a hosted environment
1. SpecificationTests: tests that specifications are implemented using SpecFlow. These tests will be written using specification by example and will run against a hosted environment.

Most of TorrentGrease will be simple crud calls. These simple calls will be covered by specification tests for the following reasons:
1. Since we're implementing our requirements using specification by example, this means that almost all CRUD calls need to be covered by 1 or more spec tests anyways.
1. Unit tests have to mock dependencies, tying them to the implementation of the code, well written spec tests approach the application as a black box and are less likely to break (e.g. refactoring the implementation of a call will break most unittests but shouldn't bread a spec test).
1. Unit tests test code in isolation, requiring multiple unittests for each function/layer in the CRUD call, specification tests test functionality in isolation; one spec test will cover multiple functions/layers.
1. Where unit tests prove that seperate parts of a system work as intended, they do so by mocking dependencies. The downside is that most unit test suites don't validate that dependencies are configured and used correctly. (e.g. mocking a db contect causes linq queries that will pass all unit tests but cause issues when used vs a real database)
1. I don't believe that we should test the software that we're using (e.g. entity framework and sqlite). That being said I do believe that we should test the way that we're using the software, incorrect usage will cause unforseen problems. Therefore things like generated EF queries should be tested instead of only testing with mocked DbContext or an in memory database.

IMO the most maintainable and simple setup for testing crud calls is by writing a specification test for it. I've met a lot of developers who would strongly disagree with me after having experienced horrible specification test suites.
I've seen my share of horror specification/integration test setups. But I've also worked on projects where specification tests were seen as a relief.
To move TorrentGrease to the latter the following principles should be followed:
1. Each test should be independent of ther tests, clean state before each test
1. TorrentGrease must be healthy when running tests against it
1. We won't tie test code to CSS markup, we'll be selecting elements by using `data-content` attributes that describe the purpose of a piece of HTML.
1. Most issues with specification test suites stem from tests that sometimes fail, the majority of the failures are cause by race conditions (e.g. the application is still loading but the test already starts clicking/reading). To prevent this we synchronise our tests and app. Never assume that X will be done by the time the next test step is ran, verify it. We don't wait on asynchronous operations (e.g. page load, click handle etc.) by using Thread.Sleep or Task.Delay, instead we poll for dom changes using polly.
1. For some reason test code is sometimes seen as a second class citizen, which is strange since it contributes to a major part of most codebases. Test code will be of the same quality as production code, no copying test X and tweaking it; refactor and reuse code.

## Specification by example
To ensure up to date documentation and prevent regression bugs all feature of TorrentGrease will be documented and verified by applying specification by example.

## Torrent client abstraction
TorrentGrease will be set up in a torrent client agnostic way. TorrentGrease will be developed against Transmission (grease for the transmission, get it ;) ), since that's the client I use.
TorrentGrease will use be written against a TorrentClient interface that can be implemented so serve other clients.

To prevent challenges when implemnting support for new torrent clients, we'll try to limit the type of calls we'll use to basic CRUD calls and don't use specialized TorrentClient features.

## gRPC
Most Asp.net core apps host REST apis. Building true RESTfull services takes a lot of work and well thought out design. 
Many hours have been burned discussing how to expose functionality in a true RESTfull manner. I can appreciate a RESTfull api when it's exposed to other teams/customers, however I believe that RESTfull loses most of it's value when you own the API and the consumer app.
Therefore TorrentGrease will host gRPC services. gRPC services are more performant and simpler to design than RESTfull services.

### gRPC code first
The Asp.net core team has implemented support for building contract (proto) first gRPC apis. This is a great setup for a platform independent scenario's.
TorrentGrease will have a C# server and C# client, therefore I would prefer simplicity over platform independence. 

Marc Gravell has developed a gRPC code first stack that integrates neatly with Asp.net core.
gRPC code first allows us to use C# to declare our gRPC contract (interfaces) and schemas (pocos). This saves us time learning the proto language.

## SQLite & EF core
TorrentGrease data will be stored using a SQLite db. The main reasoning behind this is to keep deployment simple for our users.
We'll also be using EF core & migrations, this means that swapping to another persistance provider will be doable.

We will let EF core generate our queries and write our own when needed (premature optimization is the root of all evil).

## Containerized app
TorrentGrease will be developed and tested for use in a linux container. Containerized apps are very easy to deploy and prevent bugs that might surface when using many different environments. 
Containerized apps also simplify deployment for testing.

## Blazor WASM as UI framework
TorrentGrease will use blazor WASM as a UI framework, the rationale behind this is that I've used other ui frameworks before (e.g. angular) and would love to try blazor.
I've chosen Blazor WASM instead of Blazor serverside because I don't believe that the serverside model will stay relevant for web applications, the connection delay for each event will kill the user experience IMO.

## Asp.net core
TorrentGrease will be built using Asp.net core and C# for the simple reason that I'm a microsoft dev and love this stack.