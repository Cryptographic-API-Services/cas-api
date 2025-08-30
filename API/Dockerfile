# Build on Ubuntu 24.04 with SDK 9.0 preinstalled
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG BUILD_CONFIGURATION=Release
ARG RID=linux-x64
WORKDIR /src

# copy csproj for restore caching
COPY ["API/API.csproj", "API/"]
COPY ["Libraries/Common/Common.csproj", "Libraries/Common/"]
COPY ["Libraries/DataLayer/DataLayer.csproj", "Libraries/DataLayer/"]
COPY ["Libraries/Models/Models.csproj", "Libraries/Models/"]
COPY ["Libraries/Payments/Payments.csproj", "Libraries/Payments/"]
COPY ["Libraries/Validation/Validation.csproj", "Libraries/Validation/"]

RUN dotnet restore API/API.csproj -r $RID
COPY . .
WORKDIR /src/API
RUN dotnet publish API.csproj -c $BUILD_CONFIGURATION -r $RID --no-self-contained -o /app/publish

# Runtime on Ubuntu 24.04 with ASP.NET 9.0 preinstalled
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "API.dll"]