#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ParticleSystemPrefabCreator : EditorWindow
{
    private ParticleSystemCreator creator;
    private string savePath = "Assets/Prefabs/Particles";
    private bool combineIntoSinglePrefab = false;
    
    [MenuItem("Tools/Particle System Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<ParticleSystemPrefabCreator>("Particle Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Particle System Prefab Creator", EditorStyles.boldLabel);
        
        // Create the particle creator if needed
        if (creator == null)
        {
            if (GUILayout.Button("Create Particle System Creator"))
            {
                GameObject obj = new GameObject("Temporary Particle Creator");
                creator = obj.AddComponent<ParticleSystemCreator>();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Customize the particle systems in the inspector of the Temporary Particle Creator object.", MessageType.Info);
            
            // Display save path field
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            
            // Option to combine into a single prefab
            combineIntoSinglePrefab = EditorGUILayout.Toggle("Create Combined Prefab", combineIntoSinglePrefab);
            
            if (GUILayout.Button("Save Particle Prefabs"))
            {
                SaveParticlesAsPrefabs();
            }
            
            if (GUILayout.Button("Remove Temporary Creator"))
            {
                DestroyImmediate(creator.gameObject);
                creator = null;
            }
        }
    }
    
    private void SaveParticlesAsPrefabs()
    {
        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string[] folderLevels = savePath.Split('/');
            string currentPath = folderLevels[0]; // Should be "Assets"
            
            for (int i = 1; i < folderLevels.Length; i++)
            {
                string folderName = folderLevels[i];
                string parentPath = currentPath;
                currentPath += "/" + folderName;
                
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }
            }
        }
        
        // Create all particle systems
        creator.CreateAllParticleSystems();
        
        // Get references to the created particle systems
        ParticleSystem jumpPS = creator.transform.Find("JumpParticles")?.GetComponent<ParticleSystem>();
        ParticleSystem doubleJumpPS = creator.transform.Find("DoubleJumpParticles")?.GetComponent<ParticleSystem>();
        ParticleSystem landPS = creator.transform.Find("LandParticles")?.GetComponent<ParticleSystem>();
        ParticleSystem speedPS = creator.transform.Find("SpeedParticles")?.GetComponent<ParticleSystem>();
        ParticleSystem specialPS = creator.transform.Find("SpecialParticles")?.GetComponent<ParticleSystem>();
        
        if (combineIntoSinglePrefab)
        {
            // Create a parent object for the combined prefab
            GameObject combinedObject = new GameObject("PlayerParticlesComplete");
            
            // Parent particles (if they exist) to the combined object
            if (jumpPS != null) jumpPS.transform.SetParent(combinedObject.transform, false);
            if (doubleJumpPS != null) doubleJumpPS.transform.SetParent(combinedObject.transform, false);
            if (landPS != null) landPS.transform.SetParent(combinedObject.transform, false);
            if (speedPS != null) speedPS.transform.SetParent(combinedObject.transform, false);
            if (specialPS != null) specialPS.transform.SetParent(combinedObject.transform, false);
            
            // Save the combined prefab
            string combinedPath = $"{savePath}/PlayerParticlesComplete.prefab";
            PrefabUtility.SaveAsPrefabAsset(combinedObject, combinedPath);
            
            // Clean up
            DestroyImmediate(combinedObject);
            
            //Debug.Log("Created combined particle system prefab at: " + combinedPath);
        }
        else
        {
            // Save individual prefabs
            if (jumpPS != null) PrefabUtility.SaveAsPrefabAsset(jumpPS.gameObject, $"{savePath}/JumpParticles.prefab");
            if (doubleJumpPS != null) PrefabUtility.SaveAsPrefabAsset(doubleJumpPS.gameObject, $"{savePath}/DoubleJumpParticles.prefab");
            if (landPS != null) PrefabUtility.SaveAsPrefabAsset(landPS.gameObject, $"{savePath}/LandParticles.prefab");
            if (speedPS != null) PrefabUtility.SaveAsPrefabAsset(speedPS.gameObject, $"{savePath}/SpeedParticles.prefab");
            if (specialPS != null) PrefabUtility.SaveAsPrefabAsset(specialPS.gameObject, $"{savePath}/SpecialParticles.prefab");
            
            //Debug.Log("Created individual particle system prefabs in: " + savePath);
        }
    }
}
#endif