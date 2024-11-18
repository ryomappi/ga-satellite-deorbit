using UnityEngine;

// 衛星エージェントの制御を行うクラス
public class SatelliteController : MonoBehaviour
{
    public GameObject earth;
    private float gravitationalConstant = 6.67430e-20f; // 万有引力定数 (km^3/kg/s^2)
    private float earthMass;
    private float satelliteMass;
    private bool isGravitating = true;

    void Start()
    {
        earthMass = earth.transform.GetComponent<EarthAgent>().mass;
        satelliteMass = GetComponent<SatelliteAgent>().mass;
    }

    public void Gravitate()
    {
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

    public void Stop()
    {
        // 衛星の動きを止め、万有引力の適用も停止する
        isGravitating = false;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
    void FixedUpdate()
    {
        if (isGravitating == true)
        {
            Gravitate();
        }


        // 角度に応じたスラスタの推力を適用
        // int angleSegment = (int)((Mathf.Atan2(transform.position.z, transform.position.x) * Mathf.Rad2Deg + 360) % 360 / 11.25f);
        // ApplyThrust(angleSegment);
    }

    // スラスタの推力を適用
    void ApplyThrust(int angleSegment)
    {
    }

    // スラスタの推力を適用
    // void ApplyThrust(int angleSegment)
    // {
    //     int thrustState = this.GetComponent<SatelliteAgent>().GetThrustState(angleSegment);

    //     // 0: なし, 1: 点火を表す
    //     if ((thrustState & 8) > 0) ApplyForce(Vector3.forward);   // 上スラスタ
    //     if ((thrustState & 4) > 0) ApplyForce(Vector3.back);      // 下スラスタ
    //     if ((thrustState & 2) > 0) ApplyForce(Vector3.left);      // 左スラスタ
    //     if ((thrustState & 1) > 0) ApplyForce(Vector3.right);     // 右スラスタ
    // }

    // 力を適用
    void ApplyForce(Vector3 direction)
    {
        GetComponent<Rigidbody>().AddForce(direction * this.GetComponent<SatelliteAgent>().thrustForce);
    }
}
