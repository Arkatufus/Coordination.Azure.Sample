dotnet publish -c Release
docker build -t hocon.configuration:latest .
docker-compose up --scale cluster=10