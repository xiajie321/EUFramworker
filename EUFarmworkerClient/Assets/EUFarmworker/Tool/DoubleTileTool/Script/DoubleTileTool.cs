using System;
using System.Collections.Generic;
using System.Linq;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using ZLinq;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.Script
{
    public  static class DoubleTileTool
    {
        private static readonly Dictionary<Vector3Int, TileType> _tileData = new();
        
        public static Tilemap _tagGrid;
        public static Tilemap _viewGrid;
        static bool _showTagGrid;
        static SODoubleTileViewConfig _doubleTileViewConfig;
        private static GameObject root;
        /// <summary>
        /// 每次重新进入游戏场景时都需要重新初始化一次
        /// </summary>
        public static void Init() //初始化瓦片工具
        {
            Clear();
            if (root)
            {
                Debug.LogWarning("[DoubleTileTool] 重复初始化!");
                return;
            }

            root = Object.Instantiate(Resources.Load<GameObject>("EUFarmworker/DoubleTileTool/DoubleTileTool"));
            _doubleTileViewConfig = root.GetComponent<DoubleTileToolRunTimeMono>().Config;
            root.transform.position = new Vector3(0, 0, 0);

            GameObject grid = new GameObject("Grid");
            grid.transform.SetParent(root.transform);
            grid.AddComponent<Grid>();
            grid.transform.position = new Vector3(0, 0, 0);

            GameObject viewGrid = new GameObject("ViewGrid"); //显示
            _viewGrid = viewGrid.AddComponent<Tilemap>();
            viewGrid.AddComponent<TilemapRenderer>();
            viewGrid.transform.SetParent(grid.transform);
            _viewGrid.transform.position = new Vector2(0.5f, 0.5f); //偏移

            GameObject tagGrid = new GameObject("TagGrid"); //标记
            _tagGrid = tagGrid.AddComponent<Tilemap>();
            tagGrid.AddComponent<TilemapRenderer>();
            tagGrid.transform.SetParent(grid.transform);
            _tagGrid.transform.position = new Vector3(0, 0, 0);
            tagGrid.SetActive(false);
            DoubleTileToolTileGenerate.Init(_doubleTileViewConfig.ConfigData);
        }

        /// <summary>
        /// 显示标记网格
        /// </summary>
        /// <param name="show">是否显示</param>
        public static void ShowTagGrid(bool show)
        {
            if (_showTagGrid == show) return;
            _showTagGrid = show;
            _tagGrid.gameObject.SetActive(show);
            foreach (var i in _tileData)
            {
                _tagGrid.SetTile(i.Key, GetTagTile(i.Value));
            }
            //这里要补充标记网格的显示逻辑
        }

        private static TileChangeData _lsChangeData = new();

        /// <summary>
        /// 设置瓦片
        /// </summary>
        /// <param name="position">世界坐标</param>
        /// <param name="tileType">瓦片类型</param>
        public static void SetTile(Vector3 position, TileType tileType)
        {
            if (!_tagGrid || !_viewGrid) return;
            Vector3Int cellPosition = _tagGrid.WorldToCell(position);
            _lsChangeData.Position = cellPosition;
            if (!_tileData.TryGetValue(cellPosition, out var value))
            {
                _lsChangeData.OldTileType = default;
                _lsChangeData.NewTileType = tileType;
                _tileData.Add(cellPosition, tileType);
                TileChange(cellPosition, tileType);
                return;
            }

            if (value == tileType)
            {
                LoadTile(cellPosition);
                return;
            }

            _lsChangeData.OldTileType = default;
            _lsChangeData.NewTileType = tileType;
            _tileData[cellPosition] = tileType;
            TileChange(cellPosition, tileType);
        }
        
        private static Vector3Int[] lsKTileData;
        private static TileBase[] lsVTileData;
        public static void SetTiles(Vector3Int[] positions, TileType[] tileTypes)
        {
            if (!_tagGrid || !_viewGrid) return;
            if (positions.Length != tileTypes.Length)
            {
                Debug.LogError("[DoubleTileTool] 数组长度不一致");
                return;
            }
            lsKTileData = new Vector3Int[positions.Length * 4];
            lsVTileData = new TileBase[positions.Length * 4];
            Vector3Int cellPosition;
            int sum = 0;
            for (int i =0;i<positions.Length;i++)
            {
                cellPosition = _tagGrid.WorldToCell(positions[i]);
                if(_tileData.ContainsKey(cellPosition))
                {
                    _tileData[cellPosition] = tileTypes[i];
                }
                else
                {
                    _tileData.Add(cellPosition,tileTypes[i]);
                }
                var lsbl1 = TagCellToViewCell(cellPosition);
                var type = new TileTypeGroup();
                foreach (var j in lsbl1)
                {
                    _ls = ViewCellToTagCell(j);
                    for (int k = 0; k < 4; k++)
                    {
                        type.SetTileType(k, GetTile(_ls[k]));
                    }
                    //Debug.Log($"{j} {type.LeftTop} {type.RightTop} {type.LeftBottom} {type.RightBottom}");

                    lsKTileData[sum] = j + new Vector3Int(-1, -1, 0);
                    lsVTileData[sum] = DoubleTileToolTileGenerate.GetTileBase(type);
                    sum++;
                    if (_ls.IsCreated) _ls.Dispose();
                }

                if (lsbl1.IsCreated) lsbl1.Dispose();
            }
            if (_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                _viewGrid.SetTiles(lsKTileData,lsVTileData);
            if (_showTagGrid) //渲染标记网格
            {
                _tagGrid.SetTiles(positions, GetTagTiles(tileTypes));
            }
        }
        private static TileBase[] _uninstallTiles;
        public static void UninstallTiles(Vector3Int[] positions)
        {
            if (!_tagGrid || !_viewGrid) return;
            lsKTileData = new Vector3Int[positions.Length * 4];
            lsVTileData = new TileBase[positions.Length * 4];
            int sum = 0;
            _uninstallTiles = new TileBase[positions.Length];
            Vector3Int cellPosition;
            for (int i =0;i<positions.Length;i++)
            {
                cellPosition = _tagGrid.WorldToCell(positions[i]);
                var lsbl1 = TagCellToViewCell(cellPosition);
                var type = new TileTypeGroup();
                foreach (var j in lsbl1)
                {
                    _ls = ViewCellToTagCell(j);
                    for (int k = 0; k < 4; k++)
                    {
                        type.SetTileType(k, GetTile(_ls[k]));
                    }

                    lsKTileData[sum] = j + new Vector3Int(-1, -1, 0);
                    lsVTileData[sum] = null;
                    sum++;
                    if (_ls.IsCreated) _ls.Dispose();
                }
                _uninstallTiles[i] = null;
                if (lsbl1.IsCreated) lsbl1.Dispose();
            }

            if (_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                _viewGrid.SetTiles(lsKTileData,lsVTileData);
            if (_showTagGrid) //渲染标记网格
            {
                _tagGrid.SetTiles(positions,_uninstallTiles);
            }
        }

        private static readonly Dictionary<TileType, Tile> _tagTiles = new();

        /// <summary>
        /// 获取标记的瓦片
        /// </summary>
        /// <param name="tileType"></param>
        /// <returns></returns>
        public static Tile GetTagTile(TileType tileType)
        {
            if (_tagTiles.TryGetValue(tileType, out var tile))
            {
                return tile;
            }

            var ls = ScriptableObject.CreateInstance<Tile>();
            var lsString = tileType.ToString();
            ls.sprite = _doubleTileViewConfig.ConfigData.TileDatas.Find(v => v.TileName.Equals(lsString))
                .TagTexture;
            _tagTiles.Add(tileType, ls);
            return ls;
        }

        public static TileBase[] GetTagTiles(TileType[] tileTypes)
        {
            TileBase[] tiles = new TileBase[tileTypes.Length];
            foreach (var tileType in tileTypes)
            {
                tiles[tileType.GetHashCode()] = GetTagTile(tileType);
            }

            return tiles;
        }

        private static NativeArray<Vector3Int> _ls;

        private static void TileChange(Vector3Int position, TileType tileType)
        {
            //Debug.Log(position);
            var lsbl = TagCellToViewCell(position);
            foreach (var i in lsbl)
            {
                var type = new TileTypeGroup();
                //Debug.Log(i);
                _ls = ViewCellToTagCell(i);
                for (int j = 0; j < 4; j++)
                {
                    //Debug.Log($"{ls[j]} {GetTile(ls[j])}");
                    type.SetTileType(j, GetTile(_ls[j]));
                }

                //Debug.Log($"{type.LeftTop} {type.RightTop} {type.LeftBottom} {type.RightBottom} ");
                if (_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                    _viewGrid.SetTile(i + new Vector3Int(-1, -1, 0), DoubleTileToolTileGenerate.GetTileBase(type));
                if (_ls.IsCreated) _ls.Dispose();
            }

            //更改瓦片
            if (_showTagGrid) //渲染标记网格
            {
                _tagGrid.SetTile(position, GetTagTile(tileType));
            }

            if (lsbl.IsCreated) lsbl.Dispose();
        }

        /// <summary>
        /// 注册瓦片改变事件(暂时不支持)
        /// </summary>
        /// <param name="action"></param>
        public static void RegisterTileChangeEvent(Action<TileChangeData> action)
        {
        }

        /// <summary>
        /// 获取受影响的位置(Native容器!一定要记得释放!)
        /// </summary>
        /// <param name="cellPosition">上左,上右,下左,下右</param>
        /// <returns></returns>
        public static NativeArray<Vector3Int> TagCellToViewCell(Vector3Int cellPosition)
        {
            NativeArray<Vector3Int> ls = new(4, Allocator.Temp);
            ls[0] = cellPosition + new Vector3Int(0, 1, 0);
            ls[1] = cellPosition + new Vector3Int(1, 1, 0);
            ls[2] = cellPosition;
            ls[3] = cellPosition + new Vector3Int(1, 0, 0);
            return ls;
        }

        /// <summary>
        /// 获取受影响的位置(Native容器!一定要记得释放!)
        /// </summary>
        /// <param name="cellPosition">上左,上右,下左,下右</param>
        /// <returns></returns>
        public static NativeArray<Vector3Int> ViewCellToTagCell(Vector3Int cellPosition)
        {
            NativeArray<Vector3Int> ls = new(4, Allocator.Temp);
            ls[0] = cellPosition + new Vector3Int(-1, 0, 0);
            ls[1] = cellPosition;
            ls[2] = cellPosition + new Vector3Int(-1, -1, 0);
            ls[3] = cellPosition + new Vector3Int(0, -1, 0);
            return ls;
        }

        /// <summary>
        /// 渲染指定位置的瓦片
        /// </summary>
        /// <param name="position"></param>
        public static void LoadTile(Vector3 position)
        {
            Vector3Int cellPosition = _tagGrid.WorldToCell(position);
            if (_tileData.TryGetValue(cellPosition, out var value))
            {
                TileChange(cellPosition, value);
                return;
            }

            TileChange(cellPosition, default);
        }

        /// <summary>
        /// 卸载指定位置的瓦片
        /// </summary>
        /// <param name="position"></param>
        public static void UninstallTile(Vector3 position)
        {
            Vector3Int cellPosition = _tagGrid.WorldToCell(position);
            _tagGrid.SetTile(cellPosition, null);
            var lsbl = TagCellToViewCell(cellPosition);
            foreach (var i in lsbl)
            {
                if (_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                    _viewGrid.SetTile(i + new Vector3Int(-1, -1, 0), null);
            }

            if (lsbl.IsCreated) lsbl.Dispose();
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
        /// 清除所有瓦片(初始化的时候会执行一次,但如果需要游戏场景中重新绘制仍然可以调用)
        /// </summary>
        public static void Clear()
        {
            _tileData.Clear();
            _tagGrid?.ClearAllTiles();
            _viewGrid?.ClearAllTiles();
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

    public struct TileChangeData
    {
        public Vector3Int Position;
        public TileType OldTileType;
        public TileType NewTileType;
    }
}