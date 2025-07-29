using System;
using System.Collections.Generic;
using System.Linq;


public static class RandomHelper
{
    public static readonly Random Rng = new Random();
}

public static class EnumerableExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = RandomHelper.Rng.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static List<T> Shuffled<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        list.Shuffle();
        return list;
    }
}