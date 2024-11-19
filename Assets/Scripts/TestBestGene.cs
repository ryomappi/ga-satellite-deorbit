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
    private Agent testAgent;
    private Gene bestGene;
    void Awake()
    {
        // 最も優秀な遺伝子を読み込む
        bestGene = ReadBestGene();

        // 衛星エージェントを生成
        var obj = Instantiate(GObjectAgent);
        obj.SetActive(true);
        testAgent = obj.GetComponent<Agent>();
    }

    void Start()
    {
        // 衛星エージェントを初期化
        testAgent.Reset();
        // 遺伝子を適用
        testAgent.ApplyGene(bestGene);
    }

    void FixedUpdate()
    {
        // エージェントがタスクを完了したら終了
        if (testAgent.IsDone)
        {
            Debug.Log("Task completed.");
            Debug.Log($"Fitness: {testAgent.Fitness}");
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

        string filePath = Path.Combine("test", "best_gene.txt");
        if (File.Exists(filePath))
        {
            using (StreamReader file = new StreamReader(filePath, Encoding.UTF8))
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
            Debug.LogError("best_gene.txt not found.");
        }

        return gene;
    }
}
