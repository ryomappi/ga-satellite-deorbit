using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "SatelliteGeneOperator", menuName = "ScriptableObjects/SatelliteGeneOperator")]
// 衛星エージェントの遺伝子操作を行うクラス
public class SatelliteGeneOperator : GeneOperator
{
    [Header("Mutation Variables"), SerializeField] private float PosProb = 0.5f;

    // Geneの初期化
    public override Gene Init()
    {
        var gene = new Gene();
        gene.data = new List<int>();

        // Geneのdataリストを初期化し、各要素に0~15のランダムな整数値を設定する
        for (int i = 0; i < 32; i++)
        {
            gene.data.Add(UnityEngine.Random.Range(0, 16));
        }

        return gene;
    }

    // 突然変異
    // 各変数について、変異のおこる確率、変異の大きさを決定するパラメーターをInspectorから設定できる
    public override Gene Mutate(Gene gene, int generation)
    {
        var mutated_gene = new Gene();
        mutated_gene.data = new List<int>(gene.data);

        float mutrate = MutRate(generation);
        for (int i = 0; i < gene.data.Count; i++)
        {
            if (UnityEngine.Random.value < mutrate)
            {
                mutated_gene.data[i] = MutateClamp(mutated_gene.data[i], mutrate, 0, 15);
            }
        }

        return mutated_gene;
    }

    // 世代によって変動する突然変異率
    private float MutRate(int generation) {
        // 0世代目 1, b世代目に a になる値
        // [0,1] の範囲
        float a = 0.05f;
        float b = 30.0f;
        float baseRate = a + (1.0f - a) * Mathf.Max(0f, 1.0f - generation / b);
        float perturbation = UnityEngine.Random.value < 0.1f ? 0.05f : 0f; // 10%の確率で増加
        return Mathf.Clamp01(baseRate + perturbation);
    }

    private int MutateClamp(int x, float p, int min, int max)
    {
        // 値が取りうる範囲の 100% の範囲で摂動を与える
        // 0 <= p <= 1
        int r = Mathf.Max(1, Mathf.RoundToInt((max - min) * p * PosProb));
        x += UnityEngine.Random.Range(-r, r);
        x = Mathf.Clamp(x, min, max);
        return x;
    }

    // BLX-α交叉は、今回整数GAのため使わないことにする
}
