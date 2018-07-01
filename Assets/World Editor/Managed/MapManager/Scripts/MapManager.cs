using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapManager : MonoBehaviour 
{
    #region [Section] CONST
    #region [CONST] CONST_DirectoryMaps
    public const string CONST_DirectoryMaps = 
#if UNITY_EDITOR
    "./Assets/World Editor/Managed/MapManager/Editor/MapsExamples/";
#else
    "./Maps/";
#endif
    #endregion
    #endregion
    
    #region [Section] Manager Header
    public static MapManager Instance { get; private set; }
    public static Boolean IsInitialized => Instance != null;
    #endregion
    
    public WorldSerialization WorldSerialization { get; private set; }
    public Terrain CurrentTerrain { get; private set; }
    public List<GameObject> TerrainGObjects { get; private set; }

    [SerializeField] private GameObject GObject_EmptyPrefab;

    private void Awake()
    {
        Instance = this;
        this.ValidationFileSystem();
        this.TerrainGObjects = new List<GameObject>();
        this.WorldSerialization = new WorldSerialization();
    }

    /// <summary>
    /// Check FileSystem, if dont have required files or directory
    /// </summary>
    private void ValidationFileSystem()
    {
        if (Directory.Exists(CONST_DirectoryMaps) == false)
        {
            Directory.CreateDirectory(CONST_DirectoryMaps);
            Debug.LogWarning($"Directory [{CONST_DirectoryMaps}] dont exists, directory created!");
        }
    }

    /// <summary>
    /// Open map file to ./Maps from filename
    /// </summary>
    /// <param name="filename">Full name file in directory ./Maps</param>
    /// <returns></returns>
    public void OpenMap(string filename)
    {
        if (File.Exists(CONST_DirectoryMaps + filename) == false)
        {
            Debug.LogError($"File [{filename}] dont found to [{CONST_DirectoryMaps}] diorectory!");
            return;
        }

        if (this.WorldSerialization.World.maps.Count != 0)
        {
            Debug.LogError($"You dont closed other map, please use: MapManager.Instance.CloseMap()");
            return;
        }

        try
        {
            this.WorldSerialization.Load(CONST_DirectoryMaps + filename); 
            if (this.WorldSerialization.World.maps.Count == 0)
            {
                if (this.WorldSerialization.Version != WorldSerialization.CurrentVersion)
                    Debug.LogError($"File [{filename}] have version: {this.WorldSerialization.Version}, need version: {WorldSerialization.CurrentVersion}");
                else
                    Debug.Log($"File [{filename}] is not loaded, unknown error!");
                return;
            }
            this.GenerationTerrain();
            this.GenerationTerrainheightMap();
            this.SpawnGObjects();
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception to AppManager.OpenMap: " + ex.Message);
        }
    }

    public void CloseMap()
    {
        if (this.WorldSerialization.World.maps.Count == 0)
        {
            Debug.LogError($"You dont have open Map");
            return;
        }
        
        for (var i = 0; i < this.TerrainGObjects.Count; i++)
            Destroy(this.TerrainGObjects[i]);
        
        Destroy(this.CurrentTerrain.gameObject);
        this.WorldSerialization.Clear();
    }

    private void SpawnGObjects()
    {
        if (this.GObject_EmptyPrefab != null)
        {
            for (var i = 0; i < this.WorldSerialization.World.prefabs.Count; i++)
            {
                GameObject gameObject = GameObject.Instantiate(this.GObject_EmptyPrefab, this.WorldSerialization.World.prefabs[i].position, this.WorldSerialization.World.prefabs[i].rotation);
                gameObject.transform.localScale = this.WorldSerialization.World.prefabs[i].scale;
                gameObject.name = "G: " + this.WorldSerialization.World.prefabs[i].id + " : " + this.WorldSerialization.World.prefabs[i].category;
                this.TerrainGObjects.Add(gameObject);
            }
        }
    }

    private void GenerationTerrain()
    {
        TerrainData assignTerrain = new TerrainData {
            baseMapResolution = Mathf.NextPowerOfTwo((int) (this.WorldSerialization.World.size * 0.01f)),
            heightmapResolution = Mathf.NextPowerOfTwo((int) (this.WorldSerialization.World.size * 0.5f)) + 1,
            alphamapResolution = Mathf.NextPowerOfTwo((int) (this.WorldSerialization.World.size * 0.5f)),
            size = new Vector3((float) this.WorldSerialization.World.size, 1000f, (float) this.WorldSerialization.World.size)
        };
		
        this.CurrentTerrain = Terrain.CreateTerrainGameObject(assignTerrain).GetComponent<Terrain>();
        this.CurrentTerrain.transform.position = new Vector3(this.CurrentTerrain.transform.position.x - (this.CurrentTerrain.terrainData.size.x / 2), this.CurrentTerrain.transform.position.y - 500, this.CurrentTerrain.transform.position.z - (this.CurrentTerrain.terrainData.size.z / 2));        
    }

    private void GenerationTerrainheightMap()
    {
        if (this.CurrentTerrain != null)
        {
            var terrainMap = new TerrainMap<short>(this.WorldSerialization.GetMap("terrain").data, 1);

            var heights = new float[this.CurrentTerrain.terrainData.heightmapResolution, this.CurrentTerrain.terrainData.heightmapResolution];
            for (int x = 0; x < this.CurrentTerrain.terrainData.heightmapResolution; ++x)
            {
                for (int y = 0; y < this.CurrentTerrain.terrainData.heightmapResolution; ++y)
                    heights[y, x] = BitUtility.Short2Float(terrainMap.src[(x * terrainMap.res) + y]);
            }

            this.CurrentTerrain.terrainData.SetHeights(0, 0, heights);
        }
    }
}
