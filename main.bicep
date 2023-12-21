param name string = '625242'
param location string = resourceGroup().location

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-06-01' = {
  name: '${name}virtualnetwork'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: ['10.0.0.0/24']
    }
  }
}

resource blobStorage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${name}imagestorage'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'BlobStorage'
  properties: {
    accessTier: 'Hot'
  }
}

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: '${name}weatherfunctions'
  location: location
  kind: 'functionapp'
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
    }
  }
}
