using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GaEnvironment : MonoBehaviour
{
    [Header("Settings"), SerializeField] private int totalPopulation = 200;  // 衛星エージェントの個体数
    private int TotalPopulation { get { return totalPopulation; } }
    [SerializeField] private int tournamentSelection = 85;  // トーナメント選択の選択数
    private int TournamentSelection { get { return tournamentSelection; } }
    [SerializeField] private int eliteSelection = 4;  // エリート選択の選択数
    private int EliteSelection { get { return eliteSelection; } }
    [SerializeField][Range(1, 300)] private int nAgents = 4;  // エージェントの数
    private int NAgents { get { return nAgents; } }
    [SerializeField][Range(1, 300)] private int nGeneration = 10;
    private int NGeneration { get { return nGeneration; } }
    [SerializeField] public float targetHeight = 500f;  // 目標高度
    [Header("Agent Prefab"), SerializeField] public GameObject GObjectAgent = null;
    [Header("UI References"), SerializeField] private PopulationTextDisplay textDisplay = null;
    private PopulationTextDisplay TextDisplay { get { return textDisplay; } }
    private float GenBestRecord { get; set; }  // 世代の最大適応度
    private float SumFitness { get; set; }  // 一世代の適応度の合計
    private float AvgFitness { get; set; }  // 一世代の適応度の平均
    private float SumUsedFuel { get; set; }  // 一世代の使用燃料の合計
    private float AvgUsedFuel { get; set; }  // 一世代の使用燃料の平均
    private float SumUsedTime { get; set; }  // 一世代の使用時間の合計
    private float AvgUsedTime { get; set; }  // 一世代の使用時間の平均
    private float Top10AvgUsedFuel { get; set; }  // 上位10%の使用燃料の平均
    private float Top10AvgUsedTime { get; set; }  // 上位10%の使用時間の平均
    private int SucceededAgents { get; set; }  // タスクを完了したエージェントの数
    private List<GameObject> GObjects = new List<GameObject>();  // 生成したゲームオブジェクトを格納するリスト
    private List<Agent> Agents = new List<Agent>();  // 生成したエージェントを格納するリスト <- エージェントの基本的な操作ができる
    private List<Gene> Genes = new List<Gene>();  // 生成した遺伝子を格納するリスト
    public int Generation { get; set; }  // 現在の世代番目
    private float BestRecord { get; set; }  // 全世代の最大適応度
    private List<AgentPair> AgentsSet = new List<AgentPair>();  // 生成したエージェントと遺伝子のペアを格納するリスト
    private Queue<Gene> CurrentGenes;  // 現在の遺伝子を格納するキュー
    [Header("Gene"), SerializeField] private GeneOperator Operator = null;  // 遺伝子操作を行うオペレーター

    private float GenMaxFitness { get; set; }
    private Gene BestGene = new Gene();

    void Awake()
    {
        for (int i = 0; i < TotalPopulation; i++)
        {
            Gene gene = Operator.Init();
            Genes.Add(gene);
        }
        for (int i = 0; i < NAgents; i++)
        {
            var obj = Instantiate(GObjectAgent);
            obj.SetActive(true);
            GObjects.Add(obj);
            Agents.Add(obj.GetComponent<Agent>());
        }
    }

    void Start()
    {
        SetStartAgents();
        UpdateText();
    }

    // Agent,Geneを組としてAgentsSetにいれる
    // GeneをAgentに適用している
    void SetStartAgents()
    {
        CurrentGenes = new Queue<Gene>(Genes);
        AgentsSet.Clear();  // リストの要素を削除
        var size = Math.Min(NAgents, TotalPopulation);
        for (int i = 0; i < size; i++)
        {  // 生成されたAgentとGeneのペアを作成
            AgentsSet.Add(new AgentPair
            {
                agent = Agents[i],
                gene = CurrentGenes.Dequeue()
            });
        }
        foreach (var pair in AgentsSet)
        {
            pair.agent.ApplyGene(pair.gene);  // AgentにGeneを適用
        }
    }

    private float CalcRecord(AgentPair p)
    {
        return p.agent.Fitness;
    }

    // 生きているAgentを更新する
    void FixedUpdate()
    {
        // NGeneration世代目まで終了したら終了
        if (Generation >= NGeneration)
        {
            Debug.Log("Finish");
            WriteBestGene();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;  // エディタの再生を停止
#else
            Application.Quit();  // アプリケーションを終了
#endif
            return;
        }

        foreach (var pair in AgentsSet.Where(p => !p.agent.IsDone))  // IsDoneがfalseのAgentのみ更新
        {
            pair.agent.AgentUpdate();
        }

        AgentsSet.RemoveAll(p =>
        {
            if (p.agent.IsDone)  // IsDoneがtrueのAgentのみ削除
            {
                float fitness = p.agent.Fitness;
                float usedFuel = p.agent.UsedFuel;
                float usedTime = p.agent.UsedTime;
                GenMaxFitness = Mathf.Max(fitness, GenMaxFitness);
                if (CalcRecord(p) > BestRecord) BestGene = p.gene;

                BestRecord = Mathf.Max(CalcRecord(p), BestRecord);
                GenBestRecord = Mathf.Max(CalcRecord(p), GenBestRecord);
                p.gene.Fitness = fitness;  // 遺伝子に適応度を反映
                p.gene.UsedFuel = usedFuel;  // 遺伝子に使用燃料を反映
                p.gene.UsedTime = usedTime;  // 遺伝子に使用時間を反映

                SumFitness += fitness;
                if (p.agent.Succeeded)
                {  // タスクを完了したエージェントに対してのみ計算する
                    SumUsedFuel += usedFuel;
                    SumUsedTime += usedTime;
                    SucceededAgents++;
                }
            }
            return p.agent.IsDone;
        });

        if (CurrentGenes.Count == 0 && AgentsSet.Count == 0)  // 一世代の全ての個体がタスクを終了したら
        {
            Debug.Log($"Generation {Generation + 1} Max Fitness: {GenMaxFitness}");
            SetNextGeneration();
        }
        else
        {
            SetNextAgents();
        }
    }

    private void SetNextAgents()
    {
        int size = Math.Min(NAgents - AgentsSet.Count, CurrentGenes.Count);
        for (var i = 0; i < size; i++)
        {
            var nextAgent = Agents.First(a => a.IsDone);
            var nextGene = CurrentGenes.Dequeue();
            nextAgent.Reset();
            nextAgent.ApplyGene(nextGene);
            AgentsSet.Add(new AgentPair
            {
                agent = nextAgent,
                gene = nextGene
            });
        }
        UpdateText();
    }

    private void SetNextGeneration()
    {
        AvgFitness = SumFitness / TotalPopulation;
        AvgUsedFuel = SumUsedFuel / TotalPopulation;
        AvgUsedTime = SumUsedTime / TotalPopulation;

        // Top10の遺伝子の使用燃料と使用時間の平均を計算
        var top10Genes = Genes.OrderByDescending(g => g.Fitness).Take(10).ToList();
        Top10AvgUsedFuel = top10Genes.Average(g => g.UsedFuel);
        Top10AvgUsedTime = top10Genes.Average(g => g.UsedTime);

        // 新しい世代
        // 新世代の生成と評価値などの初期化を行う
        GenPopulation();
        GenMaxFitness = -1000;
        SumFitness = 0;
        SumUsedFuel = 0;
        SumUsedTime = 0;
        GenBestRecord = -1000;
        SucceededAgents = 0;
        Agents.ForEach(a => a.Reset());
        SetStartAgents();
        UpdateText();
    }

    // 適応度で降順ソートするための適応度比較関数
    private static int CompareGenes(Gene a, Gene b)
    {
        if (a.Fitness > b.Fitness) return -1;
        if (b.Fitness > a.Fitness) return 1;
        return 0;
    }

    // 新世代の生成を選択、交叉、突然変異といった遺伝子操作を加えて行う
    private void GenPopulation()
    {
        var children = new List<Gene>();
        var bestGenes = Genes.ToList();
        // Elite Selection
        bestGenes.Sort(CompareGenes);

        for (int i = 0; i < EliteSelection; i++)
        {
            children.Add(Operator.Clone(bestGenes[i]));  // エリートな遺伝子はそのまま次世代に引き継ぐ
        }

        float mutate_only = 0.3f;

        // トーナメント選択 + 突然変異
        while (children.Count < TotalPopulation * mutate_only)
        {
            var tournamentMembers = Genes.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(TournamentSelection).ToList();
            tournamentMembers.Sort(CompareGenes);
            children.Add(Operator.Mutate(tournamentMembers[0], Generation));
            if (children.Count < TotalPopulation * mutate_only) children.Add(Operator.Mutate(tournamentMembers[1], Generation));
        }

        // トーナメント選択 + 交叉
        while (children.Count < TotalPopulation)
        {
            var tournamentMembers = Genes.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(tournamentSelection).ToList();
            tournamentMembers.Sort(CompareGenes);
            Gene child1, child2;
            (child1, child2) = Operator.Crossover(tournamentMembers[0], tournamentMembers[1], Generation);
            children.Add(child1);
            if (children.Count < TotalPopulation) children.Add(child2);
        }
        Genes = children;
        Generation++;
        WriteRecord();
    }

    private void UpdateText()
    {
        if (TextDisplay != null)
        {
            TextDisplay.UpdateText(TotalPopulation, TotalPopulation - CurrentGenes.Count, Generation, BestRecord, GenBestRecord, SucceededAgents, AvgFitness, AvgUsedFuel, AvgUsedTime, Top10AvgUsedFuel, Top10AvgUsedTime);
        }
        else
        {
            Debug.LogError("TextDisplay is not assigned.");
        }
    }
    private struct AgentPair
    {
        public Agent agent;
        public Gene gene;
    }

    private void WriteRecord()
    {
        StreamWriter file = new StreamWriter(@"test/record.csv", true, Encoding.UTF8);
        file.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Generation, BestRecord, GenBestRecord, SucceededAgents, AvgFitness, AvgUsedFuel, AvgUsedTime, Top10AvgUsedFuel, Top10AvgUsedTime));
        file.Close();
    }

    private void WriteBestGene()
    {
        StreamWriter file = new StreamWriter(@"test/best_gene.txt", false, Encoding.UTF8);
        foreach (var value in BestGene.data)
        {
            file.WriteLine(value);
        }
        file.Close();
    }
}
