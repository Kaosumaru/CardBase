using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Linq.Extensions
{
    public static class EnumerableHelper<E>
    {
        private readonly static System.Random r = new System.Random();

        public static T Random<T>(IEnumerable<T> input)
        {
            if (input.Count() == 0) return default(T);
            return input.ElementAt(r.Next(input.Count()));
        }

    }

    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> input)
        {
            return EnumerableHelper<T>.Random(input);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> input)
        {
            return input.OrderBy(x => Guid.NewGuid());
        }

        public static IEnumerable<T> Without<T>(this IEnumerable<T> input, T value)
        {
            return input.Where( x => !Equals(x, value) );
        }
    }
}

public class ResourceAssetLibrary<T> where T : UnityEngine.Object
{
    public ResourceAssetLibrary(string path)
    {
        _path = path;
        Scan();
    }

    public void Scan()
    {
        var cards = Resources.LoadAll<T>(_path);
        foreach (var card in cards)
        {
            Debug.Assert(!_objects.ContainsKey(card.name));
            _objects.Add(card.name, card);
        }
    }

    public T GetObject(string id)
    {
        _objects.TryGetValue(id, out var data);
        return data;
    }

    string _path;
    Dictionary<string, T> _objects = new Dictionary<string, T>();
}