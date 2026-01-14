using System;
using System.Collections.Generic;

[Serializable]
public struct SimulationResult
{
    public string fileName;
    public string saveTime;     

    public int agentNum;         
    public string hazardType;    
    public List<ExitInfo> exits; 

    public int deadCount;        
    public int escapedCount;     
    public float avgInjury;
    public float avgConfusion;
    public int totalMinutes;     
    public int totalSeconds;     

    public SimulationResult(string fileName, string saveTime, int agentNum, string hazardType, List<ExitInfo> exits, int deadCount, int escapedCount, float avgInjury, float avgConfusion, int totalMinutes, int totalSeconds)
    {
        this.fileName = fileName;
        this.saveTime = saveTime;
        this.agentNum = agentNum;
        this.hazardType = hazardType;
        this.exits = (exits != null) ? new List<ExitInfo>(exits) : new List<ExitInfo>(); this.deadCount = deadCount;
        this.escapedCount = escapedCount;
        this.avgInjury = avgInjury;
        this.avgConfusion = avgConfusion;
        this.totalMinutes = totalMinutes;
        this.totalSeconds = totalSeconds;
    }
}
