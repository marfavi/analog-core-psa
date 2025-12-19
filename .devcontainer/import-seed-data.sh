#!/bin/bash
set -e

echo "Waiting for SQL Server to start..."
for i in {1..30}; do
    if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Your_password123 -C -Q "SELECT 1" &>/dev/null; then
        echo "SQL Server is ready!"
        break
    fi
    echo "Attempt $i/30: SQL Server not ready yet..."
    sleep 2
done

echo "Importing seed data..."
sqlpackage /Action:Import \
    /SourceFile:/tmp/SeedData.bacpac \
    /TargetServerName:localhost \
    /TargetDatabaseName:master2 \
    /TargetUser:sa \
    /TargetPassword:Your_password123 \
    /TargetTrustServerCertificate:True

echo "Seed data imported successfully!"
