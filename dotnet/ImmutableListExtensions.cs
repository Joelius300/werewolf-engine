using System.Collections.Immutable;

namespace WerewolfEngine;

public static class ImmutableListExtensions
{
    /// <summary>
    /// Pops the first element (FIFO=queue) of an immutable list returning both the popped item and the remainder.
    /// </summary>
    /// <param name="collection"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Popped element and new list containing all the elements but the popped one.</returns>
    public static (T item, IImmutableList<T> rest) Pop<T>(this IImmutableList<T> collection)
    {
        return (collection[0], collection.RemoveAt(0));
    }
}