#!/bin/sh
set -e

echo "Running migrations..."
/app/efbundle

echo "Seeding database..."
dotnet /app/Wishapp.Web.dll --seed
