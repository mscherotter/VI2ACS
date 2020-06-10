# Video Indexer to Azure Cognitive Search
Created by Michael S. Scherotter, Kelsey Huebner, and Marcel Aldecoa

This code parses [Azure Video Indexer](https://www.videoindexer.ai/) insights data into scene and thumbnail documents, storing in [Azure Blob Storage](https://azure.microsoft.com/en-us/services/storage/blobs/) or local storage. The documents are converted to [Azure Cognitive Search](https://azure.microsoft.com/en-us/services/cognitive-services/) scene and thumbnail indices. It also allows the download of thumbnail files.  

## Search Components

### Scene Index
The **sceneindex** allows the creation of queries for scenes that have certain metadata, including transcript, faces, emotions, sentiment, labels, and audio effects.  There should be one document for each scene in each video.  The Video Indexer insights are pivoted on the scene entities to make the search documents. This index is ready to add additional insights from custom vision models to the Scene/Shots/Keyframes/KeyFrame.CustomVisionModels collection.

### Thumbnail Index
The **thumbnailindex** allows the creation ofqueries for keyframe thumbnails that have been extracted by video indexer that have certain metadata including faces, labels, OCR, keywords, and shot tags.  There should be one thumbnail document for each keyframe in each video.  The Video Indexer insights are pivoted on the keyframe thumbnail entities to make the search documents.  This index is ready to add additional insights from custom vision models to the Thumbnail.CustomVisionModels collection.

### Azure Search Query Examples
Various fields are facetable, filterable, and searchable.  Look at the model classes for the IsFilterable, IsSearchable, and IsFacetable attributes on properties. 

#### Scene Index
- Search for scenes that have a specific person: ```$count=true&$filter=Faces/any(face: face/Name eq 'John Doe')&$select=Start,End```
- Search for scenes that have a refrigerator in them: ```$count=true&$filter=Labels/any(label: label/Name eq 'refrigerator')&$select=Start,End```
- Find me the joyful scenes with John Doe about a restuarant: ```$filter=Faces/any(f: f/Name eq 'John Doe') and Emotions/any(e: e/Type eq 'Joy')&$count=true&search='restaurant'```
- Find me any scenes that have a visual content moderation score greater than 0.5 and what faces do we have in them?: ```$filter=VisualContentModerations/any(v: v/RacyScore gt 0.5)&facet=Faces/Name```
- What other peopel are in scenes with John Doe that have extreme close-up shots? ```facet= Faces/Name&$filter=Shots/any(s: s/Tags/any(t: t eq 'ExtremeCloseUp')) and Faces/any(f: f/Name eq 'John Doe')```

#### Thumbnail Index
- Search for any thumbnail that has John Doe in it and show me the emotional facets: ```$filter=Faces/any(f: f eq 'John Doe')&facet=Emotions/Type```
- Search for any thumbnail with Jon Doe in it indoors in a video about health and wellbeing: ```facet=Faces/Name&facet=Keywords/Text&facet=Labels&facet=Sentiments/SentimentType&$filter=Faces/any(f: f/Name eq 'John Doe') and Labels/any(l: l eq 'indoor') and Topics/any(t: t/Name eq 'Health and Wellbeing')&$count=true```
- Search for a thumbnail with an explosion (using a custom vision model): ```count=true&$filter=CustomVisionModels/any(m: m/id eq 'specialeffects')/Predictions/any(p: p/TagName eq 'explosion' and p/Probability gt 0.8)```

## The code
The main entry point for the application is ```Program.cs```. It will read the configuration section of ```appsettings.json``` and create an instance of each service according to the type: **FileStream** or **AzureBlob**. The ```AppHost.cs``` has the main logic inside the method ```Run()```.

### Services
Services contain the app logic for:
- Azure Blob Reader
- Azure Blob Writer
- FileStream Reader
- FileStream Writer
- Azure Cognitive Search
- Azure Video Indexer

### Models

The two (2) main classes that represent both scenes and thumbnails are ```Scene``` and ```Thumbnail``` respectively. These are the classes that could be extended to augment the data that will be used for the search, by adding new properties/entities according to the business requirement.

> Scene class
```csharp
public class Scene
{
    [System.ComponentModel.DataAnnotations.Key]
    public string Id { get; set; }

    public double Start { get; set; }

    public double End { get; set; }

    public List<Shot> Shots { get; set; }

    public Video Video { get; set; }

    public List<Transcript> Transcript { get; set; }

    public List<Face> Faces { get; set; }

    public List<Emotion> Emotions { get; set; }

    public List<Sentiment> Sentiments { get; set; }

    public List<Label> Labels { get; set; }

    public List<AudioEffect> AudioEffects { get; set; }

    public Playlist Playlist { get; set; }

    public List<Keyword> Keywords { get; set; }

    public List<NamedLocation> NamedLocations { get; set; }
}
```
> Thumbnail class
```csharp
public class Thumbnail
{
    [System.ComponentModel.DataAnnotations.Key]
    public string Id { get; set; }

    public string Uri { get; set; }

    public Video Video { get; set; }

    public double Start { get; set; }

    public double End { get; set; }

    public List<Face> Faces { get; set; }

    [IsFacetable, IsFilterable]
    public List<string> Labels { get; set; }

    public List<Ocr> Ocr { get; set; }

    public List<Keyword> Keywords { get; set; }

    [IsFacetable, IsFilterable]
    public List<string> ShotTags { get; set; }

    public List<Topic> Topics { get; set; }

    public Playlist Playlist { get; set; }

    public List<Sentiment> Sentiments { get; set; }

    public List<NamedLocation> NamedLocations { get; set; }

    public List<CustomVision> CustomVisionModels { get; set; }
}
```

## Configuring appsettings.json

### reader

Set up the credentials to read from Azure Blob Storage or local storage using FileStream. 
If using blob storage, add the [connection string](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=azure-portal). 

```json
"connectionString": "{connection string}"
```

The ```type``` can be either **FileStream** or **AzureBlob**.
```json
"type": "AzureBlob"
```

If using **FileStream**, add a local folder path.

```json
"insightsPath": "{local file path}",
"failedInsightsPath": "{local file path}"
```

### write

Set up the credentials to read from Azure Blob Storage or local storage using FileStream. 
If using blob storage, add the [connection string](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=azure-portal). 

```json
"connectionString": "{connection string}"
```

The ```type``` can be either **FileStream** or **AzureBlob**.
```json
"type": "AzureBlob"
```

If using **FileStream**, add a local folder path.

```json
"scenesPath": "{local file path}",
"thumbnailsPath": "{local file path}"
```

### azureSearch
Set up the credentials to connect to the [Azure Cognitive Search](https://azure.microsoft.com/en-us/services/cognitive-services/) service.

```json
"name": "{search service name}",
"adminKey": "{admin key}",
"deleteIndexIfExists": false,
```

> Optionally ```deleteIndexIfExists```can be ```true``` if the index will be re-create each time the code runs.

### videoIndexer

Set up the credentials to connect to Azure Video Indexer service. To find the keys, follow guidelines here: [Tutorial: Use the Video Indexer API](https://docs.microsoft.com/en-us/azure/media-services/video-indexer/video-indexer-use-apis).

```json
"location": "trial", 
"accountId": "{account id}",
"subscriptionKey": "{subscription key}",
"accessToken": "{accessToken}",
"pageSize": 25,
"downloadInsights": true,
"downloadThumbnails": true    
```

> An ```accessToken``` can be generated using the [Video Indexer Developer Portal](https://api-portal.videoindexer.ai/) under [Get Account Access Token](https://api-portal.videoindexer.ai/docs/services/Operations/operations/Get-Account-Access-Token/console).

> If the ```accessToken``` is not provided the application will try to create a read-only token using the ```subscriptionKey```.

If a paid account will be used then replace ```location``` with the region for the account.
 
Optionally ```pageSize```can be changed depending on the amount of videos indexed.

The flags ```downloadInsights``` and ```downloadThumbnails``` control whether the application will download insights files and thumbnails from [Azure Video Indexer](https://www.videoindexer.ai/).

## Running the App
- Clone the repository.
- Set up credentials and update the configurations/options in the ```appsettings.json``` file.
- Run the commands:

```
cd src/
dotnet build -v q
dotnet run
```

## Logger Config
The configuration for logging can be found in the found ```log4net.config```.
