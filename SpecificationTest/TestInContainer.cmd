docker build -t torrent-grease-containerized-spec-test -f .\Dockerfile .. 
docker run --network="host" -v /var/run/docker.sock:/var/run/docker.sock torrent-grease-containerized-spec-test %* 
rem e.g. --filter FullyQualifiedName~RelocateTorrentDataWithMultiDirTorrents