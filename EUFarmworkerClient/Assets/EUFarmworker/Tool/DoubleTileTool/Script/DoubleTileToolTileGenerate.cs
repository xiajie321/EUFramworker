using System;
using System.Collections.Generic;
using System.Linq;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EUFarmworker.Tool.DoubleTileTool.Script
{
    /// <summary>
    /// 瓦片纹理生成器(需要Init时传入数据源才可以使用)
    /// </summary>
    public static class DoubleTileToolTileGenerate
    {
        // 假设TileType是0-15的枚举（16种类型）
        private static int MAX_TILE_TYPES = 16;
        private static int ARRAY_SIZE = MAX_TILE_TYPES * MAX_TILE_TYPES * MAX_TILE_TYPES * MAX_TILE_TYPES; // 16^4 = 65536
        
        // 使用数组替代所有字典
        private static Texture2D[][] _tileGroupTexture2DResources;
        private static TileBase[] _tileBases;
        private static Sprite[][] _tileSprites;
        private static bool[] _dynamicTileTypes;
        private static List<List<Sprite>>[] _sprites;
        
        private static SODoubleTileScriptableObject _doubleTileViewConfig;

        public static void Init(SODoubleTileScriptableObject data)
        {
            _doubleTileViewConfig = data;
            MAX_TILE_TYPES = data.TileNames.Count;
            ARRAY_SIZE = MAX_TILE_TYPES * MAX_TILE_TYPES * MAX_TILE_TYPES * MAX_TILE_TYPES; 
            _tileGroupTexture2DResources = new Texture2D[ARRAY_SIZE][];
            _tileBases = new TileBase[ARRAY_SIZE];
            _tileSprites = new Sprite[ARRAY_SIZE][];
            _dynamicTileTypes = new bool[MAX_TILE_TYPES];
            _sprites = new List<List<Sprite>>[MAX_TILE_TYPES];
            // 初始化数组
            for (int i = 0; i < ARRAY_SIZE; i++)
            {
                _tileSprites[i] = new Sprite[_doubleTileViewConfig.Frame];
            }
            
            // 预缓存动态类型标记
            foreach (var tileData in _doubleTileViewConfig.TileDatas)
            {
                if (Enum.TryParse<TileType>(tileData.TileName, out TileType tileType))
                {
                    int typeIndex = (int)tileType;
                    if (typeIndex < MAX_TILE_TYPES)
                    {
                        _dynamicTileTypes[typeIndex] = tileData.IsDynamic;
                    }
                }
            }
        }

        /// <summary>
        /// 纹理生成
        /// </summary>
        /// <param name="tileTypeGroup">生成器</param>
        public static List<Texture2D> GenerateTexture2D(TileTypeGroup tileTypeGroup)
        {
            int index = TileTypeGroupToIndex(tileTypeGroup);
            
            if (_tileGroupTexture2DResources[index] != null)
                return _tileGroupTexture2DResources[index].ToList();

            //生成
            if (tileTypeGroup.IsDynamicTile()) //动态瓦片
            {
                _tileGroupTexture2DResources[index] = DynamicTexture2D(tileTypeGroup).ToArray();
                return _tileGroupTexture2DResources[index].ToList();
            }

            _tileGroupTexture2DResources[index] = new Texture2D[] { StaticTexture2D(tileTypeGroup, GetTileTypeGroupIndexs(tileTypeGroup)) };
            return _tileGroupTexture2DResources[index].ToList();
        }

        /// <summary>
        /// 获取瓦片
        /// </summary>
        /// <param name="typeGroup"></param>
        /// <returns></returns>
        public static TileBase GetTileBase(TileTypeGroup typeGroup)
        {
            int index = TileTypeGroupToIndex(typeGroup);
            
            if (_tileBases[index] != null)
            {
                return _tileBases[index];
            }

            if (typeGroup.IsDynamicTile())
            {
                Sprite[] ls = new Sprite[_doubleTileViewConfig.Frame];
                for (int i = 0; i < ls.Length; i++)
                {
                    ls[i] = GetSprite(typeGroup, i);
                }
                AnimatedTile a = ScriptableObject.CreateInstance<AnimatedTile>();
                a.m_AnimatedSprites = ls;
                a.m_AnimationStartFrame = _doubleTileViewConfig.Frame;
                _tileBases[index] = a;
                return a;
            }

            Tile b = ScriptableObject.CreateInstance<Tile>();
            b.sprite = GetSprite(typeGroup);
            _tileBases[index] = b;
            return b;
        }

        /// <summary>
        /// 获取精灵
        /// </summary>
        /// <param name="tileTypeGroup"></param>
        /// <param name="index">是动态的话可以通过该下标直接获取动画帧的精灵</param>
        /// <returns></returns>
        public static Sprite GetSprite(TileTypeGroup tileTypeGroup, int index = 0)
        {
            int arrayIndex = TileTypeGroupToIndex(tileTypeGroup);
            
            if (_tileSprites[arrayIndex] != null && 
                index < _tileSprites[arrayIndex].Length && 
                _tileSprites[arrayIndex][index] != null)
            {
                return _tileSprites[arrayIndex][index];
            }

            var texture = GenerateTexture2D(tileTypeGroup)[index];
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            _tileSprites[arrayIndex][index] = sprite;
            return sprite;
        }

        #region 动态瓦片标记

        public static bool IsDynamicTile(this TileTypeGroup tileTypeGroup)
        {
            return TagDynamicTexture2D(tileTypeGroup).IsDynamicTileTypeGroup();
        }

        //存在动态瓦片
        private static bool IsDynamicTileTypeGroup(this TagDynamicTileTypeGroup tileTypeGroup)
        {
            return tileTypeGroup.LeftBottom || tileTypeGroup.RightBottom || tileTypeGroup.LeftTop ||
                   tileTypeGroup.RightTop;
        }

        //标记动态的瓦片类型
        private static TagDynamicTileTypeGroup TagDynamicTexture2D(TileTypeGroup tileTypeGroup)
        {
            return new TagDynamicTileTypeGroup()
            {
                LeftTop = tileTypeGroup.LeftTop.IsDynamicTileTypeGroup(),
                LeftBottom = tileTypeGroup.LeftBottom.IsDynamicTileTypeGroup(),
                RightTop = tileTypeGroup.RightTop.IsDynamicTileTypeGroup(),
                RightBottom = tileTypeGroup.RightBottom.IsDynamicTileTypeGroup(),
            };
        }

        //拓展方法用于判断瓦片类型是否为动态
        private static bool IsDynamicTileTypeGroup(this TileType tileTypeGroup)
        {
            int typeIndex = (int)tileTypeGroup;
            return typeIndex < MAX_TILE_TYPES && _dynamicTileTypes[typeIndex];
        }

        #endregion

        /// <summary>
        /// 生成动态纹理
        /// </summary>
        /// <param name="tileTypeGroup"></param>
        /// <returns></returns>
        private static List<Texture2D> DynamicTexture2D(TileTypeGroup tileTypeGroup)
        {
            var ls = GetTileTypeGroupIndexs(tileTypeGroup);
            List<Texture2D> lstex = new();
            for (int i = 0; i < _doubleTileViewConfig.Frame; i++)
            {
                lstex.Add(StaticTexture2D(tileTypeGroup, ls, i));
            }
            return lstex;
        }

        private static Texture2D StaticTexture2D(TileTypeGroup tileTypeGroup, int data, int index = 0)
        {
            var ls = data;
            Texture2D lstex = null;

            switch (ls)
            {
                case 0: //对角线
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightBottom &&
                        tileTypeGroup.RightTop == tileTypeGroup.LeftBottom)
                    {
                        if (tileTypeGroup.LeftTop > tileTypeGroup.RightTop)
                        {
                            var a = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                            var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                            var c = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                            var ls1 = MergeSprites(a[2], b[0], 0, 270);
                            lstex = MergeSprites(
                                Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                                c[0], 0, 90);
                        }
                        else
                        {
                            var a = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                            var b = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                            var c = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                            var ls1 = MergeSprites(a[2], b[0], 90, 0);
                            lstex = MergeSprites(
                                Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                                c[0], 0, 180);
                        }
                        break;
                    }
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightBottom)
                    {
                        var a = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        var c = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                        var ls1 = MergeSprites(a[2], b[0], 0, 270);
                        lstex = MergeSprites(
                            Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                            c[0], 0, 90);
                    }
                    else if (tileTypeGroup.RightTop == tileTypeGroup.LeftBottom)
                    {
                        var a = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        var b = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var c = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                        var ls1 = MergeSprites(a[2], b[0], 90, 0);
                        lstex = MergeSprites(
                            Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                            c[0], 0, 180);
                    }
                    break;
                case 1: //三个点相同
                    TileType FindUniqueValue(TileTypeGroup group)
                    {
                        if (group.LeftTop == group.RightTop)
                        {
                            if (group.LeftBottom != group.LeftTop)
                                return group.LeftBottom;
                            else
                                return group.RightBottom;
                        }
                        else
                        {
                            if (group.LeftTop == group.LeftBottom)
                                return group.RightTop;
                            else
                                return group.LeftTop;
                        }
                    }

                    var lsType = FindUniqueValue(tileTypeGroup);

                    if (lsType == tileTypeGroup.LeftTop)
                    {
                        var a = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        lstex = MergeSprites(b[3], a[0], 180, 0);
                    }
                    else if (lsType == tileTypeGroup.RightTop)
                    {
                        var a = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        var b = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        lstex = MergeSprites(b[3], a[0], 90, 270);
                    }
                    else if (lsType == tileTypeGroup.RightBottom)
                    {
                        var a = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                        var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        lstex = MergeSprites(b[3], a[0], 0, 180);
                    }
                    else
                    {
                        var a = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                        var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        lstex = MergeSprites(b[3], a[0], 270, 90);
                    }
                    break;
                case 2: //四个点都不同
                    var def = GetSprites((TileType)0)[GetIndex(0, index)];
                    var la = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                    var lsb = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                    var lsc = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                    var lsd = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                    var als1 = MergeSprites(def[4], la[0], 0, 0);
                    als1 = MergeSprites(
                        Sprite.Create(als1, new Rect(0, 0, als1.width, als1.height), new Vector2(0.5f, 0.5f)), lsb[0], 0,
                        90);
                    als1 = MergeSprites(
                        Sprite.Create(als1, new Rect(0, 0, als1.width, als1.height), new Vector2(0.5f, 0.5f)), lsc[0], 0,
                        270);
                    lstex = MergeSprites(
                        Sprite.Create(als1, new Rect(0, 0, als1.width, als1.height), new Vector2(0.5f, 0.5f)), lsd[0], 0,
                        180);
                    break;
                case 3: //水平/垂直对称
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightTop)
                    {
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var a3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                        lstex = MergeSprites(def3[1], a3[1], 270, 90);
                    }
                    else
                    {
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var a3 = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                        lstex = MergeSprites(def3[1], a3[1], 0, 180);
                    }
                    break;
                case 4: //四个点完全相同
                    var def4 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                    lstex = MergeSprites(def4[4], def4[4], 0, 0);
                    break;
                case 5: //两个点水平/垂直对齐
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightTop)
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0, index)];
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var a3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                        var b3 = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 270);
                        ls5 = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), a3[0], 0, 90);
                        lstex = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), b3[0], 0, 180);
                    }
                    else if (tileTypeGroup.LeftBottom == tileTypeGroup.RightBottom)
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0, index)];
                        var def3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                        var a3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var b3 = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 90);
                        ls5 = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), a3[0], 0, 0);
                        lstex = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), b3[0], 0, 270);
                    }
                    else if (tileTypeGroup.LeftTop == tileTypeGroup.LeftBottom)
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0, index)];
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var a3 = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        var b3 = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom, index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 0);
                        ls5 = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), a3[0], 0, 90);
                        lstex = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), b3[0], 0, 180);
                    }
                    else
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0, index)];
                        var def3 = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop, index)];
                        var a3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop, index)];
                        var b3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom, index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 180);
                        ls5 = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), a3[0], 0, 0);
                        lstex = MergeSprites(Sprite.Create(ls5, new Rect(0, 0, ls5.width, ls5.height), new Vector2(0.5f, 0.5f)), b3[0], 0, 90);
                    }
                    break;
            }
            return lstex;
        }

        private static int GetIndex(TileType tileType, int index)
        {
            return tileType.IsDynamicTileTypeGroup() ? index : 0;
        }

        private static List<List<Sprite>> GetSprites(TileType tileType)
        {
            int typeIndex = (int)tileType;
            if (typeIndex < MAX_TILE_TYPES && _sprites[typeIndex] != null)
                return _sprites[typeIndex];

            var ls = _doubleTileViewConfig.TileDatas.Find(v => v.TileName == tileType.ToString()).ObjectList;
            var fh = new List<List<Sprite>>();
            foreach (var i in ls)
            {
                var v = new List<Sprite>();
                foreach (var j in i.Objects)
                {
                    v.Add(j as Sprite);
                }
                fh.Add(v);
            }

            if (typeIndex < MAX_TILE_TYPES)
            {
                _sprites[typeIndex] = fh;
            }
            return fh;
        }
        
        #region 纹理组合
        private static Texture2D MergeSprites(Sprite a, Sprite b, float aRotate, float bRotate)
        {
            Texture2D texA = a.texture;
            Texture2D texB = b.texture;
            Rect rectA = a.rect;
            Rect rectB = b.rect;

            int width = Mathf.Max((int)rectA.width, (int)rectB.width);
            int height = Mathf.Max((int)rectA.height, (int)rectB.height);

            Texture2D mergedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color[] transparentPixels = new Color[width * height];
            for (int i = 0; i < transparentPixels.Length; i++)
            {
                transparentPixels[i] = Color.clear;
            }

            mergedTexture.SetPixels(transparentPixels);
            MergeSpriteWithRotation(mergedTexture, texA, rectA, aRotate);
            MergeSpriteWithRotation(mergedTexture, texB, rectB, bRotate);
            mergedTexture.Apply();
            return mergedTexture;
        }

        private static void MergeSpriteWithRotation(Texture2D target, Texture2D source, Rect spriteRect, float rotation)
        {
            int spriteWidth = (int)spriteRect.width;
            int spriteHeight = (int)spriteRect.height;
            Color[] spritePixels = source.GetPixels((int)spriteRect.x, (int)spriteRect.y, spriteWidth, spriteHeight);
            Color[] rotatedPixels = RotatePixels(spritePixels, spriteWidth, spriteHeight, rotation);
            Vector2Int rotatedSize = GetRotatedSize(spriteWidth, spriteHeight, rotation);
            int rotatedWidth = rotatedSize.x;
            int rotatedHeight = rotatedSize.y;
            int startX = (target.width - rotatedWidth) / 2;
            int startY = (target.height - rotatedHeight) / 2;

            for (int y = 0; y < rotatedHeight; y++)
            {
                for (int x = 0; x < rotatedWidth; x++)
                {
                    int sourceIndex = y * rotatedWidth + x;
                    Color pixel = rotatedPixels[sourceIndex];
                    if (pixel.a > 0.01f)
                    {
                        int targetX = startX + x;
                        int targetY = startY + y;
                        if (targetX >= 0 && targetX < target.width && targetY >= 0 && targetY < target.height)
                        {
                            Color existingPixel = target.GetPixel(targetX, targetY);
                            Color blendedPixel = BlendPixels(existingPixel, pixel);
                            target.SetPixel(targetX, targetY, blendedPixel);
                        }
                    }
                }
            }
        }

        private static Color[] RotatePixels(Color[] originalPixels, int width, int height, float rotation)
        {
            if (IsExactAngle(rotation))
            {
                return RotatePixelsExact(originalPixels, width, height, rotation);
            }
            return RotatePixelsInterpolated(originalPixels, width, height, rotation);
        }

        private static bool IsExactAngle(float rotation)
        {
            float normalized = ((rotation % 360) + 360) % 360;
            return Mathf.Approximately(normalized, 0) ||
                   Mathf.Approximately(normalized, 90) ||
                   Mathf.Approximately(normalized, 180) ||
                   Mathf.Approximately(normalized, 270);
        }

        private static Color[] RotatePixelsExact(Color[] originalPixels, int width, int height, float rotation)
        {
            float normalized = ((rotation % 360) + 360) % 360;
            int newWidth = width;
            int newHeight = height;
            if (Mathf.Approximately(normalized, 90) || Mathf.Approximately(normalized, 270))
            {
                newWidth = height;
                newHeight = width;
            }
            Color[] rotatedPixels = new Color[newWidth * newHeight];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = originalPixels[y * width + x];
                    int newX = 0, newY = 0;
                    if (Mathf.Approximately(normalized, 0))
                    {
                        newX = x;
                        newY = y;
                    }
                    else if (Mathf.Approximately(normalized, 90))
                    {
                        newX = height - 1 - y;
                        newY = x;
                    }
                    else if (Mathf.Approximately(normalized, 180))
                    {
                        newX = width - 1 - x;
                        newY = height - 1 - y;
                    }
                    else if (Mathf.Approximately(normalized, 270))
                    {
                        newX = y;
                        newY = width - 1 - x;
                    }
                    if (newX >= 0 && newX < newWidth && newY >= 0 && newY < newHeight)
                    {
                        rotatedPixels[newY * newWidth + newX] = pixel;
                    }
                }
            }
            return rotatedPixels;
        }

        private static Color[] RotatePixelsInterpolated(Color[] originalPixels, int width, int height, float rotation)
        {
            float rad = rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            Vector2Int rotatedSize = GetRotatedSize(width, height, rotation);
            int newWidth = rotatedSize.x;
            int newHeight = rotatedSize.y;
            Color[] rotatedPixels = new Color[newWidth * newHeight];
            float centerX = (width - 1) / 2.0f;
            float centerY = (height - 1) / 2.0f;
            float newCenterX = (newWidth - 1) / 2.0f;
            float newCenterY = (newHeight - 1) / 2.0f;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float relX = x - newCenterX;
                    float relY = y - newCenterY;
                    float sourceX = (relX * cos - relY * sin) + centerX;
                    float sourceY = (relX * sin + relY * cos) + centerY;
                    Color pixel = ImprovedBilinearInterpolation(originalPixels, width, height, sourceX, sourceY);
                    rotatedPixels[y * newWidth + x] = pixel;
                }
            }
            return rotatedPixels;
        }

        private static Color ImprovedBilinearInterpolation(Color[] pixels, int width, int height, float x, float y)
        {
            if (x < 0 || x >= width - 1 || y < 0 || y >= height - 1)
            {
                return Color.clear;
            }
            int x1 = Mathf.FloorToInt(x);
            int y1 = Mathf.FloorToInt(y);
            int x2 = x1 + 1;
            int y2 = y1 + 1;
            if (x1 < 0 || x2 >= width || y1 < 0 || y2 >= height)
                return Color.clear;
            Color p11 = pixels[y1 * width + x1];
            Color p12 = pixels[y2 * width + x1];
            Color p21 = pixels[y1 * width + x2];
            Color p22 = pixels[y2 * width + x2];
            float wx = x - x1;
            float wy = y - y1;
            Color top = Color.Lerp(p11, p21, wx);
            Color bottom = Color.Lerp(p12, p22, wx);
            Color result = Color.Lerp(top, bottom, wy);
            return result;
        }

        private static Vector2Int GetRotatedSize(int width, int height, float rotation)
        {
            float normalized = ((rotation % 360) + 360) % 360;
            if (Mathf.Approximately(normalized, 0) || Mathf.Approximately(normalized, 180))
            {
                return new Vector2Int(width, height);
            }
            else if (Mathf.Approximately(normalized, 90) || Mathf.Approximately(normalized, 270))
            {
                return new Vector2Int(height, width);
            }
            float rad = rotation * Mathf.Deg2Rad;
            float cos = Mathf.Abs(Mathf.Cos(rad));
            float sin = Mathf.Abs(Mathf.Sin(rad));
            int newWidth = Mathf.RoundToInt(width * cos + height * sin);
            int newHeight = Mathf.RoundToInt(width * sin + height * cos);
            return new Vector2Int(newWidth, newHeight);
        }

        private static Color BlendPixels(Color background, Color foreground)
        {
            float alpha = foreground.a + background.a * (1 - foreground.a);
            if (alpha < 0.001f) return Color.clear;
            Color result = (foreground * foreground.a + background * background.a * (1 - foreground.a)) / alpha;
            result.a = alpha;
            return result;
        }
        #endregion

        //获取类型组的组合下标结果
        private static int GetTileTypeGroupIndexs(TileTypeGroup tileTypeGroup)
        {
            TileType lt = tileTypeGroup.LeftTop;
            TileType rt = tileTypeGroup.RightTop;
            TileType lb = tileTypeGroup.LeftBottom;
            TileType rb = tileTypeGroup.RightBottom;

            if ((lt == rb && lt != rt && lt != lb) || (rt == lb && rt != lt && rt != rb))
            {
                return 0;
            }
            if ((lt == rt && rt == lb && lt != rb) ||
                (lt == rt && rt == rb && lt != lb) ||
                (lt == lb && lb == rb && lt != rt) ||
                (rt == lb && lb == rb && rt != lt))
            {
                return 1;
            }
            if (lt != rt && lt != lb && lt != rb && rt != lb && rt != rb && lb != rb)
            {
                return 2;
            }
            if ((lt == rt && lb == rb && lt != lb) ||
                (lt == lb && rt == rb && lt != rt))
            {
                return 3;
            }
            if (lt == rt && rt == lb && lb == rb)
            {
                return 4;
            }
            if ((lt == rt && lt != lb && lt != rb && lb != rb) ||
                (lb == rb && lb != lt && lb != rt && lt != rt) ||
                (lt == lb && lt != rt && lt != rb && rt != rb) ||
                (rt == rb && rt != lt && rt != lb && lt != lb))
            {
                return 5;
            }
            return 2;
        }

        /// <summary>
        /// 释放缓存
        /// </summary>
        public static void Release()
        {
            // 清空数组
            Array.Clear(_tileGroupTexture2DResources, 0, _tileGroupTexture2DResources.Length);
            Array.Clear(_tileBases, 0, _tileBases.Length);
            Array.Clear(_tileSprites, 0, _tileSprites.Length);
            Array.Clear(_dynamicTileTypes, 0, _dynamicTileTypes.Length);
            Array.Clear(_sprites, 0, _sprites.Length);
        }

        // 修复的TileTypeGroup到数组索引的转换
        private static int TileTypeGroupToIndex(TileTypeGroup group)
        {
            int lt = (int)group.LeftTop;
            int rt = (int)group.RightTop;
            int lb = (int)group.LeftBottom;
            int rb = (int)group.RightBottom;
            
            // 确保每个分量都在0-15范围内
            lt = Mathf.Clamp(lt, 0, MAX_TILE_TYPES - 1);
            rt = Mathf.Clamp(rt, 0, MAX_TILE_TYPES - 1);
            lb = Mathf.Clamp(lb, 0, MAX_TILE_TYPES - 1);
            rb = Mathf.Clamp(rb, 0, MAX_TILE_TYPES - 1);
            
            // 计算索引：lt * 16^3 + rt * 16^2 + lb * 16 + rb
            return lt * MAX_TILE_TYPES * MAX_TILE_TYPES * MAX_TILE_TYPES +
                   rt * MAX_TILE_TYPES * MAX_TILE_TYPES +
                   lb * MAX_TILE_TYPES +
                   rb;
        }
    }

    // 原有结构体保持不变
    public struct TagDynamicTileTypeGroup
    {
        public bool LeftTop;
        public bool RightTop;
        public bool LeftBottom;
        public bool RightBottom;
    }

    public struct TileTypeGroup : IEquatable<TileTypeGroup>
    {
        public TileType LeftTop;
        public TileType RightTop;
        public TileType LeftBottom;
        public TileType RightBottom;

        public void SetTileType(int index, TileType tileType)
        {
            if (index == 0) LeftTop = tileType;
            else if (index == 1) RightTop = tileType;
            else if (index == 2) LeftBottom = tileType;
            else if (index == 3) RightBottom = tileType;
        }

        public bool Equals(TileTypeGroup other)
        {
            return LeftTop == other.LeftTop && RightTop == other.RightTop &&
                   LeftBottom == other.LeftBottom && RightBottom == other.RightBottom;
        }

        public override int GetHashCode()
        {
            return ((int)LeftTop << 24) | ((int)RightTop << 16) | ((int)LeftBottom << 8) | (int)RightBottom;
        }
    }
}