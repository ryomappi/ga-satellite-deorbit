// 衛星エージェントの遺伝子情報を定義するクラス
using System.Collections.Generic;

public class Gene
{
    /* 衛星のスラスタの制御
        Geneクラスのdataリストの各要素は、ある地球に対する衛星の速度ベクトルの角度の時に上下左右どのスラスタを噴くかに対応しており、
        ここでは11.25度ずつに32分割している。

        各要素の値は、上下左右の各スラスタについて、0: 噴射しない, 1: 噴射する とし、これを上下左右の順に結合した2進数の値を10進数に
        変換したものとなっており、2^4 = 16通りの組み合わせが存在する。
        e.g.) 12 -> 1100 -> 上下のスラスタを噴射する
    */
    public float Fitness { get; set; }  // 適応度
    public float UsedFuel { get; set; }  // 使用した燃料
    public float UsedTime { get; set; }  // 使用した時間
    public bool Succeeded { get; set; }  // タスクを達成したかどうか
    public List<int> data = new List<int>();  // 遺伝子データ
}
