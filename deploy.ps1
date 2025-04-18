$subscriptionId = "d3da86cb-0190-4251-ae11-ffc0e3fe8b52"
$resourceGroupName = "nieuwe_groep"
$location = "West Europe"
$appName = "weather-station"
$functionAppName = "625242-weather-app-functions"
$storageAccountName = "625242-image-storage"
$bicepFile = "main.bicep"

function Check-Failure {
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Deployment Failed"
        exit 1
    }
}

Write-Host "[1/5] Creating Resources"
az deployment group create --resource-group $resourceGroupName --template-file $bicepFile
Check-Failure

Write-Host "[2/5] Publishing"
dotnet publish -c Release -o "./publish"
Check-Failure

Write-Host "[3/5] Archiving"
Compress-Archive -Path ./publish/* -DestinationPath build.zip -Force
Check-Failure

Write-Host "[4/5] Deploying Function App"
az functionapp deployment source config-zip --name $functionAppName --resource-group $resourceGroupName --src build.zip
Check-Failure

Write-Host "[5/5] Cleaning Up Application Archive"
Remove-Item -Path build.zip -Force
Check-Failure

Write-Host "Deployment Successful"