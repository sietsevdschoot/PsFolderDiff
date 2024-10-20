namespace PsFolderDiff.FileHashLookupLib.Domain;

public enum FileContainsState
{
    Unspecificied = 0,
    NoMatch,
    Match,
    Modified,
}