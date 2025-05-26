# Database/Dockerfile
FROM postgres:latest

# Setze Umgebungsvariablen für PostgreSQL
ENV POSTGRES_USER=webapp
ENV POSTGRES_PASSWORD=webapp123
ENV POSTGRES_DB=webappdb

# Kopiere das Initialisierungs-Skript
COPY ./init-sqls /docker-entrypoint-initdb.d/