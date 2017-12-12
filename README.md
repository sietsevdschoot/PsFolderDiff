# PS-FolderDiff

FolderDiff is a Powershell **command line tool** to compare folders contents.
<Enter>
In order to do it quick and thoroughly, it creates a two-way hashtable of all files and their hashcode fingerprint. 

## Installation

Run the setup file

```PowerShell
.\setup.ps1
````

## Getting Started

Getting started is straight forward: 

```PowerShell
$myDocs = Get-FileHashTable "C:\MyDocuments"
$myBackups = Get-FileHashTable "C:\Backups"

$duplicates = $myDocs.GetMatchesInOther($myBackups)

$duplicates.GetFiles() | del -force

$myBackups.Refresh()
$myBackups.Save()
````

## Operations

#### GetFiles
```PowerShell
[IO.FileInfo[]] GetFiles()
````
Returns all files

#### GetFilesByHash
```PowerShell
[IO.FileInfo[]] GetFilesByHash([IO.FileInfo] $file)
````
Returns all files which share the same hash as **$file**

#### AddFolder
```PowerShell
void AddFolder([IO.DirectoryInfo] $path)
````
Adds all files in **$path** to FileHashLookup

#### AddFileHashTable
```PowerShell
void AddFileHashTable([FileHashLookup] $other)
````
Adds all files in **$other** to [FileHashLookup]

#### Contains
```PowerShell
bool Contains([IO.FileInfo] $file)
````
returns true if contains file which share the same hash as **$file**

#### GetMatchesInOther
```PowerShell
[FileHashLookup] GetMatchesInOther([FileHashLookup] $other)
````
returns a new [FileHashLookup] with all files in **$other** which match with files in $this:[FileHashLookup]

#### GetDiffInOther
```PowerShell
[FileHashLookup] GetDiffInOther([FileHashLookup] $other)
````
returns a new [FileHashLookup] with all files in **$other** which don''t match with files in $this:[FileHashLookup]

#### Refresh
```PowerShell
void Refresh()
````
Updates all files in tracked folders: 
* Added files
* Removed files
* Modified files

#### Save
```PowerShell
void Save([IO.FileInfo] $filename)
````

Saves the contents in  **$filename**

**$filename** can be omitted if it was saved before. 

#### ExcludeFilePattern
```PowerShell
void ExcludeFilePattern(string $filePattern)
````

Adds a file pattern to exclude. (e.g. *.iso).

This filter is also applied on already tracked files.

#### ExcludeFolder
```PowerShell
void ExcludeFolder([IO.DirectoryInfo] $folder)
````
Adds a directory to be excluded.

This filter is also applied on already tracked files.


## Requirements

In order to run the tests, Pester 4.1.0 is required.

