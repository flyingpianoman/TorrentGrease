This section is a work in progress, for now I write down any 'special' steps that need to be taken to develop

# docker-compose debug
If you're getting the error 'A debug operation has started during container preparation' while trying to debug the docker-compose project, please see
https://developercommunity.visualstudio.com/t/debugging-docker-compose-a-debug-operation-has-sta/1550044.

# Specification testing
To run our specification tests, you first will have to bring up a test environment. We use docker for that.

1. Publish `TorrentGrease.Server`: `dotnet publish -c Release`
1. Start the docker containers, the docker compose file is in `docker-compose`: `docker-compose up -d --build`

To update/create feature files it's recommended that you install the SpecFlow extension