using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeCollapsingTile : MonoBehaviour
{
    public void SetTileSprite(int indexOfTile)
    {
        //匹配精灵与父对象的瓦片索引
        GetComponent<SpriteRenderer>().sprite = GetComponentInParent<CollapsingPlatformTiles>().tiles[indexOfTile];
    }
}
