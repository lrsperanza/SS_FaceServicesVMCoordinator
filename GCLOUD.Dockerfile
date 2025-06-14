FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY bin/Release/net6.0/publish app/

WORKDIR /app

EXPOSE 8080
EXPOSE 8081
EXPOSE 8082


# remember to publish the app first from Visual Studio
# to debug locally, run docker image with env  variable PORT=80 and map 80:80 or 44351:80
# docker run -d -p 80:80 --env PORT=80 uploadanddownloadgcloud
# or
# docker run -d -p 44351:80 --env PORT=80 uploadanddownloadgcloud
CMD dotnet SS_QueueTriggeredApp.dll

# command below to keep the container running for tests
#CMD sleep infinity