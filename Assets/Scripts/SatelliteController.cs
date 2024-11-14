using UnityEngine;

// 衛星エージェントの制御を行うクラス
public class SatelliteController : MonoBehaviour
{
    public Transform satellite;
    public Transform earth;
    private float orbitalSpeed = 8000f; // 初期軌道速度 (m/s)
    private float gravitationalConstant = 6.67430e-11f; // 万有引力定数 (m^3/kg/s^2)

    void FixedUpdate()
    {
        // 地球を中心とした円運動をシミュレーション
        Vector3 directionToEarth = (earth.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, earth.position);

        // 万有引力の計算
        float forceMagnitude = -gravitationalConstant * (earth.GetComponent<EarthAgent>().mass * satellite.GetComponent<SatelliteAgent>().mass) / Mathf.Pow(distance, 2);
        Vector3 force = directionToEarth * forceMagnitude;
        Debug.Log("force: " + force);

        // 角度に応じたスラスタの推力を適用
        int angleSegment = (int)((Mathf.Atan2(transform.position.z, transform.position.x) * Mathf.Rad2Deg + 360) % 360 / 11.25f);
        ApplyThrust(angleSegment);
    }

    // スラスタの推力を適用
    void ApplyThrust(int angleSegment)
    {
        int thrustState = satellite.GetComponent<SatelliteAgent>().GetThrustState(angleSegment);

        // 0: なし, 1: 点火を表す
        if ((thrustState & 8) > 0) ApplyForce(Vector3.forward);   // 上スラスタ
        if ((thrustState & 4) > 0) ApplyForce(Vector3.back);      // 下スラスタ
        if ((thrustState & 2) > 0) ApplyForce(Vector3.left);      // 左スラスタ
        if ((thrustState & 1) > 0) ApplyForce(Vector3.right);     // 右スラスタ
    }

    // 力を適用
    void ApplyForce(Vector3 direction)
    {
        GetComponent<Rigidbody>().AddForce(direction * satellite.GetComponent<SatelliteAgent>().thrustForce);
    }
}
