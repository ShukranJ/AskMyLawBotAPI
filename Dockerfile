FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY WebApplication1.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish WebApplication1.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
