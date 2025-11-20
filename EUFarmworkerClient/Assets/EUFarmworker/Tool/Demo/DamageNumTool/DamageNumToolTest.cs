using System.Collections;
using System.Collections.Generic;
using EUFarmworker.Tool.DamageNumTool.Script;
using EUFarmworker.Tool.DamageNumTool.Script.Generate;
using UnityEngine;

public class DamageNumToolTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DamageNumTool.Init();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 500; i++)
        {
            DamageNumTool.AddDamageNum(new Vector2(Random.Range(-100f,100f),Random.Range(-100f,100f)),Random.Range(0,10000),(DamageNumColor)Random.Range(0,2));
        }
        
    }
}
