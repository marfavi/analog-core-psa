FROM --platform=linux/amd64 mcr.microsoft.com/mssql/server:2022-CU22-ubuntu-22.04

# 2. Set environment variables
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Your_password123

# 3. Switch to root to install dependencies
USER root

# 4. Install sqlpackage dependencies and download sqlpackage
RUN apt-get update && apt-get install -y unzip wget libunwind8 curl && \
    wget -O sqlpackage.zip https://aka.ms/sqlpackage-linux && \
    unzip sqlpackage.zip -d /opt/sqlpackage && \
    chmod +x /opt/sqlpackage/sqlpackage && \
    rm sqlpackage.zip

# 5. Copy your BACPAC file into the container
COPY sqldb-aio-core-dev-2025-11-26-11-25.bacpac /var/opt/mssql/backups/MyDatabase.bacpac

# 6. Start SQL Server and import BACPAC on startup
CMD /opt/mssql/bin/sqlservr & \
    echo "Waiting for SQL Server to start..." && sleep 20 && \
    /opt/sqlpackage/sqlpackage \
    /a:Import \
    /sf:/var/opt/mssql/backups/MyDatabase.bacpac \
    /tsn:localhost \
    /tdn:master2 \
    /tu:sa \
    /tp:$SA_PASSWORD \
    /TargetEncryptConnection:False
