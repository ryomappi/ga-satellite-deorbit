using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.UI;

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
    [Header("Agent Prefab"), SerializeField] public GameObject GObjectAgent = null;
    [Header("UI References"), SerializeField] private PopulationTextDisplay textDisplay = null;
    private PopulationTextDisplay TextDisplay { get { return textDisplay; } }
    private float GenBestRecord { get; set; }  // 世代の最大適応度
    private float SumFitness { get; set; }  // 一世代の適応度の合計
    private float AvgFitness { get; set; }  // 一世代の適応度の平均
    private float SumUsedFuel { get; set; }  // 一世代の使用燃料の合計
    private float AvgUsedFuel { get; set; }  // 一世代の使用燃料の平均
    private List<GameObject> GObjects = new List<GameObject>();  // 生成したゲームオブジェクトを格納するリスト
    private List<Agent> Agents = new List<Agent>();  // 生成したエージェントを格納するリスト <- エージェントの基本的な操作ができる
    private List<Gene> Genes = new List<Gene>();  // 生成した遺伝子を格納するリスト
    public int Generation { get; set; }  // 現在の世代番目
    private float BestRecord { get; set; }  // 全世代の最大適応度
    private List<AgentPair> AgentsSet = new List<AgentPair>();  // 生成したエージェントと遺伝子のペアを格納するリスト
    private Queue<Gene> CurrentGenes;  // 現在の遺伝子を格納するキュー
    [Header("Gene"), SerializeField] private GeneOperator Operator = null;  // 遺伝子操作を行うオペレーター

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
        /* 点数計算方式
            100点満点方式 (ただし負の点数を取りうる)

            高度
            - 500kmまでさげられていたら100点
            - それ以外は0点
            使用燃料
            - 使用した分だけ減点
        */

        float record = 0f;
        if (p.agent.Succeeded)
        {
            record += 100f;
        }
        record -= p.agent.UsedFuel;

        return record;
    }

    // 生きているAgentを更新する
    void FixedUpdate()
    {
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
                BestRecord = Math.Max(CalcRecord(p), BestRecord);  // 小さいほど良い
                GenBestRecord = Math.Max(CalcRecord(p), GenBestRecord);  // 小さいほど良い
                Debug.Log($"Fitness: {fitness}, UsedFuel: {usedFuel}");
                Debug.Log($"BestRecord: {BestRecord}, GenBestRecord: {GenBestRecord}");
                p.gene.Fitness = fitness;  // 遺伝子に適応度を反映
                p.gene.UsedFuel = usedFuel;  // 遺伝子に使用燃料を反映
                SumFitness += fitness;
                SumUsedFuel += usedFuel;
            }
            return p.agent.IsDone;
        });

        if (CurrentGenes.Count == 0 && AgentsSet.Count == 0)  // 一世代の全ての個体がタスクを終了したら
        {
            SetNextGeneration();
            Debug.Log("Next Generation");
        }
        else
        {
            SetNextAgents();
            Debug.Log("Next Agents");
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
        // 新しい世代
        // 新世代の生成と評価値などの初期化を行う
        GenPopulation();
        SumFitness = 0;
        SumUsedFuel = 0;
        GenBestRecord = 1e5f;  // 小さいほど良いため、大きい値で初期化する必要がある
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
            var tournamentMembers = Genes.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(tournamentSelection).ToList();
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
            TextDisplay.UpdateText(TotalPopulation, TotalPopulation - CurrentGenes.Count, Generation, BestRecord, GenBestRecord, AvgUsedFuel);
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
        file.WriteLine(string.Format("{0},{1},{2},{3}", Generation, BestRecord, GenBestRecord, AvgFitness));
        file.Close();
    }
}
