[CmdLetBinding()]
param()

.\setup.ps1

if (!$PSBoundParameters.ContainsKey('Verbose')) { $VerbosePreference = $PSCmdlet.GetVariableValue('VerbosePreference') }

$googlePhotos = ImportFileHashTable D:\Temp\TempDrive\myGooglePhotos.xml

$VerbosePreference = 'Silent'

$duplicates = $googlePhotos.GetDuplicateFiles()

$duplicates
