using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MissionChainRunner : IDisposable
{
    private readonly MissionChainConfig _config;
    private readonly Queue<MissionConfig> _missionQueue;
    private readonly Timer _initialDelayTimer;
    
    private MissionRunner _currentMissionRunner;
    private int _currentMissionIndex = -1;
    
    public event Action<MissionChainRunner> OnChainStarted;
    public event Action<MissionChainRunner> OnChainCompleted;
    public event Action<MissionChainRunner, MissionRunner> OnMissionStartedInChain;
    public event Action<MissionChainRunner, MissionRunner> OnMissionCompletedInChain;
    
    public MissionChainConfig Config => _config;
    public bool IsRunning { get; private set; }
    public bool IsCompleted { get; private set; }
    public string ChainName => _config?.ChainName ?? "Unknown";
    public int TotalMissions => _config?.Missions?.Count ?? 0;
    public int CurrentMissionIndex => _currentMissionIndex;
    public MissionRunner CurrentMission => _currentMissionRunner;
    
    public MissionChainRunner(MissionChainConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _missionQueue = new Queue<MissionConfig>();
        _initialDelayTimer = new Timer();
        
        foreach (var missionConfig in _config.Missions)
        {
            _missionQueue.Enqueue(missionConfig);
        }
    }
    
    public async UniTask StartAsync()
    {
        if (IsRunning)
        {
            return;
        }
        
        if (IsCompleted)
        {
            return;
        }
        
        if (!_config.Validate())
        {
            return;
        }
        
        IsRunning = true;
        
        if (_config.InitialDelay > 0)
        {
            await _initialDelayTimer.StartAsync((int)(_config.InitialDelay * 1000));
        }
        
        OnChainStarted?.Invoke(this);
        
        await StartNextMission();
    }
    
    public void Stop()
    {
        if (!IsRunning)
            return;
        
        _initialDelayTimer.Cancel();
        
        if (_currentMissionRunner != null)
        {
            _currentMissionRunner.OnMissionFinished -= HandleCurrentMissionFinished;
            _currentMissionRunner.Stop();
            _currentMissionRunner.Dispose();
            _currentMissionRunner = null;
        }
        
        IsRunning = false;
    }
    
    private async UniTask StartNextMission()
    {
        if (_missionQueue.Count == 0)
        {
            CompleteChain();
            return;
        }
        
        var nextMissionConfig = _missionQueue.Dequeue();
        _currentMissionIndex++;
        
        Debug.Log($"[ChainRunner] Starting mission {_currentMissionIndex + 1}/{TotalMissions} in chain {ChainName}");
        
        _currentMissionRunner = new MissionRunner(nextMissionConfig);
        _currentMissionRunner.OnMissionFinished += HandleCurrentMissionFinished;
        
        OnMissionStartedInChain?.Invoke(this, _currentMissionRunner);
        
        await _currentMissionRunner.StartAsync();
    }
    
    private async void HandleCurrentMissionFinished(MissionRunner missionRunner)
    {
        Debug.Log($"[ChainRunner] Mission completed in chain {ChainName}: {missionRunner.MissionName}");
        
        missionRunner.OnMissionFinished -= HandleCurrentMissionFinished;
        
        OnMissionCompletedInChain?.Invoke(this, missionRunner);
        
        missionRunner.Dispose();
        _currentMissionRunner = null;
        
        if (IsRunning && !IsCompleted)
        {
            await StartNextMission();
        }
    }
    
    private void CompleteChain()
    {
        Debug.Log($"[ChainRunner] Chain completed: {ChainName}");
        
        IsCompleted = true;
        IsRunning = false;
        
        OnChainCompleted?.Invoke(this);
    }
    
    public void Dispose()
    {
        Stop();
        _initialDelayTimer?.Cancel();
        _currentMissionRunner?.Dispose();
    }
}