

// 衛星エージェントの遺伝子情報を定義するクラス
using System.Collections.Generic;

public class Gene
{
    public float Fitness { get; set; } // 適応度
    public float Height { get; set; } // 高さ
    public float RequiredTime; // かかった時間
    public List<int> data = new List<int>(); // 遺伝子データ
}
