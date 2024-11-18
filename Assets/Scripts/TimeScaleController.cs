using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    private float timeScale = 1.0f; // 初期値は1.0 (通常速度)

    void OnGUI()
    {
        // スライダーを描画
        GUI.Label(new Rect(10, 10, 200, 20), "Time Scale: " + timeScale.ToString("F2"));
        timeScale = GUI.HorizontalSlider(new Rect(10, 40, 200, 20), timeScale, 0.1f, 100.0f);

        // TimeScale を更新
        Time.timeScale = timeScale;
    }
}
