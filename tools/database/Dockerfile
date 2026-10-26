FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json NuGet.Config ./
COPY shared/backend/ shared/backend/
COPY modules/common/curriculum/backend/ modules/common/curriculum/backend/
COPY modules/common/identity/backend/ modules/common/identity/backend/
COPY modules/sv4/accreditation/backend/ modules/sv4/accreditation/backend/
COPY modules/sv5/reporting/backend/ modules/sv5/reporting/backend/
COPY tools/database/ tools/database/
RUN dotnet publish tools/database/ObeAunQa.Database.csproj --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "ObeAunQa.Database.dll"]
CMD ["initialize"]
