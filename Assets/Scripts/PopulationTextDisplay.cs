using UnityEngine;

public class PopulationTextDisplay : MonoBehaviour
{
    public int TotalPopulation;
    public int CurrentPopulation;
    public int Generation;
    public float BestRecord;
    public float GenBestRecord;
    public float AvgUsedFuel;

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        string text = "Population: " + CurrentPopulation + "/" + TotalPopulation
                    + "\nGeneration: " + (Generation + 1)
                    + "\nBest Record: " + BestRecord
                    + "\nBest this gen: " + GenBestRecord
                    + "\nAverage: " + AvgUsedFuel;

        GUI.Label(new Rect(10, 70, 500, 200), text, style);
    }

    public void UpdateText(int totalPopulation, int currentPopulation, int generation, float bestRecord, float genBestRecord, float avgUsedFuel)
    {
        TotalPopulation = totalPopulation;
        CurrentPopulation = currentPopulation;
        Generation = generation;
        BestRecord = bestRecord;
        GenBestRecord = genBestRecord;
        AvgUsedFuel = avgUsedFuel;
    }
}
