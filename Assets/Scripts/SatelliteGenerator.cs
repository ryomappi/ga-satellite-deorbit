using System.Collections.Generic;
using UnityEngine;

public class SatelliteGenerator : MonoBehaviour
{
    public GameObject satellitePrehab;
    public GameObject earth; // 地球オブジェクト
    public int population = 1; // 衛星エージェントの個体数

    void Start()
    {
        if (earth == null)
        {
            Debug.LogError("Earth object is not assigned in the inspector.");
            return;
        }

        Transform earthTransform = earth.transform;
        GameObject satellite = Instantiate(satellitePrehab);
        satellite.SetActive(true);
    }

}
