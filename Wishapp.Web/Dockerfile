FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props .

COPY Wishapp.Web/Wishapp.Web.csproj Wishapp.Web/

RUN dotnet restore Wishapp.Web/Wishapp.Web.csproj

COPY Wishapp.Web/ Wishapp.Web/

RUN dotnet publish Wishapp.Web/Wishapp.Web.csproj -c Release -o /app/publish
