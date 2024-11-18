using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class SatelliteAgent : Agent
{
    private SatelliteController Controller { get; set; }
    private Vector3 StartPosition { get; set; }
    private Vector3 StartVelocity { get; set; }
    private Rigidbody SatelliteRb { get; set; }
    public float mass { get; set; }  // 衛星の質量 (kg)
    public float thrustForce { get; set; } // 各スラスタの最大推力 (N = kg * m/s^2)
    public float InitialVelocity { get; set; } // 初期速度 (km/s)
    public float CurrentHeight { get; set; }  // 現在の高度 (km)
    public float CurrentMass { get; set; }  // 現在の質量 (kg)
    public float MaxHealth { get; set; }  // エージェントの最大体力
    [SerializeField] private float Health;  // エージェントの体力
    public bool IsGravitated { get; set; }  // 万有引力を適用するかどうか
    public float CurrentAngle { get; set; }  // 地球に対する衛星の角度
    private GaEnvironment ga;

    void Awake()
    {
        Controller = GetComponent<SatelliteController>();
        SatelliteRb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        StartPosition = new Vector3(12756.0f / 2.0f + 1000f, 0, 0);  // 地球の半径 + 衛星の高度 (km)
        mass = 100f;
        thrustForce = 0.6f;
        InitialVelocity = 7.350103183f;
        StartVelocity = new Vector3(0, InitialVelocity, 0);
        Health = MaxHealth;
        SatelliteRb.useGravity = false;
        IsGravitated = true;

        // 衛星の初期位置を設定
        transform.position = new Vector3(12756.0f / 2.0f + 1000f, 0, 0);
        Debug.Log("Initial position: " + transform.position);
        // 衛星の質量、初速、重力を設定
        SatelliteRb.mass = mass;
        SatelliteRb.linearVelocity = StartVelocity;

        StreamWriter file = new StreamWriter(@"test/record.csv", false, Encoding.UTF8);
        file.WriteLine(string.Format("{0},{1},{2},{3}", "Generation", "Best Record", "Best this gen", "Average"));
        Console.WriteLine("ファイルの作成");
        file.Close();

        GameObject now_env = GameObject.Find("Environment");
        ga = now_env.GetComponent<GaEnvironment>();
    }

    public override void Stop()
    {
        Controller.Stop();
    }
    public override void Gravitate()
    {
        Controller.Gravitate();
    }

    // 新しい個体の初期化
    public override void AgentReset()
    {
        transform.position = StartPosition;
        SatelliteRb.mass = mass;
        SatelliteRb.linearVelocity = StartVelocity;
        CurrentHeight = GetCurrentHeight();
        CurrentMass = SatelliteRb.mass;
        CurrentAngle = GetCurrentAngle();
        IsGravitated = true;

        SetFitness(0);
        SetUsedFuel(0);
    }

    // 衛星の現在の高度を取得
    public float GetCurrentHeight()
    {
        return Vector3.Distance(transform.position, Controller.earth.transform.position) - 12756.0f / 2.0f;
    }

    // 衛星の地球に対する現在の角度を取得
    public float GetCurrentAngle()
    {
        return Mathf.Atan2(transform.position.z, transform.position.x) * Mathf.Rad2Deg;
    }

    // angleSegmentを取得
    public int GetAngleSegment()
    {
        return (int)((GetCurrentAngle() + 360) % 360 / 11.25f);
    }

    // Agentの更新・終了判定と報酬の更新
    public override void AgentUpdate()
    {
        /*
            更新:
            - 万有引力を適用する
            - 衛星の角度に応じてスラスタを制御する
            - 衛星の高度・質量・使用燃料を更新する
            終了判定: 衛星が目標の高度まで降下したらタスク終了
        */

        // 万有引力を適用
        Gravitate();

        // 衛星の角度に応じてスラスタを制御 + 使用燃料を更新


        // 衛星の質量を更新

        // 衛星の高度を更新
        float prevHeight = CurrentHeight;
        CurrentHeight = GetCurrentHeight();

        // 高度が減少しない場合、エージェントの体力を減少させる
        if (CurrentHeight >= prevHeight)
        {
            Health -= 1;
        }

        if (CurrentHeight < 500)  // 目標高度: 500km
        {
            Controller.Stop();
            Done();
            return;
        }
    }

    // Geneに応じた衛星のスラスタの制御
    public override void ApplyGene(Gene gene)
    {
        /* 衛星のスラスタの制御
            Geneクラスのdataリストの各要素は、ある地球に対する衛星の姿勢の角度の時に上下左右どのスラスタを噴くかに対応しており、
            ここでは11.25度ずつに32分割している。

            各要素の値は、上下左右の各スラスタについて、0: 噴射しない, 1: 噴射する とし、これを上下左右の順に結合した2進数の値を10進数に
            変換したものとなっており、2^4 = 16通りの組み合わせが存在する。
            e.g.) 12 -> 1100 -> 上下のスラスタを噴射する

            各変数の説明:
            - angleSegment: どの角度の姿勢に対応するかを示すindex
            - thrustState: 上下左右どのスラスタを噴くかを示す
        */
        int angleSegment = (int)((Mathf.Atan2(transform.position.z, transform.position.x) * Mathf.Rad2Deg + 360) % 360 / 11.25f);
        int thrustState = gene.data[angleSegment];
        if (thrustState == 1)
        {
            Vector3 direction = (Controller.earth.transform.position - transform.position).normalized;
            SatelliteRb.AddForce(direction * thrustForce, ForceMode.Force);
            AddUsedFuel(thrustForce);
        }
    }
}
