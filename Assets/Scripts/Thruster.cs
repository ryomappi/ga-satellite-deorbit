using UnityEngine;

public class Thruster : MonoBehaviour
{
    [Header("Thrust (N)"), SerializeField] public float force;  // スラスタの推力 (N = kg * m/s^2)
    public Vector3 direction { get; private set; }  // スラスタの向き
    [Header("Fuel Consumption (kg/s)")] public float fuelConsumption;  // 燃料消費量 (kg/s)

    void Start()
    {
        direction = transform.up;
        force *= 1e-3f;  // N -> kN
    }
}
