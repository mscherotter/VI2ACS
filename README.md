# Video Indexer to Azure Cognitive Search
Created by Michael S. Scherotter

Convert Azure Video Indexer JSON data into Azure Cognitive Search scene and thumbnail indices.

## Usage
1. Build source code src\VIToACS.csproj
2. Run video files through Microsoft Video Indexer to extract .JSON insights files
3. Put .JSON insights files into folder and put the folder name into the $vi_source_files variable in the ConfigureSearchService.ps1 script. 
4. Create an Azure Search service and put the admin key and search service name into the $adminKey and $searchName variables in the  ConfigureSearchService.ps1
5. List the names of the insight files (without .json extension) in the $sourceFiles
6. Run the ConfigureSearchService.ps1 PowerShell script.

Now all of the video index data is in two search indexes in Azure Cognitive Search, **scenes**, and **thumbnails**. 

#Scene Index
The scene index lets you create queries for scenes that have certain metadata, including transcript, faces, emotions, sentiment, labels, and audio effects

#Thumbnail Index
The thumbnail index lets you create queries for thumbnails that have been extracted by video indexer that have certain metadata including faces, labels, OCR, keywords, and shot tags 
