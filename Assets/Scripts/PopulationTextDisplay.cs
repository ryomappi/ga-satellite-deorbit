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
                    + "\nAverage Used Time: " + AvgUsedTime;

        GUI.Label(new Rect(10, 70, 500, 200), text, style);
    }

    public void UpdateText(int totalPopulation, int currentPopulation, int generation, float bestRecord, float genBestRecord, int succeededAgents, float avgFitness, float avgUsedFuel, float avgUsedTime)
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
    }
}
