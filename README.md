# Video Indexer to Azure Cognitive Search
Created by Michael S. Scherotter, Kelsey Huebner and Marcel Aldecoa

This code parses [Azure Video Indexer](https://www.videoindexer.ai/) insights data into scene and thumbnail documents, storing in [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/) or local storage. The documents are converted to [Azure Cognitive Search](https://azure.microsoft.com/en-us/services/cognitive-services/) scene and thumbnail indices.

## Search Components
### Scene Index
The **sceneindex** lets you create queries for scenes that have certain metadata, including transcript, faces, emotions, sentiment, labels, and audio effects.  There should be one document for each scene in each video.

### Query Syntax
- Search for scenes that have a specific person: _$count=true&$filter=faces/any(face: face/name eq 'John Doe')&$select=start,end_
- Search for scenes that have a refrigerator in them: _$count=true&$filter=labels/any(name: label/name eq 'refrigerator')&$select=start,end_

### Thumbnail Index
The **thumbnailindex** lets you create queries for keyframe thumbnails that have been extracted by video indexer that have certain metadata including faces, labels, OCR, keywords, and shot tags.  There should be one thumbnail document for each keyframe in each video.

## The code
The Console app (Program.cs) is the main entry point. It will read the configuration section of appsettings.json and inject an instance of Services

### Models
Services contain the app logic for:
- Azure Blob Reader
- Azure Blob Writer
- Azure Cognitive Search
- Azure Video Indexer
- FileStream Reader
- FileStream Writer

### Services
Services contain the app logic for:
- Azure Blob Reader
- Azure Blob Writer
- Azure Cognitive Search
- Azure Video Indexer
- FileStream Reader
- FileStream Writer

### Factories
Create as factory -- for example: read/write will instantiate a class based on the type (blob or filestream)

## Configuring appsettings.json

### reader

Set up your credentials to read from Azure Blob Storage or local storage using FileStream. 
If using blob storage, add your [connection string](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=azure-portal). 

```json
"connectionString": "{connection string}"
```

If using FileStream, add your local folder path.

```json
"insightsPath": "{local file path}",
"failedInsightsPath": "{local file path}"
```

### write

Set up your credentials to read from Azure Blob Storage or local storage using FileStream. 
If using blob storage, add your [connection string](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=azure-portal). 

```json
"connectionString": "{connection string}"
```

If using FileStream, add your local folder path.

```json
"scenesPath": "{local file path}",
"thumbnailsPath": "{local file path}"
```


### azureSearch
Set up your credentials to connect to your [Azure Cognitive Search](https://azure.microsoft.com/en-us/services/cognitive-services/) service.

```json
"name": "{search service name}",
"adminKey": "{admin key}",
"deleteIndexIfExists": false
```
> optionally ```deleteIndexIfExists```can be ```true``` if want to recreate the index each time the code runs

### videoIndexer

Set up your credentials to connect to your Azure Video Indexer service. To find the keys, follow guidelines here: https://docs.microsoft.com/en-us/azure/media-services/video-indexer/video-indexer-use-apis

```json
    "location": "trial", 
    "accountId": "{account id}",
    "subscriptionKey": "{subscription key}",
    "accessToken": "{accessToken}",
    "pageSize": 25,
    "downloadInsights": true,
    "downloadThumbnails": true    
```
> If the ```accessToken``` is not provided the application will try to create a read-only token using the ```subscriptionKey```.

If a paid account will be used then replace   ```location``` with the region for the account.
 
Optionally ```pageSize```can be changed depending on the amount of videos indexed.

The flags ```downloadInsights``` and ```downloadThumbnails``` control whether the application will download insights files and thumbnails from [Azure Video Indexer](https://www.videoindexer.ai/)

## Running the App
- Clone the repository
- Set up credentials in the ```appsettings.json``` file
- Run

```
cd src/
dotnet build -v q
dotnet run
```

## Logger Config
The configuration for logging can be found in the found ```log4net.config```
