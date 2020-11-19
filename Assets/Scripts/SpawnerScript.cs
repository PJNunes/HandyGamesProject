using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the behaviour of the spawner component
public class SpawnerScript : MonoBehaviour
{

    // Reference to the Dirt Block Prefab
    [SerializeField]
    protected GameObject dirtBlock;

    // Reference to the Grass Block Prefab
    [SerializeField]
    protected GameObject grassBlock;

    // Reference to the Stone Block Prefab
    [SerializeField]
    protected GameObject stoneBlock;

    // Reference to the Water Block Prefab
    [SerializeField]
    protected GameObject waterBlock;

    // Reference to the Wood Block Prefab
    [SerializeField]
    protected GameObject woodBlock;

    // Reference to the Ramp East Prefab
    [SerializeField]
    protected GameObject rampEBlock;

    // Reference to the Ramp West Prefab
    [SerializeField]
    protected GameObject rampWBlock;

    // Reference to the Bug Character
    [SerializeField]
    protected GameObject bug;

    // Reference to the Boy Character
    [SerializeField]
    protected GameObject boy;

    // Reference to the Cat Girl Character
    [SerializeField]
    protected GameObject catGirl;

    // Reference to the Horn Girl Character
    [SerializeField]
    protected GameObject hornGirl;

    // Reference to the Pink Girl Character
    [SerializeField]
    protected GameObject pinkGirl;

    // Reference to the Princess Girl Character
    [SerializeField]
    protected GameObject princGirl;

    //Enum with possible Terrain Types
    enum TerrainType {dirt, grass, stone, water, wood};

    /* Enum with the states for terrain creation: 
        ramp - a ramp was created for going down a level
        force - current level cannot change level;
        free - anything can be created
     */
    enum TerrainHidden {ramp, forced, free};
    
    // Type of terrain per columns
    int[] terrains;

    // Current level of the terrain per column
    int[] columnLevels;

    // Y Coordinates to the characters to be spawned, by layer
    float[] layerSpawnCoordinates;

    // List of all terrain Prefabs
    GameObject[] blockPrefabs;

    // List of all character prefabs
    GameObject[] availCharacters;

    // List of possible chars to spawn
    List<int> charToSpawn;

    // State of the terrain generator
    TerrainHidden terrainState;

    // Last spawned layer
    int spawnedLayer;

    // If there is a hostage on screen
    bool hostageOnScreen;

    // Total amount of columns in the map
    int columnAmount;

    // Total amount of layers in the map
    int layersAmount;

    // Value to increase per level gained
    float terrainHeightBase;

    // Value to increase per line created
    float terrainHeightLong;

    // Start is called before the first frame update
    void Start()
    {
        terrainHeightBase = 0.4f;
        terrainHeightLong = 0.8f;
        columnAmount = 8;
        layersAmount = 5;

        terrainState = TerrainHidden.free;

        blockPrefabs = new GameObject[]{dirtBlock, grassBlock, stoneBlock, waterBlock, woodBlock};
        availCharacters = new GameObject[]{boy, catGirl, hornGirl, pinkGirl, princGirl};
        charToSpawn = new List<int>{0, 1, 2, 3, 4};
        
        terrains = new int[columnAmount];
        columnLevels = new int[columnAmount];
        layerSpawnCoordinates = new float[layersAmount];

        spawnedLayer = -1;
        hostageOnScreen = false;

        GenerateTerrain();
        InvokeRepeating ("SpawnCharacter", 1, 1);
    }

