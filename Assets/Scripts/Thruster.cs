using UnityEngine;

public class Thruster : MonoBehaviour
{
    [Header("Thrust (N)"), SerializeField] private float force;  // スラスタの推力 (N = kg * m/s^2)
    private Vector3 direction;  // スラスタの向き
    [Header("Fuel Consumption (kg/s)")] public float fuelConsumption;  // 燃料消費量 (kg/s)

    void Start()
    {
        direction = transform.up;
    }

    public void ApplyForce(Rigidbody rb)
    {
        // forceを単位換算する
        force *= 1e-3f;  // kg * km/s^2
        rb.AddForce(direction * force, ForceMode.Force);
    }
}
