#!/bin/sh
set -e

echo "Running migrations..."
/app/efbundle --connection "$ConnectionStrings__Database"

echo "Seeding database..."
dotnet /app/Wishapp.Web.dll --seed
