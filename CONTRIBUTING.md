This section is a work in progress, for now I write down any 'special' steps that need to be taken to developer

# Specification testing
To run our specification tests, you first will have to bring up a test environment. We use docker for that.

1. Publish `TorrentGrease.Server`: `dotnet publish -c Release`
1. Start the docker containers, the docker compose file is in `SpecificationTest/Docker`: `docker-compose up -d --build`

To update/create feature files it's recommended that you install the SpecFlow extension