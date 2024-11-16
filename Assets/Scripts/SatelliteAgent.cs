using UnityEngine;

public class SatelliteAgent : MonoBehaviour
{
    public float mass = 100f; // 衛星の質量 (kg)
    public float thrustForce = 0.6f; // 各スラスタの最大推力 (N = kg * m/s^2)
    public float initialVelocity = 7.350103183f; // 初期速度 (km/s)
    private int[] gene; // 遺伝子: スラスタ制御を32区分で保持

    void Awake()
    {
        // 遺伝子の初期化
        gene = new int[32];
        for (int i = 0; i < gene.Length; i++)
        {
            gene[i] = Random.Range(0, 16); // 各角度区分に0〜15の4方向スラスタ状態
        }
    }

    void Start()
    {
        // 衛星の初期位置を設定
        transform.position = new Vector3(12756.0f / 2.0f + 1000f, 0, 0); // 地球の半径 + 衛星の高度 (km)
        Debug.Log("Initial position: " + transform.position);
        // 衛星の質量を設定
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.linearVelocity = new Vector3(0, initialVelocity, 0);
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // 衛星の位置をログ出力
        Debug.Log("Position: " + transform.position);
    }

    public int GetThrustState(int angleSegment)
    {
        return gene[angleSegment];
    }

    public void SetGene(int[] newGene)
    {
        gene = newGene;
    }
}
