# Video Indexer to Azure Cognitive Search
Created by Michael S. Scherotter

Convert Azure [Video Indexer](https://www.videoindexer.ai/) insight data into [Azure Cognitive Search](https://azure.microsoft.com/en-us/services/cognitive-services/) scene and thumbnail indices.

## Usage
1. Build source code src\VIToACS.csproj.  VI2oACS converts the JSON from video indexer to the JSON to insert documents into Azure Cognitive Search.
2. Run video files through Microsoft Video Indexer to extract .JSON insights files
3. Put .JSON insights files into folder and put the folder name into the **$vi_source_files** variable in the ConfigureSearchService.ps1 script. 
4. Create an Azure Search service and put the admin key and search service name into the **$adminKey** and **$searchName** variables in the  ConfigureSearchService.ps1
5. List the names of the insight files (without .json extension) in the **$sourceFiles** variable in the ConfigureSearchService.ps1 PowerShell script. 
6. Run the ConfigureSearchService.ps1 PowerShell script.

Now all of the video index data is in two search indexes in Azure Cognitive Search, **scenesindex**, and **thumbnailindex**. 

## Scene Index
The **sceneindex** lets you create queries for scenes that have certain metadata, including transcript, faces, emotions, sentiment, labels, and audio effects.  There should be one document for each scene in each video.

## Thumbnail Index
The **thumbnailindex** lets you create queries for thumbnails that have been extracted by video indexer that have certain metadata including faces, labels, OCR, keywords, and shot tags.  There should be one document for each thumbnail in each video.
