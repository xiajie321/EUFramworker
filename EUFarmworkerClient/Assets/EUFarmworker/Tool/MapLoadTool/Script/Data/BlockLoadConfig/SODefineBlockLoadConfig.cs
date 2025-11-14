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
        [SerializeField] private int _invokeQuantity = 100;
        public override void SetSingleBlockSize(int size)
        {
            _singleBlockSize = size;
        }

        public override void SetLookBlockSize(Vector3Int size)
        {
            _lookBlockSize = size;
        }
        [NonSerialized]
        private Vector3Int _qkcenter;
        [NonSerialized]
        private bool isInit = false;
        public override void OnMovePosition(Vector3 position)
        {
            var lsdata = new Vector3Int(
                position.x>=0? (int)(position.x / _singleBlockSize):(int)(position.x / _singleBlockSize)-1, 
                position.y>=0?(int)(position.y / _singleBlockSize) :(int)(position.y / _singleBlockSize) -1,
                0);
            var newcenter = new Vector3Int(lsdata.x * _singleBlockSize,lsdata.y * _singleBlockSize);//区块位置
            
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
            int lsi = _lookBlockSize.x * 2 + 1;
            int lsj  = _lookBlockSize.y * 2 + 1;
            var lsdata = newPosition + new Vector3Int(_lookBlockSize.x * _singleBlockSize,_lookBlockSize.y*_singleBlockSize,0);
            var lsdata2 = lsdata;
            if(_lsBlocks.Count >0) _lsBlocks.Clear();
            for (int i = 0; i < lsi; i++)
            {
                lsdata2 = lsdata;
                for (int j = 0; j < lsj; j++)
                {
                    _lsBlocks.Add(lsdata2);
                    lsdata2 -= new Vector3Int(0,_singleBlockSize);
                }
                lsdata -= new Vector3Int(_singleBlockSize,0);
            }

            foreach (var item in _blocks)
            {
                if (!_lsBlocks.Contains(item))
                {
                    _uninstallQueue.Enqueue(item);
                }
            }

            foreach (var item in _lsBlocks)
            {
                if (!_blocks.Contains(item))
                {
                    _loadQueue.Enqueue(item);
                }
            }
            if (!_runUninstall)
            {
                UpdateUninstall().Forget();
            }

            if (!_runLoad)
            {
                UpdateLoad().Forget();
            }
        }
        [NonSerialized]
        bool _runLoad = false;
        private async UniTaskVoid UpdateLoad()
        {
            _runLoad = true;
            while (_loadQueue.Count > 0)
            {
                LoadBlock(_loadQueue.Dequeue());//.Forget();
                await UniTask.DelayFrame(10);
            }
            _runLoad = false;
        }
        [NonSerialized]
        bool _runUninstall = false;
        private async UniTaskVoid UpdateUninstall()
        {
            _runUninstall = true;
            while (_uninstallQueue.Count > 0)
            {
                UninstallBlock(_uninstallQueue.Dequeue());//.Forget();
                await UniTask.DelayFrame(10);
            }
            _runUninstall = false;
        }
        private void LoadBlock(Vector3Int position)
        {
            if (!_blocks.Contains(position))
            {
                int num = 0;
                for (int i = 0; i < _singleBlockSize; i++)
                {
                    for (int j = 0; j < _singleBlockSize; j++)
                    {
                        if (num > 1/Time.deltaTime)
                        {
                            num = 0;
                            //await  UniTask.Yield();
                        }
                        _OnLoadBlockChangeEvent?.Invoke(position + new Vector3Int(i, j, 0));
                        num++;
                    }
                }
                _blocks.Add(position);
            }
        }

        private void UninstallBlock(Vector3Int position)
        {
            if (_blocks.Contains(position))
            {
                int num = 0;
                for (int i = 0; i < _singleBlockSize; i++)
                {
                    for (int j = 0; j < _singleBlockSize; j++)
                    {
                        if (num > 1/Time.deltaTime)
                        {
                            num = 0;
                            //await UniTask.Yield();
                        }
                        _OnUninstallBlockChangeEvent?.Invoke(position + new Vector3Int(i, j, 0));
                        num++;
                    }
                }
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