[CmdLetBinding()]
param()

.\setup.ps1

if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }

$music = [FileHashLookup]::Load("G:\Mijn Documenten\Sietse\Computer\PSFolderDiffs\New\fileServerMusic.xml")

# $googlePhotos = ImportFileHashTable D:\Temp\TempDrive\myGooglePhotos.xml

# $VerbosePreference = 'Silent'

# $duplicates = $googlePhotos.GetDuplicateFiles()

# $duplicates
