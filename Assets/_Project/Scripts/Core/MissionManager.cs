using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MissionManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MissionSystemConfig _systemConfig;
    
    [Header("Runtime Info")]
    [SerializeField] private bool _showDebugInfo = true;
    
    private readonly List<MissionChainRunner> _activeChains = new();
    private readonly Dictionary<string, MissionChainRunner> _chainsByName = new();
    
    public event Action<MissionChainRunner> OnChainStarted;
    public event Action<MissionChainRunner> OnChainCompleted;
    public event Action<MissionRunner> OnAnyMissionStarted;
    public event Action<MissionRunner> OnAnyMissionCompleted;
    
    public bool IsSystemRunning { get; private set; }
    public int ActiveChainsCount => _activeChains.Count;
    public IReadOnlyList<MissionChainRunner> ActiveChains => _activeChains;
    
    private void Start()
    {
        if (_systemConfig != null && _systemConfig.AutoStartOnLoad)
        {
            StartMissionSystem();
        }
    }
    
    public void StartMissionSystem()
    {
        if (IsSystemRunning)
        {
            Debug.LogWarning("Mission system is already running");
            return;
        }
        
        if (_systemConfig == null)
        {
            Debug.LogError("Mission system config is not set");
            return;
        }
        
        if (!_systemConfig.Validate())
        {
            Debug.LogError("Mission system config validation failed");
            return;
        }
        
        Debug.Log($"[MissionManager] Starting mission system: {_systemConfig.SystemName}");
        IsSystemRunning = true;
        
        StartParallelChains();
    }
    
    public void StopMissionSystem()
    {
        if (!IsSystemRunning)
            return;
        
        Debug.Log("[MissionManager] Stopping mission system");
        
        foreach (var chain in _activeChains)
        {
            chain.OnChainCompleted -= HandleChainCompleted;
            chain.OnMissionStartedInChain -= HandleMissionStartedInChain;
            chain.OnMissionCompletedInChain -= HandleMissionCompletedInChain;
            chain.Stop();
            chain.Dispose();
        }
        
        _activeChains.Clear();
        _chainsByName.Clear();
        IsSystemRunning = false;
    }
    
    private async void StartParallelChains()
    {
        if (!_systemConfig.AllowSimultaneousChains)
        {
            Debug.LogWarning("Simultaneous chains are not allowed in config");
            return;
        }
        
        var chains = _systemConfig.ParallelChains;
        Debug.Log($"[MissionManager] Starting {chains.Count} parallel chains");
        
        List<UniTask> chainTasks = new List<UniTask>();
        
        foreach (var chainConfig in chains)
        {
            if (chainConfig == null || !chainConfig.Validate())
            {
                Debug.LogError($"Invalid chain config, skipping");
                continue;
            }
            
            var chainRunner = new MissionChainRunner(chainConfig);
            
            chainRunner.OnChainCompleted += HandleChainCompleted;
            chainRunner.OnMissionStartedInChain += HandleMissionStartedInChain;
            chainRunner.OnMissionCompletedInChain += HandleMissionCompletedInChain;
            
            _activeChains.Add(chainRunner);
            _chainsByName[chainConfig.ChainName] = chainRunner;
            
            OnChainStarted?.Invoke(chainRunner);
            
            if (chainConfig.AutoStart)
            {
                chainTasks.Add(chainRunner.StartAsync());
            }
        }
        
        if (chainTasks.Count > 0)
        {
            await UniTask.WhenAll(chainTasks);
        }
    }
    
    public async UniTask StartChain(string chainName)
    {
        if (_chainsByName.TryGetValue(chainName, out var chain))
        {
            await chain.StartAsync();
        }
        else
        {
            Debug.LogError($"Chain not found: {chainName}");
        }
    }
    
    public void StopChain(string chainName)
    {
        if (_chainsByName.TryGetValue(chainName, out var chain))
        {
            chain.Stop();
        }
        else
        {
            Debug.LogError($"Chain not found: {chainName}");
        }
    }
    
    public MissionChainRunner GetChain(string chainName)
    {
        _chainsByName.TryGetValue(chainName, out var chain);
        return chain;
    }
    
    private void HandleChainCompleted(MissionChainRunner chain)
    {
        Debug.Log($"[MissionManager] Chain completed: {chain.ChainName}");
        
        _activeChains.Remove(chain);
        _chainsByName.Remove(chain.ChainName);
        
        OnChainCompleted?.Invoke(chain);
        
        if (_activeChains.Count == 0)
        {
            Debug.Log("[MissionManager] All chains completed!");
            IsSystemRunning = false;
        }
    }
    
    private void HandleMissionStartedInChain(MissionChainRunner chain, MissionRunner mission)
    {
        Debug.Log($"[MissionManager] Mission started: {mission.MissionName} in chain: {chain.ChainName}");
        OnAnyMissionStarted?.Invoke(mission);
    }
    
    private void HandleMissionCompletedInChain(MissionChainRunner chain, MissionRunner mission)
    {
        Debug.Log($"[MissionManager] Mission completed: {mission.MissionName} in chain: {chain.ChainName}");
        OnAnyMissionCompleted?.Invoke(mission);
    }
    
    private void OnDestroy()
    {
        StopMissionSystem();
    }
    
    private void OnGUI()
    {
        if (!_showDebugInfo || !IsSystemRunning)
            return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 500));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"Mission System: {_systemConfig?.SystemName ?? "Unknown"}");
        GUILayout.Label($"Active Chains: {ActiveChainsCount}");
        
        GUILayout.Space(10);
        
        foreach (var chain in _activeChains)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Chain: {chain.ChainName}");
            GUILayout.Label($"Status: {(chain.IsRunning ? "Running" : chain.IsCompleted ? "Completed" : "Idle")}");
            GUILayout.Label($"Progress: {chain.CurrentMissionIndex + 1}/{chain.TotalMissions}");
            
            if (chain.CurrentMission != null)
            {
                GUILayout.Label($"Current Mission: {chain.CurrentMission.MissionName}");
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}