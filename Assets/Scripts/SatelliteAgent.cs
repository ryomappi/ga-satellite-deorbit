using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class SatelliteAgent : MonoBehaviour
{
    private SatelliteController Controller { get; set; }
    private Vector3 StartPosition { get; set; }
    private Rigidbody SatelliteRb { get; set; }
    public float mass = 100f; // 衛星の質量 (kg)
    public float thrustForce = 0.6f; // 各スラスタの最大推力 (N = kg * m/s^2)
    public float initialVelocity = 7.350103183f; // 初期速度 (km/s)
    private GaEnvironment ga;

    void Awake()
    {
        Controller = GetComponent<SatelliteController>();
        SatelliteRb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        // 衛星の初期位置を設定
        transform.position = new Vector3(12756.0f / 2.0f + 1000f, 0, 0); // 地球の半径 + 衛星の高度 (km)
        Debug.Log("Initial position: " + transform.position);
        // 衛星の質量、初速、重力を設定
        SatelliteRb.mass = mass;
        SatelliteRb.linearVelocity = new Vector3(0, initialVelocity, 0);
        SatelliteRb.useGravity = false;

        // StreamWriter file = new StreamWriter(@"test/record.csv", false, Encoding.UTF8);
        // file.WriteLine(string.Format("{0},{1},{2},{3}", "Generation", "Best Record", "Best this gen", "Average"));
        // Console.WriteLine("ファイルの作成");
        // file.Close();

        // GameObject now_env = GameObject.Find("Environment");
        // ga = now_env.GetComponent<GaEnvironment>();
    }

    // 新しい個体の初期化
    // public override void AgentReset()
    // {
    //     transform.position = new Vector3(12756.0f / 2.0f + 1000f, 0, 0); // 地球の半径 + 衛星の高度 (km)

    // }

    void FixedUpdate()
    {
        // 衛星の位置をログ出力
        Debug.Log("Position: " + transform.position);
    }
}
