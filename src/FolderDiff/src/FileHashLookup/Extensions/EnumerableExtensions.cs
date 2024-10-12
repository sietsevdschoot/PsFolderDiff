namespace PsFolderDiff.FileHashLookup.Extensions;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static void InsertNewItems<T>(this List<T> @this, List<T> other, IEqualityComparer<T>? comparer = null)
    {
        foreach (var otherEntry in other)
        {
            if (!@this.Contains(otherEntry, comparer ?? EqualityComparer<T>.Default))
            {
                @this.Add(otherEntry);
            }
        }
    }
}