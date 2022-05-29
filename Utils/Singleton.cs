
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Singleton<T> where T : class, new()
{
#if UNITY_EDITOR
    static Singleton()
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

    private static T instance = null;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }

    public static void Deinitialize()
    {
        instance = null;
    }
}