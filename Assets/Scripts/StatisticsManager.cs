using System.IO;
using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    private StatisticsData _data;
    private string _savePath;
    private float _sessionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "statistics.json");
        Load();
        _sessionStartTime = Time.time;
    }

    public void RecordAttempt(bool success)
    {
        if (_data == null) return;

        _data.totalAttempts++;
        if (success) _data.successfulAttempts++;
        Save();
    }

    public void RecordCodeLines(int lines)
    {
        if (_data == null) return;

        _data.totalCodeLines += lines;
        Save();
    }

    public void RecordError(string errorType)
    {
        if (_data == null) return;

        switch (errorType)
        {
            case "syntax_error":
                _data.syntaxErrors++;
                break;
            case "runtime_error":
                _data.runtimeErrors++;
                break;
            case "logic_error":
                _data.logicErrors++;
                break;
            default:
                _data.unknownErrors++;
                break;
        }
        Save();
    }

    public StatisticsData GetData()
    {
        return _data ?? new StatisticsData();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                _data = JsonUtility.FromJson<StatisticsData>(json);
            }
            else
            {
                _data = new StatisticsData();
            }
        }
        catch
        {
            _data = new StatisticsData();
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(_savePath, json);
        }
        catch
        {
            Debug.LogWarning("Failed to save statistics");
        }
    }

    private void OnApplicationQuit()
    {
        if (_data != null && _sessionStartTime > 0)
        {
            _data.totalPlayTimeSeconds += Time.time - _sessionStartTime;
            Save();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this && _data != null && _sessionStartTime > 0)
        {
            _data.totalPlayTimeSeconds += Time.time - _sessionStartTime;
            Save();
        }
    }
}