using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Handles Block Management, such as layer information
public class BlockManager : MonoBehaviour
{

    // Layer of the block
    int layer;

    // Sets the block layer
    void SetBlockLayer(int l){
        layer = l;
        gameObject.layer = 9+l;
        GetComponent<SpriteRenderer>().sortingLayerName = "Terrain"+l;
    }

    // Sets the layer, used if the block is a ramp
    void SetRampLayer(int l){
        layer = l;
        gameObject.layer = 9+l;
        GetComponent<SpriteRenderer>().sortingLayerName = "Ramp"+l;
    }
}
