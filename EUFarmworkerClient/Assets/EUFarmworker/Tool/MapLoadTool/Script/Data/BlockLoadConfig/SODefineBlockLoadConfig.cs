using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.BlockLoadConfig
{
    [CreateAssetMenu(fileName = "DefineBlockLoadConfig", menuName = "EUTool/MapLoad/BlockLoadConfig/DefineBlockLoadConfig")]
    public class SODefineBlockLoadConfig:SOBlockLoadConfigBase
    {
        [SerializeField]
        private int _singleBlockSize = 30;
        [SerializeField]
        private Vector3Int _lookBlockSize = new(3,3,0);
        public override void SetSingleBlockSize(int size)
        {
            _singleBlockSize = size;
        }

        public override int GetSingleBlockSize()
        {
            return _singleBlockSize;
        }

        public override void SetLookBlockSize(Vector3Int size)
        {
            _lookBlockSize = size;
        }

        public override Vector3Int GetLookBlockSize()
        {
            return _lookBlockSize;
        }

        [NonSerialized]
        private Vector3Int _qkcenter;
        [NonSerialized]
        private bool isInit = false;
        public override void OnMovePosition(Vector3 position)
        {
            var lsdata = new Vector3Int(
                position.x>=0? (int)(position.x / _singleBlockSize):(int)(position.x / _singleBlockSize) -1, 
                position.y>=0?(int)(position.y / _singleBlockSize) :(int)(position.y / _singleBlockSize) -1,
                0);
            var newcenter = new Vector3Int(lsdata.x * _singleBlockSize,lsdata.y * _singleBlockSize);//区块位置
            if (!_runUninstall)
            {
                UpdateUninstall().Forget();
            }
            if (!_runLoad)
            {
                UpdateLoad().Forget();
            }

            if(_qkcenter ==  newcenter && isInit) return;
            UpdateBlock(newcenter);
            _qkcenter = newcenter;
        }
        [NonSerialized]
        private readonly HashSet<Vector3Int> _blocks = new();
        [NonSerialized]
        private readonly HashSet<Vector3Int> _lsBlocks = new();
        [NonSerialized]
        private readonly Queue<Vector3Int> _loadQueue = new();
        [NonSerialized]
        private readonly Queue<Vector3Int> _uninstallQueue = new();
        private void UpdateBlock(Vector3Int newPosition)
        {
            _lsBlocks.Clear();
    
            int lsi = _lookBlockSize.x * 2 + 1;
            int lsj = _lookBlockSize.y * 2 + 1;
    
            var currentPos = new Vector3Int(
                newPosition.x + _lookBlockSize.x * _singleBlockSize,
                newPosition.y + _lookBlockSize.y * _singleBlockSize,
                0
            );
    
            // 预计算减少循环内的运算
            int xStep = _singleBlockSize;
            int yStep = _singleBlockSize;
    
            for (int i = 0; i < lsi; i++)
            {
                var rowPos = currentPos;
                for (int j = 0; j < lsj; j++)
                {
                    _lsBlocks.Add(rowPos);
                    rowPos.y -= yStep;
                }
                currentPos.x -= xStep;
            }
    
            // 使用更高效的集合差异计算
            foreach (var existingBlock in _blocks)
            {
                if (!_lsBlocks.Contains(existingBlock))
                {
                    _uninstallQueue.Enqueue(existingBlock);
                }
            }
    
            foreach (var newBlock in _lsBlocks)
            {
                if (!_blocks.Contains(newBlock))
                {
                    _loadQueue.Enqueue(newBlock);
                }
            }
        }
        [NonSerialized]
        bool _runLoad = false;
        private async UniTaskVoid UpdateLoad()
        {
            if(_loadQueue.Count == 0) return;
            _runLoad = true;
            while (_loadQueue.Count > 0)
            {
                LoadBlock(_loadQueue.Dequeue());
                await UniTask.DelayFrame(10);
            }
            _runLoad = false;
        }
        [NonSerialized]
        bool _runUninstall = false;
        private async UniTaskVoid UpdateUninstall()
        {
            if(_uninstallQueue.Count == 0) return;
            _runUninstall = true;
            while (_uninstallQueue.Count > 0)
            {
                UninstallBlock(_uninstallQueue.Dequeue());
                await UniTask.DelayFrame(10);
            }
            _runUninstall = false;
        }
        private void LoadBlock(Vector3Int position)
        {
            if (!_blocks.Contains(position))
            {
                _OnLoadBlockChangeEvent?.Invoke(position);
                _blocks.Add(position);
            }
        }

        private void UninstallBlock(Vector3Int position)
        {
            if (_blocks.Contains(position))
            {
                _OnUninstallBlockChangeEvent?.Invoke(position);
                _blocks.Remove(position);
            }
        }
        public override void Init(Vector3 position = default)
        {
            OnMovePosition(position);
            isInit = true;
        }
    }
}