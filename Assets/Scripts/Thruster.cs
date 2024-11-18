using UnityEngine;

public class Thruster : MonoBehaviour
{
    public float force;  // スラスタの推力 (N = kg * m/s^2)
    private Vector3 direction;  // スラスタの向き
    public float fuelConsumption;  // 燃料消費量 (kg/s)

    void Start()
    {
        direction = transform.up;
    }

    public void ApplyForce(Rigidbody rb)
    {
        rb.AddForce(direction * force, ForceMode.Force);
        Debug.Log($"Thrust {name}: {force}");
    }
}
