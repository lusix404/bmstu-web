FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY CoffeeShops/*.csproj CoffeeShops/
COPY CoffeeShops.Domain/*.csproj CoffeeShops.Domain/
COPY CoffeeShops.DataAccess/*.csproj CoffeeShops.DataAccess/
COPY CoffeeShops.DTOs/*.csproj CoffeeShops.DTOs/
COPY CoffeeShops.Services/*.csproj CoffeeShops.Services/
COPY CoffeeShops.Tests/*.csproj CoffeeShops.Tests/


RUN dotnet restore

COPY . .

WORKDIR /src/CoffeeShops
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeeShops.dll"]