using System.Collections;
using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer _spriteRenderer;
    void Start()
    {
        DoubleTileTool.Init();
        var c = new Vector3(0, 0, 0);
        DoubleTileTool.SetTile(c,TileType.瓦片0);
        DoubleTileTool.SetTile(c+new Vector3(1,0,0),TileType.瓦片1);
        DoubleTileTool.SetTile(c+new Vector3(2,0,0),TileType.瓦片1);
        DoubleTileTool.SetTile(c+new Vector3(3,1,0),TileType.瓦片1);
        DoubleTileTool.SetTile(c+new Vector3(2,1,0),TileType.瓦片1);
        DoubleTileTool.SetTile(c+new Vector3(2,-1,0),TileType.瓦片1);
        DoubleTileTool.SetTile(c+new Vector3(2,-2,0),TileType.瓦片1);
        DoubleTileTool.SetTile(c + new Vector3(1,1,0),TileType.瓦片0);
        DoubleTileTool.SetTile(c + new Vector3(-1,-1,0),TileType.瓦片0);
        DoubleTileTool.SetTile(c + new Vector3(1,-2,0),TileType.瓦片0);
        DoubleTileTool.SetTile(c + new Vector3(-1,1,0),TileType.瓦片0);
        _spriteRenderer.sprite = DoubleTileToolTileGenerate.GetSprite(new TileTypeGroup()
        {
            LeftBottom = TileType.瓦片0,
            RightTop  = TileType.瓦片0
        },0);
    }

}
