#!/bin/bash
set -e

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &
SQLSERVER_PID=$!

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
for i in {1..60}; do
    if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -C -Q "SELECT 1" &>/dev/null; then
        echo "SQL Server is ready!"
        break
    fi
    if [ $i -eq 60 ]; then
        echo "SQL Server failed to start in time"
        exit 1
    fi
    echo "Attempt $i/60: SQL Server not ready yet..."
    sleep 2
done

# Import seed data if the file exists
if [ -f /tmp/SeedData.bacpac ]; then
    echo "Checking if database master2 exists..."
    DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -C -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'master2'" -h -1 | tr -d '[:space:]')
    
    if [ "$DB_EXISTS" != "0" ]; then
        echo "Database master2 exists, dropping it..."
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -C -Q "ALTER DATABASE master2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE master2;"
        echo "Database dropped successfully!"
    fi

    echo "Importing seed data..."
    sqlpackage /Action:Import \
        /SourceFile:/tmp/SeedData.bacpac \
        /TargetServerName:localhost \
        /TargetDatabaseName:master2 \
        /TargetUser:sa \
        /TargetPassword:"${SA_PASSWORD}" \
        /TargetTrustServerCertificate:True

    echo "Seed data imported successfully!"
fi

# Keep SQL Server running in foreground
wait $SQLSERVER_PID
