# VipaPDFMerge deploy per GitHub Actions

Šis variantas pats GitHub serveryje:

1. įdiegia .NET 10 SDK;
2. `dotnet restore`;
3. `dotnet build -c Release`;
4. `dotnet publish -c Release`;
5. patikrina, ar atsirado `web.config` ir `VipaPDFMerge.dll`;
6. deployina tik `publish` rezultatą į Azure Web App `VipaPDFMerge`.

## 1. Sukurk GitHub repository

GitHub sukurk naują repository, pvz.:

`VipaPDFMerge`

Įkelk VISĄ šio ZIP turinį į repository root.

Repository root turi atrodyti:

```text
.github/
  workflows/
    azure-webapp.yml
Controllers/
Models/
Services/
Setup/
Properties/
VipaPDFMerge.csproj
Program.cs
appsettings.json
README.md
GITHUB-DEPLOY.md
```

## 2. Azure Publish Profile

Azure Portal:

`VipaPDFMerge -> Overview -> Get publish profile`

Atsisiųsk publish profile XML.

Jeigu Azure rodo, kad basic authentication publishing credentials yra išjungti,
juos reikia įjungti Web App deployment nustatymuose arba vietoje publish profile
naudoti OIDC deployment.

## 3. GitHub Secret

GitHub repository:

`Settings -> Secrets and variables -> Actions -> New repository secret`

Name:

`AZURE_WEBAPP_PUBLISH_PROFILE`

Value:

įklijuok VISĄ Azure atsisiųsto publish profile XML turinį.

## 4. Paleidimas

Įkelk projektą į `main` branch.

Workflow pasileis automatiškai.

Arba:

`GitHub -> Actions -> Build and deploy VipaPDFMerge to Azure Web App -> Run workflow`

## 5. Patikrink GitHub Actions

Turi būti žali abu jobs:

- `build`
- `deploy`

Build loge turi matytis publish failai, tarp jų:

- `VipaPDFMerge.dll`
- `VipaPDFMerge.exe`
- `VipaPDFMerge.runtimeconfig.json`
- `web.config`

## 6. Azure testas

Po deploy:

`https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net/health`

Turi grįžti:

```json
{
  "status": "Healthy",
  "utc": "..."
}
```

Swagger:

`https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net/swagger`

API:

`POST https://vipapdfmerge-c3ahfjhza5aebmhf.westus3-01.azurewebsites.net/api/merge-pdf`

## 7. Managed Identity

Po to Azure Portal:

`VipaPDFMerge -> Settings -> Identity -> System assigned -> On`

Managed Identity turi turėti Microsoft Graph `Sites.Selected` application role ir
`write` permission konkrečiam OneDrive personal site.

Tai atskiras žingsnis nuo Web App deployment. `/health` turi veikti net ir dar
nesutvarkius Graph teisių.
