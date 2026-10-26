FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props global.json NuGet.Config ObeAunQa.slnx ./
COPY apps/api/ObeAunQa.Api.csproj apps/api/
COPY shared/backend/ObeAunQa.SharedKernel.csproj shared/backend/
COPY modules/common/curriculum/backend/ObeAunQa.Modules.Curriculum.csproj modules/common/curriculum/backend/
COPY modules/common/identity/backend/ObeAunQa.Modules.Identity.csproj modules/common/identity/backend/
COPY modules/sv4/accreditation/backend/ObeAunQa.Modules.Accreditation.csproj modules/sv4/accreditation/backend/
COPY modules/sv5/reporting/backend/ObeAunQa.Modules.Reporting.csproj modules/sv5/reporting/backend/

RUN dotnet restore apps/api/ObeAunQa.Api.csproj

COPY apps/api/ apps/api/
COPY shared/backend/ shared/backend/
COPY modules/common/curriculum/backend/ modules/common/curriculum/backend/
COPY modules/common/identity/backend/ modules/common/identity/backend/
COPY modules/sv4/accreditation/backend/ modules/sv4/accreditation/backend/
COPY modules/sv5/reporting/backend/ modules/sv5/reporting/backend/

RUN dotnet publish apps/api/ObeAunQa.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=6 \
    CMD curl --fail --silent http://127.0.0.1:8080/health/ready > /dev/null || exit 1

ENTRYPOINT ["dotnet", "ObeAunQa.Api.dll"]
