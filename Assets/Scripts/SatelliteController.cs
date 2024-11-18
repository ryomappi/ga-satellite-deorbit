using UnityEngine;
using UnityEngine.UIElements;

// 衛星エージェントの制御を行うクラス
public class SatelliteController : MonoBehaviour
{
    public GameObject earth;
    private float gravitationalConstant = 6.67430e-20f; // 万有引力定数 (km^3/kg/s^2)
    private float earthMass;
    private float satelliteMass;
    private SatelliteAgent satelliteAgent;
    private Thruster[] thrusters;

    void Start()
    {
        earthMass = earth.transform.GetComponent<EarthAgent>().mass;
        satelliteMass = GetComponent<SatelliteAgent>().mass;
        satelliteAgent = GetComponent<SatelliteAgent>();
        thrusters = GetComponentsInChildren<Thruster>();
    }

    public void Stop()
    {
        // 衛星の動きを止め、万有引力の適用も停止する
        satelliteAgent.IsGravitated = false;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public void Gravitate()
    {
        if (!satelliteAgent.IsGravitated) return;

        // 地球を中心とした円運動をシミュレーション
        Vector3 directionToEarth = (earth.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, earth.transform.position);
        Debug.Log("distance: " + distance);

        // 万有引力の計算
        float forceMagnitude = gravitationalConstant * (earthMass * satelliteMass) / (distance * distance);
        Vector3 force = directionToEarth * forceMagnitude;
        Debug.Log("force: " + force.magnitude);
        GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
    }

    // スラスタの推力を適用
    void ApplyThrust(int thrustState)
    {
        for (int i = 0; i < thrusters.Length; i++)
        {
            if ((thrustState & (1 << i)) != 0)
            {
                thrusters[i].ApplyForce(GetComponent<Rigidbody>());
            }
        }

    }

    // void FixedUpdate()
    // {
    //     if (isGravitating == true)
    //     {
    //         Gravitate(isGravitating);
    //     }
    // }
}
