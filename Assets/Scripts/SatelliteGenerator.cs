using UnityEngine;

public class SatelliteGenerator : MonoBehaviour
{
    public GameObject satellitePrehab;
    public int population = 100; // 衛星エージェントの個体数

    void Start()
    {
        for (int i = 0; i < population; i++)
        {
            GameObject satellite = Instantiate(satellitePrehab);
            satellite.transform.position = new Vector3(0, 0, 1637.8f);
        }
    }
}
