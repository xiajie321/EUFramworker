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
        private static Dictionary<TileKey, TileType> _tileData = new();
        private static event Action<TileChangeData> _onTileChangeEvent;
        private static Tilemap _tagGrid;
        private static Tilemap _viewGrid;
        public static Tilemap TileGrid => _tagGrid;
        public static Tilemap ViewGrid => _viewGrid;
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
                Object.DestroyImmediate(root);
            }
            NativeInit();
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

        private static void NativeInit()
        {
            _ls = new(4,Allocator.Persistent);
            _ls2 = new(4,Allocator.Persistent);
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
                _onTileChangeEvent?.Invoke(_lsChangeData);
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
            _onTileChangeEvent?.Invoke(_lsChangeData);
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

            #region 数组新建与重建

            if (lsKTileData == null)
            {
                lsKTileData = new Vector3Int[positions.Length * 4];
            }
            else if (lsKTileData.Length != positions.Length * 4)
            {
                lsKTileData = new Vector3Int[positions.Length * 4];
            }
            if (lsVTileData == null)
            {
                lsVTileData = new TileBase[positions.Length * 4];
            }
            else if (lsVTileData.Length != positions.Length * 4)
            {
                lsVTileData = new TileBase[positions.Length * 4];
            }

            #endregion

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
                TagCellToViewCell(cellPosition,ref _ls2);
                var type = new TileTypeGroup();
                for (int j = 0;j<_ls2.Length;j++)
                {
                    ViewCellToTagCell(_ls2[j],ref _ls);
                    for (int k = 0; k < 4; k++)
                    {
                        type.SetTileType(k, GetTile(_ls[k]));
                    }
                    //Debug.Log($"{j} {type.LeftTop} {type.RightTop} {type.LeftBottom} {type.RightBottom}");

                    lsKTileData[sum] = new Vector3Int(_ls2[j].x-1, _ls2[j].y-1, _ls2[j].z);
                    lsVTileData[sum] = DoubleTileToolTileGenerate.GetTileBase(type);
                    sum++;
                }
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

            #region 数组新建与重建
            if (lsKTileData == null)
            {
                lsKTileData = new Vector3Int[positions.Length * 4];
            }
            else if (lsKTileData.Length != positions.Length * 4)
            {
                lsKTileData = new Vector3Int[positions.Length * 4];
            }
            if (lsVTileData == null)
            {
                lsVTileData = new TileBase[positions.Length * 4];
            }
            else if (lsVTileData.Length != positions.Length * 4)
            {
                lsVTileData = new TileBase[positions.Length * 4];
            }
            if (_uninstallTiles == null)
            {
                _uninstallTiles = new TileBase[positions.Length];
            }
            else if (_uninstallTiles.Length != positions.Length)
            {
                _uninstallTiles = new TileBase[positions.Length];
            }

            #endregion

            int sum = 0;
            Vector3Int cellPosition;
            for (int i =0;i<positions.Length;i++)
            {
                cellPosition = _tagGrid.WorldToCell(positions[i]);
                TagCellToViewCell(cellPosition,ref _ls2);
                var type = new TileTypeGroup();
                for (int j=0;j<_ls2.Length;j++)
                {
                    ViewCellToTagCell(_ls2[j],ref _ls);
                    for (int k = 0; k < 4; k++)
                    {
                        type.SetTileType(k, GetTile(_ls[k]));
                    }

                    lsKTileData[sum] = new Vector3Int(_ls2[j].x-1,_ls2[j].y -1, _ls2[j].z);
                    lsVTileData[sum] = null;
                    sum++;
                }
                _uninstallTiles[i] = null;
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
            var lsString = tileType.PoolToString();
            ls.sprite = _doubleTileViewConfig.ConfigData.TileDatas.Find(v => v.TileName.Equals(lsString))
                .TagTexture;
            _tagTiles.Add(tileType, ls);
            return ls;
        }

        public static TileBase[] GetTagTiles(TileType[] tileTypes)
        {
            TileBase[] tiles = new TileBase[tileTypes.Length];
            for (int i =0;i<tileTypes.Length;i++)
            {
                tiles[tileTypes[i].GetHashCode()] = GetTagTile(tileTypes[i]);
            }

            return tiles;
        }

        private static NativeArray<Vector3Int> _ls;
        private static NativeArray<Vector3Int> _ls2;

        private static void TileChange(Vector3Int position, TileType tileType)
        {
            //Debug.Log(position);
            TagCellToViewCell(position, ref _ls2);
            for (int i =0 ;i<_ls2.Length;i++)
            {
                var type = new TileTypeGroup();
                //Debug.Log(i);
                ViewCellToTagCell(_ls2[i],ref _ls);
                for (int j = 0; j < 4; j++)
                {
                    //Debug.Log($"{ls[j]} {GetTile(ls[j])}");
                    type.SetTileType(j, GetTile(_ls[j]));
                }

                //Debug.Log($"{type.LeftTop} {type.RightTop} {type.LeftBottom} {type.RightBottom} ");
                if (_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                    _viewGrid.SetTile(new Vector3Int(_ls2[i].x-1, _ls2[i].y-1, _ls2[i].z), DoubleTileToolTileGenerate.GetTileBase(type));
            }

            //更改瓦片
            if (_showTagGrid) //渲染标记网格
            {
                _tagGrid.SetTile(position, GetTagTile(tileType));
            }
        }

        /// <summary>
        /// 注册瓦片改变事件(SetTiles更改不会触发)注意,要使用具体方法而非匿名方法否则无法注销
        /// </summary>
        /// <param name="action"></param>
        public static void RegisterTileChangeEvent(Action<TileChangeData> action)
        {
            _onTileChangeEvent += action;
        }
        /// <summary>
        /// 注销瓦片改变事件(SetTiles更改不会触发)注意,要使用具体方法而非匿名方法否则无法注销
        /// </summary>
        /// <param name="action"></param>
        public static void UnRegisterTileChangeEvent(Action<TileChangeData> action)
        {
            _onTileChangeEvent -= action;
        }


        /// <summary>
        /// 获取受影响的位置(Native容器!一定要记得释放!)
        /// </summary>
        /// <param name="cellPosition">上左,上右,下左,下右</param>
        /// <returns></returns>
        public static void TagCellToViewCell(Vector3Int cellPosition,ref NativeArray<Vector3Int> nativeArray)
        {
            nativeArray[0] = new Vector3Int(cellPosition.x, cellPosition.y+1,cellPosition.z);
            nativeArray[1] = new Vector3Int(cellPosition.x+1, cellPosition.y+1,cellPosition.z);
            nativeArray[2] = cellPosition;
            nativeArray[3] = new Vector3Int(cellPosition.x+1, cellPosition.y,cellPosition.z);
        }

        /// <summary>
        /// 获取受影响的位置(Native容器!一定要记得释放!)
        /// </summary>
        /// <param name="cellPosition">上左,上右,下左,下右</param>
        /// <returns></returns>
        public static void ViewCellToTagCell(Vector3Int cellPosition,ref NativeArray<Vector3Int> nativeArray)
        {
            nativeArray[0] =  new Vector3Int(cellPosition.x-1, cellPosition.y, cellPosition.z);
            nativeArray[1] = cellPosition;
            nativeArray[2] = new Vector3Int(cellPosition.x-1, cellPosition.y-1, cellPosition.z);
            nativeArray[3] = new Vector3Int(cellPosition.x, cellPosition.y-1, cellPosition.z);
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
            TagCellToViewCell(cellPosition,ref _ls2);
            for (int i=0;i<_ls2.Length;i++)
            {
                if (_doubleTileViewConfig.ConfigData.TileObjectType == TileObjectType.Sprite)
                    _viewGrid.SetTile(new Vector3Int(_ls2[i].x-1,_ls2[i].y -1,_ls2[i].z), null);
            }
            
        }

        public static TileType GetTile(Vector3 position)
        {
            if (_tileData.TryGetValue(_tagGrid.WorldToCell(position), out var value))
            {
                return value;
            }
            return 0;
        }

        public static TileType GetTile(Vector3Int position)
        {
            if (_tileData.TryGetValue(position, out var value))
            {
                return value;
            }
            return 0;
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

        public static void Dispose()
        {
            if(_ls.IsCreated) _ls.Dispose();
            if(_ls2.IsCreated)_ls2.Dispose();
            _onTileChangeEvent =  null;
        }
    }

    public static class TileTypeExpand
    {
        private static readonly Dictionary<TileType, string> _stringPool = new();
        /// <summary>
        /// 使用字典缓存以减少ToString带来的内存开销
        /// </summary>
        /// <param name="tileType"></param>
        /// <returns></returns>
        public static string PoolToString(this TileType tileType)
        {
            if (_stringPool.TryGetValue(tileType, out var str))
            {
                return str;
            }
            _stringPool.Add(tileType, tileType.ToString());
            return _stringPool[tileType];
        }
    }
    
    public struct TileChangeData
    {
        public Vector3Int Position;
        public TileType OldTileType;
        public TileType NewTileType;
    }

    public struct TileKey : IEquatable<TileKey>
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;
    
        public TileKey(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    
        public TileKey(Vector3Int vec)
        {
            this.x = vec.x;
            this.y = vec.y;
            this.z = vec.z;
        }
    
        // 隐式转换：TileKey → Vector3Int
        public static implicit operator Vector3Int(TileKey key)
        {
            return new Vector3Int(key.x, key.y, key.z);
        }
    
        // 隐式转换：Vector3Int → TileKey
        public static implicit operator TileKey(Vector3Int vec)
        {
            return new TileKey(vec.x, vec.y, vec.z);
        }
    
        // 显式转换（如果需要）
        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, y, z);
        }
    
        // 直接比较，避免类型检查和装箱
        public bool Equals(TileKey other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    
        // 优化的GetHashCode
        public override int GetHashCode()
        {
            // 使用素数减少哈希冲突
            unchecked
            {
                int hash = x;
                hash = (hash * 397) ^ y;
                hash = (hash * 397) ^ z;
                return hash;
            }
        }
    
        // 避免使用object.Equals
        public override bool Equals(object obj) => false;
    
        // 重写ToString以便调试
        public override string ToString()
        {
            return $"TileKey({x}, {y}, {z})";
        }
    
        // 为了方便，添加一些常用属性
        public int Magnitude => Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
    
        public static TileKey zero => new TileKey(0, 0, 0);
        public static TileKey one => new TileKey(1, 1, 1);
    }
}