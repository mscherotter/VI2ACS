# https://docs.microsoft.com/en-us/azure/search/search-get-started-powershell

$adminKey  = "<AZURE_SEARCH_ADMIN_KEY>"
$searchName = "<SEARCH_NAME>"
$vi_source_files = "<DIRECTORY WITH VIDEO INDEXER JSON>"

$url = "https://" + $searchName + ".search.windows.net/"

$apiVersion = "?api-version=2019-05-06"

$headers = @{
'api-key' = $adminKey
'Content-Type' = 'application/json' 
'Accept' = 'application/json' }

# replace with the names of the video index json files (without extensions)
$sourceFiles = "videoindex1", "videoindex2", "videoindex3", "office","accessibility"

Write-Output "Deleting scene Index"
Invoke-RestMethod -Uri ($url + "indexes/sceneindex" + $apiVersion) -Headers $headers -Method Delete 

Write-Output "Deleting thumbnail Index"
Invoke-RestMethod -Uri ($url + "indexes/thumbnailindex" + $apiVersion) -Headers $headers -Method Delete 

Write-Output "Creating Scene Index"
Invoke-RestMethod -Uri ($url + "indexes/sceneindex" + $apiVersion) -Headers $headers -Method Put -Body (Get-Content .\CreateIndex.json -Raw) # | ConvertTo-Json

Write-Output "Creating Thumbnail Index"
Invoke-RestMethod -Uri ($url + "indexes/thumbnailindex" + $apiVersion) -Headers $headers -Method Put -Body (Get-Content .\CreateThumbnailIndex.json -Raw) # | ConvertTo-Json

foreach ($sourceFile in $sourceFiles){
    Write-Output ("Adding " + $sourceFile + "...")

    $path =  $vi_source_files + $sourceFile + ".json"

    Write-Output ("Creating documents from video indexer from " + $path)
    ..\src\bin\Debug\netcoreapp3.1\VIToACS.exe $path

    $contentPath = ".\scenes_"+ $sourceFile + ".json"
    Invoke-RestMethod -Uri ($url + "indexes/sceneindex/docs/index" + $apiVersion) -Headers $headers -Method Post -Body (Get-Content $contentPath -Raw)  # | ConvertTo-Json

    $contentPath = ".\thumbnails_"+ $sourceFile + ".json" 
    Invoke-RestMethod -Uri ($url + "indexes/thumbnailindex/docs/index" + $apiVersion) -Headers $headers -Method Post -Body (Get-Content $contentPath -Raw)  #| ConvertTo-Json
}

