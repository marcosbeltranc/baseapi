FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todo el proyecto incluyendo migraciones
COPY . .

# Restaurar paquetes
RUN dotnet restore "./BaseApi.csproj"

# Build del proyecto
RUN dotnet build "./BaseApi.csproj" -c Release -o /app/build

# Publicar
RUN dotnet publish "./BaseApi.csproj" -c Release -o /app/publish

# Instalar dotnet-ef para aplicar migraciones
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copiar las migraciones desde la etapa de build
COPY --from=build /src/Data/Migrations ./Data/Migrations/

# Script para aplicar migraciones al iniciar
RUN echo '#!/bin/bash\n\
dotnet ef database update\n\
dotnet BaseApi.dll\n\
' > /app/entrypoint.sh && \
chmod +x /app/entrypoint.sh

ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]