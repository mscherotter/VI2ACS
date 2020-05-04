# Video Indexer to Azure Cognitive Search
Created by Michael S. Scherotter

Convert Azure Video Indexer JSON data into Azure Cognitive Search scene and thumbnail indices.

## Usage
1. Run video files through Microsoft Video Indexer to extract .JSON insights files
2. Put JSON files into folder and put the folder name into the $vi_source_files variable in the ConfigureSearchService.ps1 script. 
3. Create an Azure Search service and put the admin key and search service name into the $adminKey and $searchName variables in the  ConfigureSearchService.ps1
4. List the names of the insight files (without .json extension) in the $sourceFiles
5. Run the ConfigureSearchService.ps1 PowerShell script.

Now all of the video index data is in two search indexes in Azure Cognitive Search, **scenes**, and **thumbnails**. 
