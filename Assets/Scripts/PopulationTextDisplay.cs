using UnityEngine;

public class PopulationTextDisplay : MonoBehaviour
{
    public int TotalPopulation;
    public int CurrentPopulation;
    public int Generation;
    public float BestRecord;
    public float GenBestRecord;
    public int SucceededAgents;
    public float AvgFitness;
    public float AvgUsedFuel;
    public float AvgUsedTime;
    public int TopN;
    public float Top10AvgUsedFuel;
    public float Top10AvgUsedTime;

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        string text = "Population: " + CurrentPopulation + "/" + TotalPopulation
                    + "\nGeneration: " + (Generation + 1)
                    + "\nBest Record: " + BestRecord
                    + "\nBest this gen: " + GenBestRecord
                    + "\nSucceeded Agents: " + SucceededAgents
                    + "\nAverage Fitness: " + AvgFitness
                    + "\nAverage Used Fuel: " + AvgUsedFuel
                    + "\nAverage Used Time: " + AvgUsedTime
                    + $"\nTop {TopN} Average Used Fuel: " + Top10AvgUsedFuel
                    + $"\nTop {TopN} Average Used Time: " + Top10AvgUsedTime;

        GUI.Label(new Rect(10, 70, 500, 200), text, style);
    }

    public void UpdateText(int totalPopulation, int currentPopulation, int generation, float bestRecord, float genBestRecord, int succeededAgents, float avgFitness, float avgUsedFuel, float avgUsedTime, int topN, float top10AvgUsedFuel, float top10AvgUsedTime)
    {
        TotalPopulation = totalPopulation;
        CurrentPopulation = currentPopulation;
        Generation = generation;
        BestRecord = bestRecord;
        GenBestRecord = genBestRecord;
        SucceededAgents = succeededAgents;
        AvgFitness = avgFitness;
        AvgUsedFuel = avgUsedFuel;
        AvgUsedTime = avgUsedTime;
        TopN = topN;
        Top10AvgUsedFuel = top10AvgUsedFuel;
        Top10AvgUsedTime = top10AvgUsedTime;
    }
}
