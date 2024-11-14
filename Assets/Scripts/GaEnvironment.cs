using UnityEngine;
using System.Collections.Generic;

public class GaEnvironment : MonoBehaviour
{
    public int population = 100; // 衛星エージェントの個体数
    public int generation = 100; // 世代数
    [Header("Agent Prefab"), SerializeField] public GameObject GObjectAgent = null;

    // 生成したゲームオブジェクトを格納するリスト
    private List<GameObject> GObjects = new List<GameObject>();
    // 生成した遺伝子を格納するリスト
    private List<Gene> genes = new List<Gene>();
    void Awake()
    {
        
    }
}
