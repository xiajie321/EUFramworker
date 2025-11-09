using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.Script.Data
{
    [Serializable]
    public class DoubleTileData
    {
        public string TileName;//瓦片名称
        public Sprite TagTexture;//用于标记的瓦片纹理
        public TileObjectType TileObjectType = TileObjectType.Sprite;
        public bool IsDynamic =false;
        [SerializeField]
        public List<DoubleTileDataItem> ObjectList = new();//动态瓦片每帧的纹理s
    }

    [Serializable]
    public class DoubleTileDataItem
    {
        private int _count;
        public DoubleTileDataItem(int count = 5)
        {
            _count = count;
            Objects = new(_count);
            for (int i = 0; i < _count; i++)
            {
                Objects.Add(null);
            }
        }
        public List<Object> Objects;
    }
    public enum TileObjectType
    {
        Sprite,
        GameObject,
    }
}