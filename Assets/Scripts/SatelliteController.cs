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

        // 進行方向に対する相対的な上下左右を再計算する
        Vector3 velocity = satelliteAgent.SatelliteRb.linearVelocity;
        if (velocity.sqrMagnitude < 1e-6f)
        {
            // 速度が極端に小さい場合はスラスタ噴射なし (例)
            return;
        }

        Vector3 forward = velocity.normalized;
        Vector3 right = new Vector3(forward.y, -forward.x, 0).normalized;

        int upBit = (thrustState & 8) >> 3;  // 8 => 1000
        int downBit = (thrustState & 4) >> 2;  // 4 => 0100
        int leftBit = (thrustState & 2) >> 1;  // 2 => 0010
        int rightBit = thrustState & 1;       // 1 => 0001

        // 噴射方向を合成
        Vector3 thrustForce = Vector3.zero;
        thrustForce += (upBit == 1) ? forward * thrusters[0].force : Vector3.zero;
        thrustForce += (downBit == 1) ? -forward * thrusters[0].force : Vector3.zero;
        thrustForce += (leftBit == 1) ? -right * thrusters[0].force : Vector3.zero;
        thrustForce += (rightBit == 1) ? right * thrusters[0].force : Vector3.zero;

        // 使用する燃料を計算
        int activeThrusters = 0;
        for (int i = 0; i < thrusters.Length; i++)
        {
            if ((thrustState & (1 << i)) != 0)
            {
                activeThrusters++;
            }
        }
        float fuelConsumption = thrusters[0].fuelConsumptionRate * Time.fixedDeltaTime * activeThrusters;

        // スラスタの推力を適用
        if (satelliteAgent.SatelliteRb.mass - fuelConsumption < satelliteAgent.InitialMass - satelliteAgent.MaxFuel) return;
        else
        {
            satelliteAgent.SatelliteRb.AddForce(thrustForce, ForceMode.Force);

            // 燃料を消費
            satelliteAgent.SatelliteRb.mass -= fuelConsumption;  // 衛星の質量を更新
            satelliteAgent.AddUsedFuel(fuelConsumption);  // 使用燃料を更新
        }
    }
}
