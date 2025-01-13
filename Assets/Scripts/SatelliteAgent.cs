using System;
using System.Collections.Generic;
using UnityEngine;

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
    public float MaxHealth { get; private set; }  // エージェントの最大体力
    [SerializeField] private float Health;  // エージェントの体力
    public float MaxFuel { get; private set; }  // エージェントの最大燃料 (kg)
    public float MaxTime { get; private set; }  // エージェントの最大使用時間 (s)
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
        InitialMass = 100f;
        MaxHealth = 1000f;
        MaxFuel = InitialMass * 0.2f;
        MaxTime = 3600f * 24;
        InitialVelocity = CalcInitialVelocity();
        StartPosition = new Vector3(12756.0f / 2.0f + InitialHeight, 0, 0);  // 地球の半径 + 衛星の高度 (km)
        StartVelocity = new Vector3(0, InitialVelocity, 0);
        Health = MaxHealth;
        CurrentHeight = GetCurrentHeight();
        SatelliteRb.useGravity = false;
        IsGravitated = true;

        // 衛星の初期位置を設定
        transform.position = StartPosition;
        // 衛星の質量、初速、重力を設定
        SatelliteRb.mass = InitialMass;
        SatelliteRb.linearVelocity = StartVelocity;
    }

    private float CalcInitialVelocity()
    {
        // 衛星の初速を計算する
        // 速度 = sqrt(G * M / r)
        float G = 6.67430e-20f;
        float M = 5.972e24f;
        float r = 12756.0f / 2.0f + InitialHeight;
        float initialVelocity = Mathf.Sqrt(G * M / r);
        return initialVelocity;  // km/s
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
        transform.rotation = Quaternion.identity;
        SatelliteRb.mass = InitialMass;
        SatelliteRb.linearVelocity = StartVelocity;
        SatelliteRb.angularVelocity = Vector3.zero;

        CurrentHeight = GetCurrentHeight();
        IsGravitated = true;
        Health = MaxHealth;

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
        // 衛星の "up" ベクトルを取得（衛星のローカル座標系での上方向）
        Vector3 satelliteUp = transform.up.normalized;

        // 地球中心基準での衛星の傾きを計算
        float tiltX = Vector3.Dot(satelliteUp, Vector3.right);  // X軸方向の傾き
        float tiltY = Vector3.Dot(satelliteUp, Vector3.up);     // Y軸方向の傾き
        // float tiltZ = Vector3.Dot(satelliteUp, Vector3.forward);// Z軸方向の傾き

        // 傾きをベクトルとして返す
        return new Vector3(tiltX, tiltY, 0);
    }

    public int GetTiltSegment()
    {
        // 衛星の傾きベクトルを取得
        Vector3 tilt = GetSatelliteTilt();

        // 傾きベクトルからXY平面上の角度に変換
        float xyAngle = Mathf.Atan2(tilt.y, tilt.x) * Mathf.Rad2Deg;
        int segment = (int)((xyAngle + 180) / 11.25f);

        return segment;
    }

    // 衛星の速度ベクトルの傾きのセグメントを取得
    public int GetVelocitySegment()
    {
        // 衛星の速度ベクトルを取得
        Vector3 velocity = SatelliteRb.linearVelocity;

        // 速度がほぼゼロのとき, セグメントを0番とする
        if (velocity.sqrMagnitude < 1e-6f) {
            return 0;
        }

        // XY平面上の速度の角度を求める
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        // 0 ~ 360度に正規化
        float normalizedAngle = (angle + 360f) % 360f;
        // 11.25度刻みで32分割
        int segment = (int)(normalizedAngle / 11.25f);
        return segment;
    }

    public float CalcFitness()
    {
        /* 適応度の計算
            F(x) = w_1 * \frac{T_current}{T_max} + w_2 * \frac{F_current}{F_max} + w_3 * \frac{D_desc}{D_max}
            - T_current: 残り時間
            - T_max: 最大使用時間
            - F_current: 燃料残量
            - F_max: 最大使用燃料
            - D_desc: 降下距離
            - D_max: 最大降下距離
            - w_1, w_2, w_3: 重み

            値域: 0 <= F(x) <= 1
        */

        float w1 = 0.1f;
        float w2 = 0.3f;
        float w3 = 10f;
        float T_current = MaxTime - UsedTime;
        float T_max = MaxTime;
        float F_current = MaxFuel - UsedFuel;
        float F_max = MaxFuel;
        float D_max = InitialHeight - TargetHeight;  // 最大降下距離
        float D_desc = Mathf.Min(InitialHeight - CurrentHeight, D_max);  // 目標高度までの距離
        return w1 * (T_current / T_max) + w2 * (F_current / F_max) + w3 * (D_desc / D_max);
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
        int velocitySegment = GetVelocitySegment();
        if (velocitySegment >= 0 && velocitySegment < GeneData.Count)
        {
            int thrustState = GeneData[velocitySegment];  // 0~15の値
            ApplyThrust(thrustState);
        }
        else
        {
            Debug.LogError("Angle segment out of range: " + velocitySegment);
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

        // 高度が減少していない場合体力を減らし、減少している場合は体力を回復
        float epsilon = 1e-5f;
        if (Mathf.Abs(CurrentHeight - prevHeight) <= epsilon)
        {
            // なにもしない
        }
        else if (CurrentHeight > prevHeight)
        {
            Health -= 0.1f;
        }
        else
        {
            Health += 0.01f;
            Health = Mathf.Min(Health, MaxHealth);
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