    // Generates the Terrain of the map
    void GenerateTerrain()
    {
        int block, level, minLevel, maxLevel, startConsecutive;
        float height;

        // Randomizes the type of terrain to spawn
        for(int i=0; i<columnAmount; i++)
        {            
            block = Random.Range(0, 5);
            terrains[i]=block;                        
        }
        
        // Generates the remaining layers
        for (int layer = 0; layer<layersAmount; layer++)
        {
            terrainState = TerrainHidden.free;

            // Randomly selects an area to have three consecutive unobstructed blocks
            startConsecutive = Random.Range(0, columnAmount-4);
            for(int i=0; i<columnAmount; i++)
            {        
                if (terrainState != TerrainHidden.free)
                {
                    level = columnLevels[i-1];
                    terrainState = (terrainState == TerrainHidden.ramp ? TerrainHidden.forced : TerrainHidden.free);

                    if(i >= startConsecutive && i < startConsecutive+3)
                        startConsecutive = i+1;
                }
                else
                {
                    // Ensures that the consecutive blocks aren't a lower level
                    if(i >= startConsecutive && i < startConsecutive+3)
                        minLevel = Mathf.Max(columnLevels[i], columnLevels[Mathf.Max(0, i-1)]);
                    else
                        minLevel = Mathf.Max(0, Mathf.Max(columnLevels[i]-1, columnLevels[Mathf.Max(0, i-1)]-1));

                    maxLevel = Mathf.Min(2, Mathf.Min(columnLevels[i]+1, columnLevels[Mathf.Max(0, i-1)]+1));

                    if (maxLevel<minLevel)
                    {
                        level = maxLevel;
                        startConsecutive++;
                    }
                    else
                        level = Random.Range(minLevel, maxLevel+1);
                }

                // Calculates the Y position to spawn the object based on it's level and layer                                                
                height = terrainHeightBase * (level + 1) + terrainHeightLong * layer;

                // There is only one fifth of a chance for a block in a column to swap it's type
                block = Random.Range(0,5) == 0 ? Random.Range(0, 5) : terrains[i];
                terrains[i]=block;                    
                
                // Spawns terrain prefab and sets the specified ordering layer
                Instantiate(blockPrefabs[block], new Vector3(i, height, 0), Quaternion.identity).SendMessage("SetBlockLayer", layer);

                // Spawns an aditional block in the block in the new layer is too high, removing then the whitespace
                if(level - columnLevels[i] > 1 || (layer == 0 && level > 0))
                {
                    Instantiate(blockPrefabs[block], new Vector3(i, height-terrainHeightBase, 0), Quaternion.identity).SendMessage("SetBlockLayer", layer+1);                    
                }
                
                // Adds a ramp on the top of itself if the block is lower than the one on it's left
                if(i > 0 && columnLevels[i-1] > level)
                {
                    height += terrainHeightBase;
                    
                    Instantiate(rampEBlock, new Vector3(i, height, 0), Quaternion.identity).SendMessage("SetRampLayer", layer);
                    terrainState = TerrainHidden.ramp;
                }
                // Adds a ramp on the left if the block is higher than the one on it's left
                else if(i > 0 && columnLevels[i-1] < level)
                {
                    Instantiate(rampWBlock, new Vector3(i-1, height, 0), Quaternion.identity).SendMessage("SetRampLayer", layer);
                }
                
                columnLevels[i] = level;
            }

            // Calculates the coordinates for the characters to spawn in that layer
            layerSpawnCoordinates[layer]= terrainHeightBase * (columnLevels[0] + 2) + terrainHeightLong * layer;
        }
    }

    // Function called to spawn a character
    void SpawnCharacter()
    {
        int layer;

        // Ensures a different layer is selected every turn
        do
        {
            layer = Random.Range(0, layersAmount);
        }
        while(spawnedLayer == layer);

        spawnedLayer = layer;

        // A bug is spawned if there is already a character on the screen or half of the times
        if(hostageOnScreen || charToSpawn.Count == 0 || Random.Range(0, 2) != 0)
        {
            Instantiate(bug, new Vector3(-2.5f, layerSpawnCoordinates[layer], 0), Quaternion.identity).SendMessage("SetInfo", new float[2]{layerSpawnCoordinates[layer], layer});
        }
        else
        {
            // Gets a random character
            int randomChar = Random.Range(0, charToSpawn.Count);
            
            GameObject hostage = Instantiate(availCharacters[charToSpawn[randomChar]], new Vector3(-2.5f, layerSpawnCoordinates[layer], 0), Quaternion.identity);
            hostage.SendMessage("SetInfo", new float[3]{layerSpawnCoordinates[layer], layer, charToSpawn[randomChar]});

            // Spawns a bug before and after the character
            GameObject bugClone = Instantiate(bug, new Vector3(-1.5f, layerSpawnCoordinates[layer], 0), Quaternion.identity);
            bugClone.SendMessage("SetInfo", new float[2]{layerSpawnCoordinates[layer], layer});
            bugClone.SendMessage("SetHostage", hostage);
            
            bugClone = Instantiate(bug, new Vector3(-3.5f, layerSpawnCoordinates[layer], 0), Quaternion.identity);
            bugClone.SendMessage("SetInfo", new float[2]{layerSpawnCoordinates[layer], layer});
            bugClone.SendMessage("SetHostage", hostage);

            // Removes character from the possible list of characters
            charToSpawn.RemoveAt(randomChar);
            hostageOnScreen = true;
        }
    }

    // Called to inform that there are no characters on scree
    public void HostageOut(int hostageId)
    {
        hostageOnScreen = false;

        // If Id is given, character will be respawned
        if (hostageId != -1)
        {
            charToSpawn.Add(hostageId);
        }
    }
}
