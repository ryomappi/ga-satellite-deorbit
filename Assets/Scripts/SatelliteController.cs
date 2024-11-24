using UnityEngine;

// 衛星エージェントの制御を行うクラス
public class SatelliteController : MonoBehaviour
{
    public GameObject earth;
    public float gravitationalConstant = 6.67430e-20f; // 万有引力定数 (km^3/kg/s^2)
    private float earthMass { get; set; }
    private SatelliteAgent satelliteAgent { get; set; }
    private Thruster[] thrusters { get; set; }

    void Start()
    {
        earthMass = earth.transform.GetComponent<EarthAgent>().mass;
        satelliteAgent = GetComponent<SatelliteAgent>();
        thrusters = GetComponentsInChildren<Thruster>();
    }

    public void Stop()
    {
        // 衛星の動きを止め、万有引力の適用も停止する
        satelliteAgent.IsGravitated = false;
        satelliteAgent.SatelliteRb.linearVelocity = Vector3.zero;
        satelliteAgent.SatelliteRb.angularVelocity = Vector3.zero;
    }

    public void Gravitate()
    {
        if (!satelliteAgent.IsGravitated) return;

        // 地球を中心とした円運動をシミュレーション
        Vector3 directionToEarth = (earth.transform.position - satelliteAgent.transform.position).normalized;
        float distance = Vector3.Distance(satelliteAgent.transform.position, earth.transform.position);

        // 万有引力の計算
        float forceMagnitude = gravitationalConstant * (earthMass * satelliteAgent.SatelliteRb.mass) / (distance * distance);
        Vector3 force = directionToEarth * forceMagnitude;
        satelliteAgent.SatelliteRb.AddForce(force, ForceMode.Force);
    }

    // スラスタの推力を適用
    public void ApplyThrust(int thrustState)
    {
        // 燃料がない場合にはスラスタを噴射しない
        if (satelliteAgent.SatelliteRb.mass <= satelliteAgent.InitialMass - satelliteAgent.MaxFuel) return;

        for (int i = 0; i < thrusters.Length; i++)
        {
            if ((thrustState & (1 << i)) != 0)
            {
                if (satelliteAgent.SatelliteRb.mass - thrusters[i].fuelConsumption < satelliteAgent.InitialMass - satelliteAgent.MaxFuel) continue;
                else {
                    // スラスタの推力を適用
                    Vector3 thrustForce = thrusters[i].direction * thrusters[i].force;
                    satelliteAgent.SatelliteRb.AddForce(thrustForce, ForceMode.Force);

                    // スラスタ1個噴くごとに燃料を消費
                    satelliteAgent.SatelliteRb.mass -= thrusters[i].fuelConsumption;  // 衛星の質量を更新
                    satelliteAgent.AddUsedFuel(thrusters[i].fuelConsumption);  // 使用燃料を更新
                }
            }
        }
    }
}
