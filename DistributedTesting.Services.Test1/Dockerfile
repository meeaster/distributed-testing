FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["DistributedTesting.Services.Test1/DistributedTesting.Services.Test1.csproj", "DistributedTesting.Services.Test1/"]
COPY ["DistributedTesting.Common/DistributedTesting.Common.csproj", "DistributedTesting.Common/"]
RUN dotnet restore "DistributedTesting.Services.Test1/DistributedTesting.Services.Test1.csproj"
COPY . .
WORKDIR "/src/DistributedTesting.Services.Test1"
RUN dotnet build "DistributedTesting.Services.Test1.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DistributedTesting.Services.Test1.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DistributedTesting.Services.Test1.dll"]