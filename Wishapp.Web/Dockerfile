FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props .

COPY Wishapp.Web/Wishapp.Web.csproj Wishapp.Web/

RUN dotnet restore Wishapp.Web/Wishapp.Web.csproj

COPY Wishapp.Web/ Wishapp.Web/

RUN dotnet publish Wishapp.Web/Wishapp.Web.csproj -c Release -o /app/publish

FROM build AS migrator-build
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --project Wishapp.Web/Wishapp.Web.csproj --no-build -o /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrator
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=migrator-build /app/efbundle .
COPY --from=build /app/publish .
COPY docker/migrate-and-seed.sh migrate-and-seed.sh
RUN chmod +x migrate-and-seed.sh
ENTRYPOINT ["/app/migrate-and-seed.sh"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Wishapp.Web.dll"]
