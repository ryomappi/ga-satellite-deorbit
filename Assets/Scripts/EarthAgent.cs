using System.Linq.Expressions;
using UnityEngine;

public class EarthAgent: MonoBehaviour
{
    [Tooltip("Mass of the Earth in kg")]
    public float mass = 5.972e24f; // 地球の質量 (kg)
    [Tooltip("Rotation speed of the Earth in degrees per second")]
    public float rotationSpeed = 10f;

    void Update()
    {
        // Rotate the Earth around the y-axis
        transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
    }
}
