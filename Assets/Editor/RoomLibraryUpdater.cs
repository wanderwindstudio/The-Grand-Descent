using UnityEditor;
using UnityEngine;

// Keeps Assets/Rooms/RoomLibrary.asset listing every prefab in Assets/Rooms that has a RoomModule.
// Runs by itself whenever a prefab in that folder is saved, added, moved or deleted.
public class RoomLibraryUpdater : AssetPostprocessor
{
    public const string LibraryPath = "Assets/Rooms/RoomLibrary.asset";

    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (var list in new[] { imported, deleted, moved, movedFrom })
            foreach (var path in list)
                if (path.StartsWith("Assets/Rooms/") && path.EndsWith(".prefab"))
                {
                    EditorApplication.delayCall -= RefreshQuietly;
                    EditorApplication.delayCall += RefreshQuietly;
                    return;
                }
    }

    static void RefreshQuietly() => Refresh();

    [MenuItem("Grand Descent/Refresh Room List")]
    public static RoomLibrary Refresh()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Rooms")) AssetDatabase.CreateFolder("Assets", "Rooms");
        var library = AssetDatabase.LoadAssetAtPath<RoomLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<RoomLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        library.rooms.Clear();
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Rooms" }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (prefab != null && prefab.GetComponent<RoomModule>() != null) library.rooms.Add(prefab);
        }
        library.rooms.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        Debug.Log($"Room list updated: {library.rooms.Count} room(s) in Assets/Rooms.");
        return library;
    }
}
