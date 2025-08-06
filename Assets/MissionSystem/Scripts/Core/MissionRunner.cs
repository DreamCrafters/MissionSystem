using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MissionRunner : IDisposable
{
    private readonly MissionConfig _config;
    private readonly Timer _delayTimer;
    private IMission _missionInstance;
    
    public event Action<MissionRunner> OnMissionStarted;
    public event Action<MissionRunner> OnMissionFinished;
    public event Action<MissionRunner> OnMissionPointReached;
    
    public MissionConfig Config => _config;
    public bool IsRunning { get; private set; }
    public bool IsCompleted { get; private set; }
    public string MissionName => _config?.MissionName ?? "Unknown";
    
    public MissionRunner(MissionConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _delayTimer = new Timer();
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
        
        
        if (_config.StartDelay > 0)
        {
            await _delayTimer.StartAsync((int)(_config.StartDelay * 1000));
        }
        
        _missionInstance = _config.CreateMissionInstance();
        if (_missionInstance == null)
        {
            return;
        }
        
        SubscribeToMissionEvents();
        
        IsRunning = true;
        
        _missionInstance.Start();
    }
    
    public void Stop()
    {
        if (!IsRunning)
            return;
        
        _delayTimer.Cancel();
        
        if (_missionInstance != null)
        {
            UnsubscribeFromMissionEvents();
            
            if (_missionInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            if (_missionInstance is MonoBehaviour monoBehaviour)
            {
                GameObject.Destroy(monoBehaviour.gameObject);
            }
            
            _missionInstance = null;
        }
        
        IsRunning = false;
    }
    
    private void SubscribeToMissionEvents()
    {
        if (_missionInstance == null)
            return;
        
        _missionInstance.OnStarted += HandleMissionStarted;
        _missionInstance.OnMissionPointReached += HandleMissionPointReached;
        _missionInstance.OnFinished += HandleMissionFinished;
    }
    
    private void UnsubscribeFromMissionEvents()
    {
        if (_missionInstance == null)
            return;
        
        _missionInstance.OnStarted -= HandleMissionStarted;
        _missionInstance.OnMissionPointReached -= HandleMissionPointReached;
        _missionInstance.OnFinished -= HandleMissionFinished;
    }
    
    private void HandleMissionStarted()
    {
        OnMissionStarted?.Invoke(this);
    }
    
    private void HandleMissionPointReached()
    {
        OnMissionPointReached?.Invoke(this);
    }
    
    private void HandleMissionFinished()
    {
        IsCompleted = true;
        IsRunning = false;
        
        UnsubscribeFromMissionEvents();
        OnMissionFinished?.Invoke(this);
    }
    
    public void Dispose()
    {
        Stop();
        _delayTimer?.Cancel();
    }
}