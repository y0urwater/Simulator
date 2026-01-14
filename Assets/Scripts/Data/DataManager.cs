using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum PreSetData
{
    Size,
    AgentNum,
    HazardType,
    Exits,
}

public enum SimulationData
{
    Minute,
    Second,
    Dead,
    Escaped,
    Injury,
    Confusion,
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<PreSetData, object> preData = new Dictionary<PreSetData, object>();

    private Dictionary<SimulationData, object> simulData = new Dictionary<SimulationData, object>();

    public List<SimulationResult> allReports = new List<SimulationResult>();

    public void ClearAllData()
    {
        preData.Clear();
        simulData.Clear();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPreData(PreSetData key, object value)
    {
        preData[key] = value;
    }

    public void SetSimulData(SimulationData key, object value)
    {
        simulData[key] = value;
    }

    public T GetPreData<T>(PreSetData key)
    {
        if (preData.TryGetValue(key, out object value))
        {
            if (value is T convertedValue)
            {
                return convertedValue;
            }
            else
            {
                print($"[DataManager] 키 '{key}'의 값이 요청된 타입 ({typeof(T).Name})과 일치하지 않습니다.");
            }
        }
        return default(T);
    }

    public T GetSimulData<T>(SimulationData key)
    {
        if (simulData.TryGetValue(key, out object value))
        {
            if (value is T convertedValue)
            {
                return convertedValue;
            }
            else
            {
                print($"[DataManager] 키 '{key}'의 값이 요청된 타입 ({typeof(T).Name})과 일치하지 않습니다.");
            }
        }
        return default(T);
    }

    public void SaveReport()
    {
        string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        string fileName = $"Result_{time}.json";
        string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        SimulationResult report = new SimulationResult(
            fileName,
            saveTime,
            GetPreData<int>(PreSetData.AgentNum),
            GetPreData<string>(PreSetData.HazardType),
            GetPreData<List<ExitInfo>>(PreSetData.Exits),
            GetSimulData<int>(SimulationData.Dead),
            GetSimulData<int>(SimulationData.Escaped),
            GetSimulData<float>(SimulationData.Injury),
            GetSimulData<float>(SimulationData.Confusion),
            GetSimulData<int>(SimulationData.Minute),
            GetSimulData<int>(SimulationData.Second));

        string json = JsonConvert.SerializeObject(report, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, report.fileName);
        File.WriteAllText(path, json);
    }

    public List<SimulationResult> LoadAllReports()
    {
        allReports.Clear();

        string path = Application.persistentDataPath;

        if (Directory.Exists(path))
        {
            string[] files = Directory.GetFiles(path, "Result_*.json");

            foreach (string filePath in files)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    SimulationResult report = JsonConvert.DeserializeObject<SimulationResult>(json);

                    allReports.Add(report);
                }
                catch (System.Exception e)
                {
                    print($"파일 로드 실패 ({filePath}): {e.Message}");
                }
            }
        }
        return allReports;
    }

    public void DeleteReport(string fileName)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}

