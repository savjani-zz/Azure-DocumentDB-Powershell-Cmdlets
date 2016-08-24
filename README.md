# Azure-DocumentDB-Powershell-Cmdlets
Azure Document Database Management Powershell Cmdlets

This Project is a modest attempt to create Powershell Cmdlets for Administration & Management of Azure DocumentDB Database using Powershell. I have started with a small set of cmdlets but exposing it to the community for further addition to the list.

Powershell Cmdlet Definition & its Usage

## Creating a New Database

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
$db = Add-Database -Context $ctx -Name "DocDBPS"
Get-database -Context $ctx -SelfLink $db.SelfLink | ft

## Creating a New Database & New Document Collection

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
$db = Add-Database -Context $ctx -Name "DocDBPS2"
$coll = Add-DocumentCollection -Context $ctx -DatabaseLink $db.SelfLink -Name "DocCollPS"
Get-database -Context $ctx -SelfLink $db.SelfLink | ft

## Creating a New Database & New Document Collection & Setting Automatic Indexing Policy

Import-Module 'C:\Users\pariks\Documents\GitHub\Azure-DocumentDB-Powershell-Cmdlets\Azrdocdb\Azrdocdb1\bin\Debug\Azrdocdb.dll'
$ctx = New-Context -Uri <uri> -Key <key>
$db = Add-Database -Context $ctx -Name 'DocDB'
$coll = Add-DocumentCollection -Context $ctx -DatabaseLink $db.SelfLink -Name 'DocCollPS' -AutoIndexing $true 
Get-database -Context $ctx -SelfLink $db.SelfLink | ft

## Creating a New Database & New Document Collection & Setting Automatic Indexing Policy & IndexingMode 

Import-Module 'C:\Users\pariks\Documents\GitHub\Azure-DocumentDB-Powershell-Cmdlets\Azrdocdb\Azrdocdb1\bin\Debug\Azrdocdb.dll'
$ctx = New-Context -Uri <uri> -Key <key>
$db = Add-Database -Context $ctx -Name 'DocDB'
$coll = Add-DocumentCollection -Context $ctx -DatabaseLink $db.SelfLink -Name 'DocCollPS' -AutoIndexing $true -IndexingMode 'Lazy'
Get-database -Context $ctx -SelfLink $db.SelfLink | ft


## List all the Databases in the Given Azure DocumentDB Database Account

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
Get-databases -Context $ctx | ft

## List Database Properties for a given Database

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
$db = Get-databases -Context $ctx | Where-Object {$_.Id -eq "DocDBPS"}
Get-database -Context $ctx -SelfLink $db.SelfLink | ft

## Drop Database 

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
$db = Get-databases -Context $ctx | Where-Object {$_.Id -eq "DocDBPS"}
remove-database -Context $ctx -SelfLink $db.SelfLink 

## Get Database Account Consistency Level

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
Get-DatabaseAccountConsistencyLevel -Context $ctx

## Set Database Account Consistency Level

Import-Module C:\Users\pariks\Documents\WindowsPowerShell\Modules\DocDB\Azrdocdb.dll
$ctx = New-Context -Uri <uri> -Key <key>
Set-DatabaseAccountConsistencyLevel -Context $ctx -DefaultConsistencyLevel Eventual
Get-DatabaseAccountConsistencyLevel -Context $ctx

## Add Document

Import-Module "C:\Program Files\WindowsPowerShell\Modules\Azrdocdb\Azrdocdb.dll"
$ctx = New-Context -Uri <uri> -Key <key>
$SelfLink = Get-databases -Context $ctx | Where-Object {$_.Id -eq "<dbName>"}
#To create new collection, use next 2 lines. For existing collection, use only 2nd line
#$coll = Add-DocumentCollection -Context $ctx -DatabaseLink $SelfLink.SelfLink -Name "<collectionName>"
$collName = "dbs/<dbName>/colls/<collectionName>/"
#Adds *.json from a folder. In this case, C:\JsonDocs
$doc = Add-DocDbDocument -DatabaseLink $SelfLink.SelfLink -Context $ctx -CollectionPath $collName -Folder "C:\JsonDocs"

## Add Stored Procedure

Import-Module "C:\Program Files\WindowsPowerShell\Modules\Azrdocdb\Azrdocdb.dll"
$ctx = New-Context -Uri <uri> -Key <key>
#For existing database. For new database, use Add-Database
$SelfLink = Get-databases -Context $ctx | Where-Object {$_.Id -eq "<dbName>"}
#To create new collection, use next 2 lines. For existing collection, use only 2nd line
#$coll = Add-DocumentCollection -Context $ctx -DatabaseLink $SelfLink.SelfLink -Name "<collectionName>"
$collName = "dbs/<dbName>/colls/<collectionName>/"
#Adds *.js from a folder. In this case, C:\JsonDocs
$doc = Add-StoredProc -DatabaseLink $SelfLink.SelfLink -Context $ctx -CollectionPath $collName -Folder "C:\JsonDocs"


