using UnityEngine;

public class SatelliteAgent : MonoBehaviour
{
    public float mass = 0.0001f; // 衛星の質量 (kg)
    public float thrustForce = 0.6f; // 各スラスタの最大推力 (N)
    public float initialVelocity = 7.350103183406522f; // 初期速度 (m/s)
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
        transform.position = new Vector3(1275.6f + 7.378f, 0, 0); // 地球の半径 + 衛星の高度 (m)
        Debug.Log("Initial position: " + transform.position);
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
