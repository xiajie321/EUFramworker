using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace EUFarmworker.Tool.MapLoadTool.Script.Data.NoiseConfig
{
    [CreateAssetMenu(fileName = "DefineNoiseConfig", menuName = "EUTool/MapLoad/NoiseConfig/DefineNoiseConfig")]
    public class SODefineNoiseConfig:SONoiseConfigBase
    {
        [SerializeField]
        private int _send = 0;

        [SerializeField] 
        private float _scale = 10f;
        public override int GetSend()
        {
            return _send;
        }

        public override void SetSend(int value)
        {
            _send = value;
        }

        public override float OnUse(Vector3 position)
        {
            return noise.snoise(new float2((position.x + _send) / _scale,(position.y + _send) /_scale));
        }

        public override void Dispose()
        {
        }
    }
}