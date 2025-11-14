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
        private static readonly Dictionary<TileTypeGroup, List<Texture2D>> _tileGroupTexture2DResources = new();//缓存生成的2D纹理
        private static readonly Dictionary<TileTypeGroup,TileBase> _tileBases = new();//缓存生成的TileBase
        private static readonly Dictionary<TileTypeGroup, Sprite[]> _tileSprites = new();//缓存生成的精灵
        private static SODoubleTileScriptableObject _doubleTileViewConfig;

        public static void Init(SODoubleTileScriptableObject data)
        {
            _doubleTileViewConfig = data;
        }

        /// <summary>
        /// 纹理生成
        /// </summary>
        /// <param name="tileTypeGroup">生成器</param>
        public static List<Texture2D> GenerateTexture2D(TileTypeGroup tileTypeGroup)
        {
            if (_tileGroupTexture2DResources.ContainsKey(tileTypeGroup))
                return _tileGroupTexture2DResources[tileTypeGroup];
            //生成
            if (tileTypeGroup.IsDynamicTile()) //动态瓦片
            {
                _tileGroupTexture2DResources.Add(tileTypeGroup, DynamicTexture2D(tileTypeGroup));
                return _tileGroupTexture2DResources[tileTypeGroup];
            }
            Debug.Log("111");
            _tileGroupTexture2DResources.Add(tileTypeGroup, new List<Texture2D>() { StaticTexture2D(tileTypeGroup,GetTileTypeGroupIndexs(tileTypeGroup)) });
            return _tileGroupTexture2DResources[tileTypeGroup];
        }
        /// <summary>
        /// 获取瓦片
        /// </summary>
        /// <param name="typeGroup"></param>
        /// <returns></returns>
        public static TileBase GetTileBase(TileTypeGroup typeGroup)
        {
            if (_tileBases.ContainsKey(typeGroup))
            {
                return _tileBases[typeGroup];
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
                a.m_AnimationStartFrame =  _doubleTileViewConfig.Frame;
                _tileBases.Add(typeGroup, a);
                return a;
            }

            Tile b = ScriptableObject.CreateInstance<Tile>();
            b.sprite = GetSprite(typeGroup);
            _tileBases.Add(typeGroup, b);
            return b;
        }
        /// <summary>
        /// 获取精灵
        /// </summary>
        /// <param name="tileTypeGroup"></param>
        /// <param name="index">是动态的话可以通过该下标直接获取动画帧的精灵</param>
        /// <returns></returns>
        public static Sprite GetSprite(TileTypeGroup tileTypeGroup,int index = 0)
        {
            if (_tileSprites.ContainsKey(tileTypeGroup))
            {
                if (_tileSprites[tileTypeGroup].Length > index)//验证是否超过索引长度
                {
                    if(_tileSprites[tileTypeGroup][index])//验证是否为空
                        return _tileSprites[tileTypeGroup][index];
                }
                var ls =DoubleTileToolTileGenerate.GenerateTexture2D(tileTypeGroup)[index];
                _tileSprites[tileTypeGroup][index] = Sprite.Create(ls, new Rect(0, 0, ls.width, ls.height), new Vector2(0.5f, 0.5f));
                return _tileSprites[tileTypeGroup][index];
            }
            _tileSprites.Add(tileTypeGroup, new Sprite[_doubleTileViewConfig.Frame]);
            var ls1 =DoubleTileToolTileGenerate.GenerateTexture2D(tileTypeGroup)[index];
            _tileSprites[tileTypeGroup][index] = Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f));
            return _tileSprites[tileTypeGroup][index];
        }
        #region 动态瓦片标记

        private static readonly Dictionary<TileType, bool> _dynamicTileTypes = new(); //缓存处理过的标记,降低查询List带来的性能压力

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
            if (_dynamicTileTypes.ContainsKey(tileTypeGroup))
                return _dynamicTileTypes[tileTypeGroup];
            _dynamicTileTypes.Add(tileTypeGroup,
                _doubleTileViewConfig.TileDatas.Find(v => v.TileName == tileTypeGroup.ToString()).IsDynamic);
            return _dynamicTileTypes[tileTypeGroup];
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
            List<Texture2D>  lstex = new();
            for (int i=0;i<_doubleTileViewConfig.Frame;i++)
            {
                lstex.Add(StaticTexture2D(tileTypeGroup,ls,i));
            }
            return lstex;
        }
        
        private static Texture2D StaticTexture2D(TileTypeGroup tileTypeGroup,int data, int index = 0)
        {
            var ls = data;
            Texture2D lstex = null;

            switch (ls)
            {
                case 0: //对角线
                    //Debug.Log("对角线");
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightBottom &&
                        tileTypeGroup.RightTop == tileTypeGroup.LeftBottom)
                    {
                        if (tileTypeGroup.LeftTop > tileTypeGroup.RightTop)
                        {
                            var a = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                            var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                            var c = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                            var ls1 = MergeSprites(a[2], b[0], 0, 270);
                            lstex = MergeSprites(
                                Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                                c[0], 0, 90);
                        }
                        else
                        {
                            var a = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                            var b = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                            var c = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                            var ls1 = MergeSprites(a[2], b[0], 90, 0);
                            lstex = MergeSprites(
                                Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                                c[0], 0, 180);
                        }
                        break;    
                    }
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightBottom)
                    {
                        var a = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var b = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        var c = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                        var ls1 = MergeSprites(a[2], b[0], 0, 270);
                        lstex = MergeSprites(
                            Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                            c[0], 0, 90);
                    }
                    else if(tileTypeGroup.RightTop == tileTypeGroup.LeftBottom)
                    {
                        var a = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        var b = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var c = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                        var ls1 = MergeSprites(a[2], b[0], 90, 0);
                        lstex = MergeSprites(
                            Sprite.Create(ls1, new Rect(0, 0, ls1.width, ls1.height), new Vector2(0.5f, 0.5f)),
                            c[0], 0, 180);
                    }

                    break;
                case 1: //三个点相同
                    //Debug.Log("三个点相同");
                    TileType FindUniqueValue(TileTypeGroup group)
                    {
                        // 如果左上和右上相同，那么不同的值在底部
                        if (group.LeftTop == group.RightTop)
                        {
                            // 如果左下与左上不同，则左下是唯一不同的
                            if (group.LeftBottom != group.LeftTop)
                                return group.LeftBottom;
                            // 否则右下是唯一不同的
                            else
                                return group.RightBottom;
                        }
                        else
                        {
                            // 左上和右上不同，检查哪个是唯一的
                            if (group.LeftTop == group.LeftBottom)
                                return group.RightTop; // 右上是唯一的
                            else
                                return group.LeftTop; // 左上是唯一的
                        }
                    }

                    var lsType= FindUniqueValue(tileTypeGroup);
                    
                    if (lsType == tileTypeGroup.LeftTop)
                    {
                        var a = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var b =GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        lstex = MergeSprites(b[3],a[0],180,0);
                    }
                    else if (lsType == tileTypeGroup.RightTop)
                    {
                        var a = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        var b =GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        lstex = MergeSprites(b[3],a[0],90,270);
                    }
                    else if(lsType == tileTypeGroup.RightBottom)
                    {
                        var a = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                        var b =GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        lstex = MergeSprites(b[3],a[0],0,180);
                    }
                    else
                    {
                        var a = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                        var b =GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        lstex = MergeSprites(b[3],a[0],270,90);
                    }
                    break;
                case 2: //四个点都不同
                    //Debug.Log("四个点都不同");
                    var def = GetSprites((TileType)0)[GetIndex(0,index)];
                    var la =  GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                    var lsb =  GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                    var lsc =  GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                    var lsd=  GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                    var als1 = MergeSprites(def[4],la[0],0,0);
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
                    //Debug.Log("水平/垂直对称");
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightTop)
                    {
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var a3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                        lstex = MergeSprites(def3[1], a3[1], 270, 90);
                    }
                    else
                    {
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var a3 = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                        lstex = MergeSprites(def3[1], a3[1], 0, 180);
                    }
                    break;
                case 4: //四个点完全相同
                    //Debug.Log("四个点完全相同");
                    var def4 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                    lstex = MergeSprites(def4[4],def4[4],0,0);
                    break;
                case 5: //两个点水平/垂直对齐
                    //Debug.Log("两个点水平/垂直对齐");
                    if (tileTypeGroup.LeftTop == tileTypeGroup.RightTop)
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0,index)];
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var a3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                        var b3 = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 270);
                        ls5 = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), a3[0],0,90);
                        lstex = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), b3[0],0,180);
                    }
                    else if(tileTypeGroup.LeftBottom == tileTypeGroup.RightBottom)
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0,index)];
                        var def3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                        var a3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var b3 = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 90);
                        ls5 = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), a3[0],0,0);
                        lstex = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), b3[0],0,270);
                    }
                    else if (tileTypeGroup.LeftTop == tileTypeGroup.LeftBottom)
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0,index)];
                        var def3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var a3 = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        var b3 = GetSprites(tileTypeGroup.RightBottom)[GetIndex(tileTypeGroup.RightBottom,index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 0);
                        ls5 = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), a3[0],0,90);
                        lstex = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), b3[0],0,180);
                    }
                    else
                    {
                        var def1 = GetSprites((TileType)0)[GetIndex(0,index)];
                        var def3 = GetSprites(tileTypeGroup.RightTop)[GetIndex(tileTypeGroup.RightTop,index)];
                        var a3 = GetSprites(tileTypeGroup.LeftTop)[GetIndex(tileTypeGroup.LeftTop,index)];
                        var b3 = GetSprites(tileTypeGroup.LeftBottom)[GetIndex(tileTypeGroup.LeftBottom,index)];
                        var ls5 = MergeSprites(def1[4], def3[1], 0, 180);
                        ls5 = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), a3[0],0,0);
                        lstex = MergeSprites(Sprite.Create(ls5,new Rect(0,0,ls5.width,ls5.height),new Vector2(0.5f,0.5f)), b3[0],0,90);
                    }
                    break;
            }
            return lstex;
        }

        private static int GetIndex(TileType tileType, int index) //获取下标(用于生成动态瓦片时避免下标越界问题)
        {
            //Debug.Log($"{tileType.ToString()}:{(tileType.IsDynamicTileTypeGroup() ? index : 0)}");
            return tileType.IsDynamicTileTypeGroup() ? index : 0;
        }
        private static readonly Dictionary<TileType, List<List<Sprite>>> _sprites = new(); //这里存的是从配置中读取的精灵数据

        private static List<List<Sprite>> GetSprites(TileType tileType)
        {
            if (_sprites.ContainsKey(tileType)) return _sprites[tileType];
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

            _sprites.Add(tileType, fh);
            return fh;
        }
        #region 纹理组合
        private static Texture2D MergeSprites(Sprite a, Sprite b, float aRotate, float bRotate)
        {
            // 获取两个精灵的纹理和矩形信息
            Texture2D texA = a.texture;
            Texture2D texB = b.texture;
            Rect rectA = a.rect;
            Rect rectB = b.rect;

            // 计算合并后的纹理尺寸（取两个精灵的最大宽高）
            int width = Mathf.Max((int)rectA.width, (int)rectB.width);
            int height = Mathf.Max((int)rectA.height, (int)rectB.height);

            // 创建新的纹理
            Texture2D mergedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // 初始化透明背景
            Color[] transparentPixels = new Color[width * height];
            for (int i = 0; i < transparentPixels.Length; i++)
            {
                transparentPixels[i] = Color.clear;
            }

            mergedTexture.SetPixels(transparentPixels);

            // 合并第一个精灵（带旋转）
            MergeSpriteWithRotation(mergedTexture, texA, rectA, aRotate);

            // 合并第二个精灵（带旋转，覆盖模式）
            MergeSpriteWithRotation(mergedTexture, texB, rectB, bRotate);
            mergedTexture.Apply();
            return mergedTexture;
        }
        private static void MergeSpriteWithRotation(Texture2D target, Texture2D source, Rect spriteRect, float rotation)
        {
            int spriteWidth = (int)spriteRect.width;
            int spriteHeight = (int)spriteRect.height;

            // 获取原始精灵像素
            Color[] spritePixels = source.GetPixels((int)spriteRect.x, (int)spriteRect.y, spriteWidth, spriteHeight);

            // 应用旋转
            Color[] rotatedPixels = RotatePixels(spritePixels, spriteWidth, spriteHeight, rotation);

            // 计算旋转后的尺寸
            Vector2Int rotatedSize = GetRotatedSize(spriteWidth, spriteHeight, rotation);
            int rotatedWidth = rotatedSize.x;
            int rotatedHeight = rotatedSize.y;

            // 计算在目标纹理中的位置（居中）
            int startX = (target.width - rotatedWidth) / 2;
            int startY = (target.height - rotatedHeight) / 2;

            // 将旋转后的像素复制到目标纹理（覆盖模式）
            for (int y = 0; y < rotatedHeight; y++)
            {
                for (int x = 0; x < rotatedWidth; x++)
                {
                    int sourceIndex = y * rotatedWidth + x;
                    Color pixel = rotatedPixels[sourceIndex];

                    // 使用更宽松的透明度检查，避免边缘半透明像素被忽略
                    if (pixel.a > 0.01f) // 降低阈值，捕捉更多边缘像素
                    {
                        int targetX = startX + x;
                        int targetY = startY + y;

                        if (targetX >= 0 && targetX < target.width && targetY >= 0 && targetY < target.height)
                        {
                            // 使用混合模式而不是直接覆盖，保持平滑过渡
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
            // 处理特殊角度：0, 90, 180, 270度使用精确算法
            if (IsExactAngle(rotation))
            {
                return RotatePixelsExact(originalPixels, width, height, rotation);
            }

            // 其他角度使用插值算法
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
    
            // 90度和270度需要交换宽高
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
    
            // 精确的中心点计算（使用浮点数中心）
            float centerX = (width - 1) / 2.0f;
            float centerY = (height - 1) / 2.0f;
            float newCenterX = (newWidth - 1) / 2.0f;
            float newCenterY = (newHeight - 1) / 2.0f;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // 计算相对于新图像中心的坐标
                    float relX = x - newCenterX;
                    float relY = y - newCenterY;
            
                    // 应用反向旋转
                    float sourceX = (relX * cos - relY * sin) + centerX;
                    float sourceY = (relX * sin + relY * cos) + centerY;
            
                    // 使用改进的插值方法
                    Color pixel = ImprovedBilinearInterpolation(originalPixels, width, height, sourceX, sourceY);
                    rotatedPixels[y * newWidth + x] = pixel;
                }
            }
    
            return rotatedPixels;
        }
        private static Color ImprovedBilinearInterpolation(Color[] pixels, int width, int height, float x, float y)
        {
            // 边界检查
            if (x < 0 || x >= width - 1 || y < 0 || y >= height - 1)
            {
                return Color.clear;
            }

            int x1 = Mathf.FloorToInt(x);
            int y1 = Mathf.FloorToInt(y);
            int x2 = x1 + 1;
            int y2 = y1 + 1;

            // 确保不越界
            if (x1 < 0 || x2 >= width || y1 < 0 || y2 >= height)
                return Color.clear;

            // 获取四个相邻像素
            Color p11 = pixels[y1 * width + x1];
            Color p12 = pixels[y2 * width + x1];
            Color p21 = pixels[y1 * width + x2];
            Color p22 = pixels[y2 * width + x2];
    
            // 计算插值权重
            float wx = x - x1;
            float wy = y - y1;
    
            // 双线性插值
            Color top = Color.Lerp(p11, p21, wx);
            Color bottom = Color.Lerp(p12, p22, wx);
            Color result = Color.Lerp(top, bottom, wy);
    
            return result;
        }
        private static Vector2Int GetRotatedSize(int width, int height, float rotation)
        {
            // 处理特殊角度
            float normalized = ((rotation % 360) + 360) % 360;
    
            if (Mathf.Approximately(normalized, 0) || Mathf.Approximately(normalized, 180))
            {
                return new Vector2Int(width, height);
            }
            else if (Mathf.Approximately(normalized, 90) || Mathf.Approximately(normalized, 270))
            {
                return new Vector2Int(height, width);
            }

            // 其他角度使用三角函数计算
            float rad = rotation * Mathf.Deg2Rad;
            float cos = Mathf.Abs(Mathf.Cos(rad));
            float sin = Mathf.Abs(Mathf.Sin(rad));
    
            int newWidth = Mathf.RoundToInt(width * cos + height * sin);
            int newHeight = Mathf.RoundToInt(width * sin + height * cos);
    
            return new Vector2Int(newWidth, newHeight);
        }

        private static Color BlendPixels(Color background, Color foreground)
        {
            // 标准Alpha混合
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
            // 获取四个角的类型
            TileType lt = tileTypeGroup.LeftTop;
            TileType rt = tileTypeGroup.RightTop;
            TileType lb = tileTypeGroup.LeftBottom;
            TileType rb = tileTypeGroup.RightBottom;

            // 1、两个点对角线相同时取下标2,剩余两个点取下标0
            if ((lt == rb && lt != rt && lt != lb) || (rt == lb && rt != lt && rt != rb))
            {
                return 0;
            }

            // 2、三个点相同时取下标3，剩下不同的点取下标0
            if ((lt == rt && rt == lb && lt != rb) || // 上左相同，右下不同
                (lt == rt && rt == rb && lt != lb) || // 上右相同，左下不同
                (lt == lb && lb == rb && lt != rt) || // 左下相同，上右不同
                (rt == lb && lb == rb && rt != lt)) // 右下相同，上左不同
            {
                return 1;
            }

            // 3、四个点不同时全部取下标0
            if (lt != rt && lt != lb && lt != rb && rt != lb && rt != rb && lb != rb)
            {

                return 2;
            }

            // 4、四个点(两两相同并且水平对称)取下标1
            if ((lt == rt && lb == rb && lt != lb) || // 水平对称：上下相同
                (lt == lb && rt == rb && lt != rt)) // 垂直对称：左右相同
            {
                return 3;
            }

            // 5、四个点完全相同时仅取下标4
            if (lt == rt && rt == lb && lb == rb)
            {
                return 4;
            }

            // 6、两个相同的点水平对齐并且另外两个点不同，相同的点取下标1,不同的点取下标0
            // 水平对齐的情况
            if ((lt == rt && lt != lb && lt != rb && lb != rb) || // 上边相同
                (lb == rb && lb != lt && lb != rt && lt != rt)) // 下边相同
            {
                return 5;
            }

            // 垂直对齐的情况
            if ((lt == lb && lt != rt && lt != rb && rt != rb) || // 左边相同
                (rt == rb && rt != lt && rt != lb && lt != lb)) // 右边相同
            {
                return 5;
            }

            // 默认情况：全部取0
            return 2;
        }

        /// <summary>
        /// 释放缓存
        /// </summary>
        public static void Release()
        {
            _dynamicTileTypes.Clear();
            _sprites.Clear();
            _tileBases.Clear();
            _tileSprites.Clear();
            _tileGroupTexture2DResources.Clear();
        }
    }

    public struct TagDynamicTileTypeGroup
    {
        public bool LeftTop;
        public bool RightTop;
        public bool LeftBottom;
        public bool RightBottom;
    }

    public struct TileTypeGroup
    {
        public TileType LeftTop;
        public TileType RightTop;
        public TileType LeftBottom;
        public TileType RightBottom;

        public void SetTileType(int index, TileType tileType)
        {
            if (index == 0)
            {
                LeftTop = tileType;
            }
            else if (index == 1)
            {
                RightTop = tileType;
                
            }
            else if (index == 2)
            {
                
                LeftBottom = tileType;
                
            }
            else if (index == 3)
            {
                RightBottom = tileType;
                
            }
        }
    }
}