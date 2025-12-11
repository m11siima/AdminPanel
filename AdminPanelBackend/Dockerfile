FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore AdminPanel.sln
WORKDIR /src/AdminPanel
RUN dotnet publish AdminPanel.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:44387
EXPOSE 44387
ENTRYPOINT ["dotnet", "AdminPanel.dll"]

