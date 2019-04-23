FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["DistributedTesting/DistributedTesting.csproj", "DistributedTesting/"]
COPY ["DistributedTesting.Common/DistributedTesting.Common.csproj", "DistributedTesting.Common/"]
RUN dotnet restore "DistributedTesting/DistributedTesting.csproj"
COPY . .
WORKDIR "/src/DistributedTesting"
RUN dotnet build "DistributedTesting.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DistributedTesting.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DistributedTesting.dll"]
