FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Project.slnx ./
COPY src/Sporim.Api/Sporim.Api.csproj src/Sporim.Api/
COPY tests/Sporim.Api.Tests/Sporim.Api.Tests.csproj tests/Sporim.Api.Tests/
RUN dotnet restore Project.slnx

COPY . ./
RUN dotnet publish src/Sporim.Api/Sporim.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sporim.Api.dll"]
