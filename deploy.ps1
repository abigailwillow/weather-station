$resourceGroupName = "weather-app"
$location = "West Europe"
$appName = "weather-station"
$functionAppName = "625242weatherfunctions"
$storageAccountName = "625242imagestorage"
$bicepFile = "main.bicep"

Write-Host "[1/4] Archiving"
Compress-Archive -Path . -DestinationPath build.zip -Force

Write-Host "[2/4] Publishing"
dotnet publish -c Release -o "./publish"

Write-Host "[3/4] Creating"
az deployment group create --resource-group $resourceGroupName --template-file $bicepFile --parameters $parametersFile

Write-Host "[4/4] Deploying"
az functionapp deployment source config-zip --name $functionAppName --resource-group $resourceGroupName --src build.zip

Write-Host "Deployment Successful"
