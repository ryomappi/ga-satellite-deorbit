using UnityEngine;

public class SatelliteAgent : MonoBehaviour
{
    public float mass = 100f; // 衛星の質量 (kg)
    public float thrustForce = 0.6f; // 各スラスタの最大推力 (N)
    public float initialVelocity = 2.2027e-6f; // 初期速度 (km/s)
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
        transform.position = new Vector3(0, 1375.6f, 0);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.linearVelocity = new Vector3(initialVelocity, 0, 0);
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
