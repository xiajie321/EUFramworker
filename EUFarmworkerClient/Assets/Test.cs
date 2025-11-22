using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EUFarmworker.Tool.DoubleTileTool.Script;
using EUFarmworker.Tool.DoubleTileTool.Script.Generate;
using PrimeTween;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    [SerializeField] private GameObject ls;
    private List<GameObject> _ls = new();
    TransformAccessArray _transforms;
    //private NativeArray<NativeArray<int>> _lsArray = new(10000,Allocator.Persistent);
    void Start()
    {
        _transforms = new(10000, 64);
        GameObject ls2;
        for (int i = 0; i < 10000; i++)
        {
            ls2 = Instantiate(ls);
            ls2.transform.position = new Vector3(Random.Range(-50,50),Random.Range(-50,50));
            _transforms.Add(ls2.transform);
            _ls.Add(ls2);
        }
    }

    private void Update()
    {
        MoveJob moveJob = new MoveJob()
        {
            DeltaTime = Time.deltaTime,
        };
        JobHandle handle = moveJob.Schedule(_transforms);
        handle.Complete();
    }
    private void OnDestroy()
    {
        if(_transforms.isCreated)
            _transforms.Dispose();
    }
}

public struct MoveJob : IJobParallelForTransform
{
    public float DeltaTime;
 
    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(1 * 10 *DeltaTime,1 * 10 *DeltaTime);
    }
}
