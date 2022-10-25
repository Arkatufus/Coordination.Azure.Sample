dotnet publish -c Release
docker build -t akka.hosting.configuration:latest .
docker-compose up --scale cluster=10