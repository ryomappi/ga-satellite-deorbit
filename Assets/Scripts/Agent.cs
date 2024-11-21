using UnityEditor.Purchasing;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public bool IsDone { get; private set; }  // エージェントがタスクを完了したかどうか
    public bool IsFrozen { get; private set; }  // エージェントが凍結されているかどうか
    public bool Succeeded { get; private set; }  // エージェントがタスクを達成したかどうか
    public float Fitness { get; private set; }  // エージェントの適応度
    public float SpentTime { get; private set; }  // エージェントがタスク達成に使用した時間
    public float UsedFuel { get; private set; }  // エージェントがタスク達成に使用した燃料
    public float UsedTime { get; private set; }  // エージェントがタスク達成に使用した時間

    public void SetFitness(float fitness)
    {
        Fitness = fitness;
    }
    public void AddFitness(float fitness)
    {
        Fitness += fitness;
    }
    public void SetUsedFuel(float usedFual)
    {
        UsedFuel = usedFual;
    }
    public void AddUsedFuel(float usedFual)
    {
        UsedFuel += usedFual;
    }
    public void SetUsedTime(float usedTime)
    {
        UsedTime = usedTime;
    }
    public void AddUsedTime(float usedTime)
    {
        UsedTime += usedTime;
    }
    public abstract void AgentUpdate();
    public abstract void AgentReset();
    public abstract void ApplyGene(Gene gene);
    public abstract void Stop();
    public abstract void Gravitate();
    public abstract void ApplyThrust(int thrustState);
    public void Done()
    {
        IsDone = true;
    }
    public void Freeze()
    {
        IsFrozen = true;
    }
    public void Reset()
    {
        AgentReset();
        IsDone = false;
        IsFrozen = false;
        Succeeded = false;
    }
    public void Succeed()
    {
        Succeeded = true;
    }
}
