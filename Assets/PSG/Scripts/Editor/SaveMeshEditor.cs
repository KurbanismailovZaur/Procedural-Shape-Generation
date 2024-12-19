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
        path = EditorUtility.SaveFilePanel(title, Application.dataPath, filename, extension);
        return !string.IsNullOrEmpty(path);
    }

    private void SaveMeshAsAssetWithSaveFileDialog(MeshBase meshBase, string name)
    {
        if (!TrySelectFilePlaceInDialog("Save Mesh To Asset", name, "asset", out var path))
            return;

        path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
        
        SaveMeshAsAsset(meshBase, path);
    }

    private void SaveMeshAsAsset(MeshBase meshBase, string localPath)
    {
        // Make a copy of Mesh to prevent sharing it among other MeshFilters
        var meshCopy = Instantiate(meshBase.C_MF.sharedMesh);

        AssetDatabase.CreateAsset(meshCopy, localPath);
        var meshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(localPath);
        meshBase.C_MF.sharedMesh = meshAsset;
        
        EditorGUIUtility.PingObject(meshAsset);
    }

    private void SaveMaterialAsAsset(MeshBase meshBase, string localPath)
    {
        var materialCopy = Instantiate(meshBase.C_MR.sharedMaterial);

        AssetDatabase.CreateAsset(materialCopy, localPath);
        var materialAsset = AssetDatabase.LoadAssetAtPath<Material>(localPath);
        meshBase.C_MR.sharedMaterial = materialAsset;
    }
    
    private void SavePhysicsMaterial2DAsAsset(MeshBase meshBase, string localPath)
    {
        var collider2D = meshBase.GetComponent<Collider2D>();
        var physicsMaterialCopy = Instantiate(collider2D.sharedMaterial);

        AssetDatabase.CreateAsset(physicsMaterialCopy, localPath);
        var physicsMaterialAsset = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(localPath);
        collider2D.sharedMaterial = physicsMaterialAsset;
    }


    private void SaveGameObjectAsPrefabWithSaveFileDialog(MeshBase meshBase, string name)
    {
        if (!TrySelectFilePlaceInDialog("Save As Prefab", name, "prefab", out var path))
            return;

        var meshPath = Path.ChangeExtension(path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)), ".asset");
        SaveMeshAsAsset(meshBase, meshPath);

        var materialPath = Path.ChangeExtension(meshPath, ".mat");
        SaveMaterialAsAsset(meshBase, materialPath);

        var physicsMaterial2DPath = Path.ChangeExtension(meshPath, ".physicsMaterial2D");
        SavePhysicsMaterial2DAsAsset(meshBase, physicsMaterial2DPath);

        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(meshBase.gameObject, path, InteractionMode.AutomatedAction);
        prefab.GetComponent<MeshFilter>().sharedMesh = meshBase.C_MF.sharedMesh;
        EditorGUIUtility.PingObject(prefab);
    }
}