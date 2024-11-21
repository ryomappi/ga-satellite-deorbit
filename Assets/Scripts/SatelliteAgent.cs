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
    public Rigidbody SatelliteRb { get; set; }
    public float InitialMass { get; set; }  // 衛星の質量 (kg)
    public float InitialVelocity { get; set; } // 初期速度 (km/s)
    public float InitialHeight { get; set; }  // 初期高度 (km)
    private float TargetHeight = 500f;  // 目標高度 (km)
    [SerializeField] private float CurrentHeight;  // 現在の高度 (km)
    public float MaxHealth = 100f;  // エージェントの最大体力
    [SerializeField] private float Health;  // エージェントの体力
    public float MaxFuel = 20f;  // エージェントの最大燃料
    public float MaxTime = 3600f; // エージェントの最大使用時間
    public bool IsGravitated { get; set; }  // 万有引力を適用するかどうか
    public List<int> GeneData { get; set; }  // 遺伝子データ
    private TrailRenderer TrailRenderer;

    void Awake()
    {
        Controller = GetComponent<SatelliteController>();
        SatelliteRb = GetComponent<Rigidbody>();
        TrailRenderer = GetComponent<TrailRenderer>();
    }
    void Start()
    {
        InitialHeight = 1000f;
        StartPosition = new Vector3(12756.0f / 2.0f + InitialHeight, 0, 0);  // 地球の半径 + 衛星の高度 (km)
        InitialMass = 100f;
        InitialVelocity = 7.350103183f;
        StartVelocity = new Vector3(0, InitialVelocity, 0);
        Health = MaxHealth;
        CurrentHeight = GetCurrentHeight();
        SatelliteRb.useGravity = false;
        IsGravitated = true;

        // 衛星の初期位置を設定
        transform.position = new Vector3(12756.0f / 2.0f + 1000f, 0, 0);
        // 衛星の質量、初速、重力を設定
        SatelliteRb.mass = InitialMass;
        SatelliteRb.linearVelocity = StartVelocity;
    }

    public override void Stop()
    {
        Controller.Stop();
    }
    public override void Gravitate()
    {
        Controller.Gravitate();
    }
    public override void ApplyThrust(int thrustState)
    {
        Controller.ApplyThrust(thrustState);
    }

    // 新しい個体の初期化
    public override void AgentReset()
    {
        transform.position = StartPosition;
        SatelliteRb.mass = InitialMass;
        SatelliteRb.linearVelocity = StartVelocity;
        CurrentHeight = GetCurrentHeight();
        IsGravitated = true;
        Health = MaxHealth;
        MaxFuel = 20f;
        MaxTime = 1000f;

        // TrailRendererをリセット
        if (TrailRenderer != null)
        {
            TrailRenderer.Clear();
        }

        SetFitness(0);
        SetUsedFuel(0);
        SetUsedTime(0);
    }

    // 衛星の現在の高度を取得
    public float GetCurrentHeight()
    {
        return Vector3.Distance(transform.position, Controller.earth.transform.position) - 12756.0f / 2.0f;
    }

    // 地球中心基準での衛星の姿勢の傾きを計算
    public Vector3 GetSatelliteTilt()
    {
        // 地球中心（原点）を基準とした衛星の位置ベクトルを取得
        Vector3 position = transform.position.normalized;

        // 衛星の "up" ベクトルを取得（衛星のローカル座標系での上方向）
        Vector3 satelliteUp = transform.up.normalized;

        // 地球中心基準での衛星の傾きを計算
        float tiltX = Vector3.Dot(satelliteUp, Vector3.right);  // X軸方向の傾き
        float tiltY = Vector3.Dot(satelliteUp, Vector3.up);     // Y軸方向の傾き
        float tiltZ = Vector3.Dot(satelliteUp, Vector3.forward);// Z軸方向の傾き

        // 傾きをベクトルとして返す
        return new Vector3(tiltX, tiltY, tiltZ);
    }

    public int GetTiltSegment()
    {
        // 衛星の傾きベクトルを取得
        Vector3 tilt = GetSatelliteTilt();

        // 任意の軸に対する傾きをセグメント化
        // 例: Z軸方向の傾きを32等分
        float tiltZAngle = Mathf.Atan2(tilt.z, tilt.x) * Mathf.Rad2Deg;
        int segment = (int)((tiltZAngle + 360) % 360 / 11.25f);

        return segment;
    }

    public float CalcFitness()
    {
        /* 適応度の計算
            F(x) = w_1 * \frac{T_current}{T_max} + w_2 * \frac{F_current}{F_max} + w_3 * P
            - T_current: 残り時間
            - T_max: 最大使用時間
            - F_current: 燃料残量
            - F_max: 最大使用燃料
            - P: タスクを達成したかどうか
            - w_1, w_2, w_3: 重み
        */

        float w1 = 0.2f;
        float w2 = 0.8f;
        float w3 = 1;
        float T_current = MaxTime - UsedTime;
        float T_max = MaxTime;
        float F_current = MaxFuel - UsedFuel;
        float F_max = MaxFuel;
        float P = (Succeeded ? 0 : -1) * 1000;  // タスク達成時と失敗時で報酬に大きな差をつける
        return w1 * (T_current / T_max) + w2 * (F_current / F_max) + w3 * P;
    }

    // Agentの更新・終了判定と報酬の更新
    public override void AgentUpdate()
    {
        /*
            更新:
            - 万有引力を適用する
            - 衛星の角度に応じてスラスタを制御する
            - 衛星の高度・角度・質量・使用燃料を更新する
            終了判定: 衛星が目標の高度まで降下したらタスク終了
        */

        // 万有引力を適用
        Gravitate();  // memo: これはSatelliteController.cs内のFixedUpdate()で呼び出してもいいかも

        // 衛星の角度に応じてスラスタを制御 + 質量・使用燃料を更新
        int tiltSegment = GetTiltSegment();
        if (tiltSegment >= 0 && tiltSegment < GeneData.Count)
        {
            int thrustState = GeneData[tiltSegment];  // 0~15の値
            ApplyThrust(thrustState);
        }
        else
        {
            Debug.LogError("Angle segment out of range: " + tiltSegment);
        }

        // 使用時間を更新
        AddUsedTime(Time.fixedDeltaTime);

        // 高度を更新
        float prevHeight = CurrentHeight;
        CurrentHeight = GetCurrentHeight();

        // MaxTimeを超えた場合は終了
        if (UsedTime > MaxTime)
        {
            Stop();
            Done();
            AddFitness(CalcFitness());
            return;
        }

        // 高度が減少していない場合体力を減らす
        if (CurrentHeight >= prevHeight)
        {
            Health -= 0.1f;
        } else
        {
            Health += 0.01f;
        }

        // タスク達成時に終了処理
        if (CurrentHeight < TargetHeight)  // 目標高度: 500km
        {
            Stop();
            Done();
            Succeed();
            AddFitness(CalcFitness());
            return;
        }

        // 体力が0になった場合は終了
        if (Health <= 0)
        {
            Stop();
            Done();
            AddFitness(CalcFitness());
            return;
        }
    }

    // GeneをAgentに適用
    public override void ApplyGene(Gene gene)
    {
        GeneData = gene.data;  // 0~15の値が32個並んだリスト (各傾き角度時のスラスタの状態に対応)
    }
}
