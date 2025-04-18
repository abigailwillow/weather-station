param prefix string = '625242'
param location string = resourceGroup().location

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-06-01' = {
    name: '${prefix}-virtual-network'
    location: location
    properties: {
        addressSpace: {
            addressPrefixes: ['10.0.0.0/24']
        }
    }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
    name: '${prefix}storageaccount'
    location: location
    sku: {
        name: 'Standard_LRS'
    }
    kind: 'StorageV2'
    properties: {
        allowBlobPublicAccess: true
        accessTier: 'Hot'
    }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = {
    parent: storageAccount
    name: 'default'
}

resource jobSetupQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = {
    parent: queueService
    name: 'job-setup-queue'
}

resource imageGenerationQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = {
    parent: queueService
    name: 'image-generation-queue'
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
    parent: storageAccount
    name: 'default'
}

resource imageBlobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
    parent: blobService
    name: 'images'
    properties: {
        publicAccess: 'Blob'
    }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
    parent: storageAccount
    name: 'default'
}

resource jobsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
    parent: tableService
    name: 'jobs'
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
resource weatherAppFunctions 'Microsoft.Web/sites@2023-01-01' = {
    name: '${prefix}-weather-app-functions'
    location: location
    kind: 'functionapp'
    properties: {
        siteConfig: {
            appSettings: [
                {
                    name: 'FUNCTIONS_EXTENSION_VERSION'
                    value:  '~4'
                } 
                {
                    name: 'FUNCTIONS_WORKER_RUNTIME'
                    value: 'dotnet-isolated'
                }
                {
                    name: 'WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED'
                    value: '1'
                }
                {
                    name: 'WEBSITE_RUN_FROM_PACKAGE'
                    value: '1'
                }
                {
                    name: 'AzureWebJobsStorage'
                    value: storageAccountConnectionString
                }
            ]
        }
    }
}
