# Helpdesk en Blazor Web App y .NET

## Definición y estructura del proyecto

## 1. Qué es un Helpdesk

Un **Helpdesk** es una aplicación donde usuarios autenticados registran incidencias, peticiones o dudas, y un equipo de soporte las gestiona hasta resolverlas.

## Configuracion segura (desarrollo)

No guardes secretos en `appsettings*.json`. Este proyecto usa User Secrets en `HelpDesk.Web`.

Configura valores locales con:

```powershell
dotnet user-secrets --project "HelpDesk.Web/HelpDesk.Web.csproj" set "SeedData:Admin:Enabled" "true"
dotnet user-secrets --project "HelpDesk.Web/HelpDesk.Web.csproj" set "SeedData:Admin:Email" "admin@helpdesk.local"
dotnet user-secrets --project "HelpDesk.Web/HelpDesk.Web.csproj" set "SeedData:Admin:Password" "TU_PASSWORD"
dotnet user-secrets --project "HelpDesk.Web/HelpDesk.Web.csproj" set "AzureBlobStorage:ConnectionString" "TU_AZURE_BLOB_CONNECTION_STRING"
```

Tambien tienes una plantilla en `HelpDesk.Web/appsettings.Example.json` para referencia de claves requeridas.
