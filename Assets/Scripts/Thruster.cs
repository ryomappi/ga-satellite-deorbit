using UnityEngine;

public class Thruster : MonoBehaviour
{
    public float force { get; private set; }  // スラスタの推力 (N = kg * m/s^2)
    public Vector3 direction { get; private set; }  // スラスタの向き
    public float isp { get; private set; }  // 比推力 (s)
    public float fuelConsumptionRate { get; private set; }  // 燃料消費率 (kg/s)

    void Start()
    {
        force = 3f;  // 推力 (N)
        isp = 170f;  // 比推力 (s)
        direction = transform.up;
        fuelConsumptionRate = force / (isp * 9.81f);  // kg/s
        force *= 1e-3f;  // N -> kN
    }
}
