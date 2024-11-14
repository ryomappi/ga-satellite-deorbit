using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
// 地球GameObjectの生成を行う
public class EarthGenerator : MonoBehaviour
{
    public GameObject earthPrefab;
    void Start()
    {
        GameObject earth = Instantiate(earthPrefab);
        earth.transform.position = Vector3.zero;
    }
}
