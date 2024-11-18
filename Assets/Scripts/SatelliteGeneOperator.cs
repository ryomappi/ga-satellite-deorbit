using UnityEngine;
using System.Collections.Generic;

// 衛星エージェントの遺伝子操作を行うクラス
public class SatelliteGeneOperator : ScriptableObject
{
    // Geneの初期化
    public Gene Init()
    {
        var gene = new Gene();
        return gene;
    }

    // Geneの複製
    public Gene Clone(Gene gene)
    {
        var cloned_gene = new Gene();
        cloned_gene.data = new List<int>(gene.data);
        return cloned_gene;
    }

    // 突然変異
    // 各変数について、変異のおこる確率、変異の大きさを決定するパラメーターをInspectorから設定できる
    public Gene Mutate(Gene gene, int generation)
    {
        var mutated_gene = new Gene();
        return mutated_gene;
    }

    // 交叉
    public (Gene, Gene) CrossOver(Gene gene1, Gene gene2)
    {
        int Length = gene1.data.Count;
        var child_gene1 = new Gene();
        var child_gene2 = new Gene();
        child_gene1.data = new List<int>(gene1.data);
        child_gene2.data = new List<int>(gene2.data);
        for (int i = 0; i < Length; i++)
        {
            if (UnityEngine.Random.value < 0.5f)
            {
                child_gene1.data[i] = gene2.data[i];
                child_gene2.data[i] = gene1.data[i];
            }
        }
        return (child_gene1, child_gene2);
    }

    // 世代によって変化する値
    private float MutRate(int generation)
    {
        return 1;
    }

    // BLX-α交叉
}
