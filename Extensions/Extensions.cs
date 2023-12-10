using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> input)
        {
            return input.Where(x => x != null);
        }
    }
}

public class ResourceAssetLibrary<T> where T : UnityEngine.Object
{
    ResourceAssetLibrary(string path)
    {
        _path = path;
        Scan();
    }

    public void Scan()
    {
        var assets = Resources.LoadAll<T>(_path);
        foreach (var asset in assets)
        {
            _objects.Add(asset.name, asset);
            _objectList.Add(asset);
        }
    }

    public T GetObject(string guid)
    {
        _objects.TryGetValue(guid, out var data);
        return data;
    }

    public List<T> AllAssets()
    {
        return _objectList;
    }


#if UNITY_EDITOR
    static ResourceAssetLibrary()
    {
        EditorApplication.playModeStateChanged += PlayModeState;
    }

    private static void PlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // deinitialize all singletons when entering play mode
            // this is done to handle disabled "domain reload" option in Unity
            Deinitialize();
        }
    }
#endif

    private static ResourceAssetLibrary<T> instance = null;

    public static void Deinitialize()
    {
        instance = null;
    }

    public static ResourceAssetLibrary<T> Get(string path)
    {
        if (instance == null)
            instance = new ResourceAssetLibrary<T>(path);

        return instance;
    }

    string _path;
    List<T> _objectList = new();
    Dictionary<string, T> _objects = new Dictionary<string, T>();
}