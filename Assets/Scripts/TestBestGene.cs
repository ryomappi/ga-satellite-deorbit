using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestBestGene : MonoBehaviour
{
    [Header("Agent Prefab"), SerializeField] private GameObject GObjectAgent = null;
    [Header("Best Gene Path"), SerializeField] private string BestGeneFileName = null;
    private Agent testAgent;
    private Gene bestGene;
    void Awake()
    {
        // 最も優秀な遺伝子を読み込む
        bestGene = ReadBestGene();
        if (bestGene == null)
        {
            Debug.LogError("Failed to read the best gene.");
            return;
        }
        // 衛星エージェントを生成
        var obj = Instantiate(GObjectAgent);
        obj.SetActive(true);
        testAgent = obj.GetComponent<Agent>();
        if (testAgent == null)
        {
            Debug.LogError("Failed to get Agent component from the instantiated object.");
            return;
        }
    }

    void Start()
    {
        if (testAgent != null && bestGene != null)
        {
            // 衛星エージェントを初期化
            testAgent.Reset();
            // 遺伝子を適用
            testAgent.ApplyGene(bestGene);
        }
    }

    void FixedUpdate()
    {
        if (testAgent == null) return;

        // エージェントがタスクを完了したら終了
        if (testAgent.IsDone)
        {
            Debug.Log("Task completed.");
            Debug.Log($"Fitness: {testAgent.Fitness}");
            if (testAgent.Succeeded)
            {
                Debug.Log("Succeeded.");
            }
            else
            {
                Debug.Log("Failed.");
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }
        testAgent.AgentUpdate();
    }

    private Gene ReadBestGene()
    {
        Gene gene = new Gene();
        gene.data = new List<int>();
        string BestGenePath = Path.Combine(Application.dataPath, "results", BestGeneFileName + ".txt");
        Debug.Log("Best Gene Path: " + BestGenePath);

        if (File.Exists(BestGenePath))
        {
            using (StreamReader file = new StreamReader(BestGenePath, Encoding.UTF8))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (int.TryParse(line, out int value))
                    {
                        gene.data.Add(value);
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Best Gene Path not found: {BestGenePath}");
            return null;
        }
        // gene.dataの中身を見る
        for (int i = 0; i < gene.data.Count; i++)
        {
            Debug.Log($"Gene.data[{i}]: {gene.data[i]}");
        }
        return gene;
    }
}
