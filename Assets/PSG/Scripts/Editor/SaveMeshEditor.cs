using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using PSG;

/// <summary>
/// 
/// Saving generated meshes.
/// 
/// Meshes and objects will appear in PSG/ directories by default.
/// To load, simply drag object to scene or load it by script.
/// 
/// Watch out for overwritting saved assets!
/// 
/// </summary>
[CustomEditor(typeof(MeshBase), true)]
public class SaveMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var meshBase = (MeshBase)target;
        
        if (GUILayout.Button("Save Mesh As Asset")) 
            SaveMeshAsAssetWithSaveFileDialog(meshBase, meshBase.name);

        if (GUILayout.Button("Save GameObject As Prefab")) 
            SaveGameObjectAsPrefabWithSaveFileDialog(meshBase, meshBase.name);
    }

    private bool TrySelectFilePlaceInDialog(string title, string filename, string extension, out string path)
    {
        path = EditorUtility.SaveFilePanel(title, null, filename, extension);
        return !string.IsNullOrEmpty(path);
    }

    private void SaveMeshAsAssetWithSaveFileDialog(MeshBase meshBase, string name)
    {
        if (!TrySelectFilePlaceInDialog("Save Mesh To Asset", name, "asset", out var path))
            return;

        path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
        
        SaveMeshAsAsset(meshBase, path);
    }

    private void SaveMeshAsAsset(MeshBase meshBase, string path)
    {
        // Make a copy of Mesh to prevent sharing it among other MeshFilters
        var meshCopy = Instantiate(meshBase.C_MF.sharedMesh);
        
        AssetDatabase.CreateAsset(meshCopy, path);
        var meshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        meshBase.C_MF.sharedMesh = meshCopy;
        
        EditorGUIUtility.PingObject(meshAsset);
    } 

    private void SaveGameObjectAsPrefabWithSaveFileDialog(MeshBase meshBase, string name)
    {
        if (!TrySelectFilePlaceInDialog("Save As Prefab", name, "prefab", out var path))
            return;

        var meshPath = Path.ChangeExtension(path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)), ".asset");
        
        // Mesh need to be saved too
        SaveMeshAsAsset(meshBase, meshPath);

        if (File.Exists(path))
            AssetDatabase.DeleteAsset(path);
        
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(meshBase.gameObject, path, InteractionMode.AutomatedAction);
        EditorGUIUtility.PingObject(prefab);
    }

    private void SaveMaterial(Material material, string name)
    {
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(material)))
        {
            AssetDatabase.CreateAsset(material, "Assets/PSG/Saved meshes/" + name + "'s material.asset");
        }
    }
}