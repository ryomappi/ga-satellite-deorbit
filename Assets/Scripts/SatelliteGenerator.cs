using System.Collections.Generic;
using UnityEngine;

public class SatelliteGenerator : MonoBehaviour
{
    public GameObject satellitePrehab;
    public int population = 10; // 衛星エージェントの個体数
    public GameObject earth; // 地球オブジェクト

    private List<GameObject> satellites = new List<GameObject>();

    void Start()
    {
        if (earth == null)
        {
            Debug.LogError("Earth object is not assigned in the inspector.");
            return;
        }

        Transform earthTransform = earth.transform;
        for (int i = 0; i < population; i++)
        {
            GameObject satellite = Instantiate(satellitePrehab);
            satellite.transform.position = new Vector3(0, 0, 1637.8f);
            SatelliteController controller = satellite.GetComponent<SatelliteController>();
            if (controller == null)
            {
                controller = satellite.AddComponent<SatelliteController>();
            }
            controller.satellite = satellite.transform;
            controller.earth = earthTransform;
            satellites.Add(satellite);
        }
    }

}
