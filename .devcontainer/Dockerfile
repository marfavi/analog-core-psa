FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Set environment variables
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Your_password123

# Install sqlpackage - download, extract to temp, then move
RUN apt-get update && \
    apt-get install -y wget unzip libicu70 && \
    mkdir -p /tmp/sqlpackage && \
    wget -q https://aka.ms/sqlpackage-linux -O /tmp/sqlpackage.zip && \
    unzip -q /tmp/sqlpackage.zip -d /tmp/sqlpackage && \
    mv /tmp/sqlpackage /opt/sqlpackage && \
    chmod +x /opt/sqlpackage/sqlpackage && \
    ln -s /opt/sqlpackage/sqlpackage /usr/local/bin/sqlpackage && \
    rm -rf /tmp/sqlpackage.zip /var/lib/apt/lists/*

# Copy seed data and entrypoint script
COPY ./SeedData.bacpac /tmp/SeedData.bacpac
COPY ./docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

EXPOSE 1433

# Start SQL Server via entrypoint
ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
