using System;
using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.Script
{
    public class DoubleTileTool
    {
        private static readonly Dictionary<Vector3Int,TileType> _tileData = new();

        private static event Action<Vector3Int, TileType> _onTileChangeEvent;
        public static Tilemap _tagGrid;
        public static Tilemap _viewGrid;
        static bool _showTagGrid;
        static SODoubleTileViewConfig _doubleTileViewConfig;
        public static void Init()//初始化瓦片工具
        {
            if (_tagGrid || _viewGrid)
            {
                Debug.LogWarning("[DoubleTileTool] 重复初始化!");
                return;
            }
            GameObject root = Object.Instantiate(Resources.Load<GameObject>("EUFarmworker/DoubleTileTool/DoubleTileTool"));
            _doubleTileViewConfig = root.GetComponent<DoubleTileToolRunTimeMono>().Config;
            root.transform.position = new Vector3(0,0,0);
            
            GameObject grid = new GameObject("Grid");
            grid.transform.SetParent(root.transform);
            grid.AddComponent<Grid>();
            grid.transform.position = new Vector3(0,0,0);
            
            GameObject viewGrid = new GameObject("ViewGrid");//显示
            _viewGrid = viewGrid.AddComponent<Tilemap>();
            viewGrid.AddComponent<TilemapRenderer>();
            viewGrid.transform.SetParent(grid.transform);
            _viewGrid.transform.position = new Vector2(0.5f, 0.5f);//偏移
            
            GameObject tagGrid = new GameObject("TagGrid");//标记
            _tagGrid = tagGrid.AddComponent<Tilemap>();
            tagGrid.AddComponent<TilemapRenderer>();
            tagGrid.transform.SetParent(grid.transform);
            _tagGrid.transform.position = new Vector3(0,0,0);
            tagGrid.SetActive(false);
            DoubleTileToolTileGenerate.Init(_doubleTileViewConfig.ConfigData);
        }
        /// <summary>
        /// 显示标记网格
        /// </summary>
        /// <param name="show">是否显示</param>
        public static void ShowTagGrid(bool show)
        {
            if(_showTagGrid == show) return;
            _showTagGrid = show;
            _tagGrid.gameObject.SetActive(show);
            foreach (var i in _tileData)
            {
                _tagGrid.SetTile(i.Key,GetTagTile(i.Value));
            }
            //这里要补充标记网格的显示逻辑
        }
        /// <summary>
        /// 设置瓦片
        /// </summary>
        /// <param name="position">世界坐标</param>
        /// <param name="tileType">瓦片类型</param>
        public static void SetTile(Vector3 position,TileType tileType)
        {
            Vector3Int cellPosition = _tagGrid.WorldToCell(position);
            if (!_tileData.ContainsKey(cellPosition))
            {
                _tileData.Add(cellPosition,tileType);
                TileChange(cellPosition,tileType);
                return;
            }
            if(_tileData[cellPosition] == tileType)
                return;
            _tileData[cellPosition] = tileType;
            TileChange(cellPosition,tileType);
        }
        private static readonly Dictionary<TileType,Tile> _tagTiles = new();
        /// <summary>
        /// 获取标记的瓦片
        /// </summary>
        /// <param name="tileType"></param>
        /// <returns></returns>
        public static Tile GetTagTile(TileType tileType)
        {
            if (_tagTiles.ContainsKey(tileType))
            {
                return _tagTiles[tileType];
            }
            var ls = ScriptableObject.CreateInstance<Tile>();
            ls.sprite = _doubleTileViewConfig.ConfigData.TileDatas.Find(v => v.TileName.Equals(tileType.ToString())).TagTexture;
            _tagTiles.Add(tileType,ls);
            return ls;
        }
        private static void TileChange(Vector3Int position, TileType tileType)
        {
            //Debug.Log(position);
            foreach (var i in TagCellToViewCell(position))
            {
                var type = new TileTypeGroup();
                //Debug.Log(i);
                List<Vector3Int> ls = ViewCellToTagCell(i);
                for(int j = 0;j<4;j++)
                {
                    //Debug.Log($"{ls[j]} {GetTile(ls[j])}");
                    type.SetTileType(j,GetTile(ls[j]));
                }
                //Debug.Log($"{type.LeftTop} {type.RightTop} {type.LeftBottom} {type.RightBottom} ");
                if(_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                    _viewGrid.SetTile(i+new Vector3Int(-1,-1,0),DoubleTileToolTileGenerate.GetTileBase(type));
            }
            //更改瓦片
            if (_showTagGrid)//渲染标记网格
            { 
                _tagGrid.SetTile(position,GetTagTile(tileType));
            }
            _onTileChangeEvent?.Invoke(position,tileType);
        }
        /// <summary>
        /// 注册瓦片改变事件
        /// </summary>
        /// <param name="action"></param>
        public static void RegisterTileChangeEvent(Action<Vector3Int, TileType> action)
        {
            _onTileChangeEvent += action;
        }
        /// <summary>
        /// 获取受影响的位置
        /// </summary>
        /// <param name="cellPosition">上左,上右,下左,下右</param>
        /// <returns></returns>
        public static List<Vector3Int>TagCellToViewCell(Vector3Int cellPosition)
        {
            List<Vector3Int> ls = new();
            ls.Add(cellPosition + new Vector3Int(0,1,0));
            ls.Add(cellPosition + new  Vector3Int(1,1,0));
            ls.Add(cellPosition);
            ls.Add(cellPosition +  new  Vector3Int(1,0,0));
            return ls;
        }

        /// <summary>
        /// 获取受影响的位置
        /// </summary>
        /// <param name="cellPosition">上左,上右,下左,下右</param>
        /// <returns></returns>
        public static List<Vector3Int> ViewCellToTagCell(Vector3Int cellPosition)
        {
            List<Vector3Int> ls = new();
            ls.Add(cellPosition + new Vector3Int(-1,0,0));
            ls.Add(cellPosition);
            ls.Add(cellPosition + new Vector3Int(-1,-1,0));
            ls.Add(cellPosition + new Vector3Int(0,-1,0));
            return ls;
        }
        public static void RemoveTile(Vector3 position)
        {
            Vector3Int cellPosition = _tagGrid.WorldToCell(position);
            if(!_tileData.ContainsKey(cellPosition)) return;
            _tagGrid.SetTile(cellPosition,null);
            _tileData.Remove(cellPosition);
        }
        
        public static TileType GetTile(Vector3 position)
        {
            return _tileData.GetValueOrDefault(_tagGrid.WorldToCell(position));
        }

        public static TileType GetTile(Vector3Int position)
        {
            return _tileData.GetValueOrDefault(position);
        }
        
        /// <summary>
        /// 清除所有瓦片
        /// </summary>
        public static void Clear()
        {
            _tileData.Clear();
            _tagGrid.ClearAllTiles();
            _viewGrid.ClearAllTiles();
        }
        /// <summary>
        /// 释放缓存(不会导致瓦片被清除,清除场景中的瓦片应使用Clear)
        /// </summary>
        public static void Release()
        {
            DoubleTileToolTileGenerate.Release();
            _tagTiles.Clear();
        }
    }
}
