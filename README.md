# VipaPDFMerge — ASP.NET Core Web API (.NET 10)

Šis projektas skirtas jau sukurtam Azure **Web App** `VipaPDFMerge`.

Tai nėra Azure Functions projektas.

## Endpointai

### Health

GET:

`/health`

### API info

GET:

`/`

### Swagger

GET:

`/swagger`

### PDF merge

POST:

`/api/merge-pdf`

Body:

```json
{
  "OneDriveUrl": "https://zzz-my.sharepoint.com/personal/pn_admin_vipa_lt/",
  "FolderPath": "https://zzz-my.sharepoint.com/personal/pn_admin_vipa_lt/Documents/MAS rastai/DNMF1/14730"
}
```

Response:

```json
{
  "result": {
    "Message": "",
    "ErrorMessage": "",
    "InnerErrorMessage": "",
    "IsSuccessful": true,
    "MergedPDFFilePath": "https://zzz-my.sharepoint.com/personal/pn_admin_vipa_lt/Documents/MAS rastai/DNMF1/14730/Merged/merged_pdf_2026-09-16-15-06-48-642.pdf"
  }
}
```

## Azure Web App

Tavo URL:

`https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net`

Po deploy pirmiausia tikrink:

`https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net/health`

Po to Swagger:

`https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net/swagger`

PDF endpoint:

`https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net/api/merge-pdf`

## Managed Identity

Azure Portal:

VipaPDFMerge -> Settings -> Identity -> System assigned -> On -> Save.

Kodas naudoja `DefaultAzureCredential`, todėl Azure Web App'e bus naudojama
System Assigned Managed Identity. Client Secret nereikalingas.

Managed Identity dar turi būti suteikta Microsoft Graph prieiga.

Rekomenduojamas variantas:

- Microsoft Graph application permission: `Sites.Selected`
- konkrečiam OneDrive personal site: `write`

Setup kataloge yra pagalbiniai PowerShell skriptai.

## Optional API key

Azure Web App:

Settings -> Environment variables

Pridėk:

`ApiKey = tavo-slaptas-raktas`

Tada request header:

`x-api-key: tavo-slaptas-raktas`

Jei `ApiKey` tuščias arba neegzistuoja, papildoma API key patikra išjungta.

## Local test

```powershell
az login
dotnet restore
dotnet run
```

Tada:

`http://localhost:5080/swagger`

## Publish

Geriausia iš projekto katalogo:

```powershell
dotnet publish -c Release -o .\publish
```

Į Azure Web App turi būti deployintas `publish` katalogo turinys, ne pats source ZIP.

Jei naudoji Visual Studio / VS Code Azure App Service deploy, įrankis gali atlikti
build/publish automatiškai.
