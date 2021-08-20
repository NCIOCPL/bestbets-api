# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app
## This does not need to wait for the loader or other resources.
## Any integration tests will need to wait for the API to report being healthy
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=inttest
ENV ASPNETCORE_URLS="http://*:5000"
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENTRYPOINT ["dotnet", "watch", "--project", "src/NCI.OCPL.Api.BestBets", "run"]