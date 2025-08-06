using System;
using System.Collections;
using UnityEngine;

public class SurvivalMission : MonoBehaviour, IMission
{
    [Header("Mission Settings")]
    [SerializeField] private float _survivalDuration = 60f;
    [SerializeField] private bool _showTimer = true;
    
    [Header("Checkpoints")]
    [SerializeField] private float[] _checkpointTimes = { 15f, 30f, 45f };
    
    public event Action OnStarted;
    public event Action OnMissionPointReached;
    public event Action OnFinished;
    
    private float _elapsedTime = 0f;
    private int _currentCheckpoint = 0;
    private bool _isRunning = false;
    
    public float ElapsedTime => _elapsedTime;
    public float RemainingTime => Mathf.Max(0, _survivalDuration - _elapsedTime);
    public float Progress => _elapsedTime / _survivalDuration;
    
    void IMission.Start()
    {
        StartMission();
    }
    
    private void StartMission()
    {
        Debug.Log($"[SurvivalMission] Starting - survive for {_survivalDuration} seconds");
        
        _isRunning = true;
        _elapsedTime = 0f;
        _currentCheckpoint = 0;
        
        OnStarted?.Invoke();
        StartCoroutine(SurvivalRoutine());
    }
    
    private IEnumerator SurvivalRoutine()
    {
        while (_isRunning && _elapsedTime < _survivalDuration)
        {
            _elapsedTime += Time.deltaTime;
            
            if (_currentCheckpoint < _checkpointTimes.Length && 
                _elapsedTime >= _checkpointTimes[_currentCheckpoint])
            {
                Debug.Log($"[SurvivalMission] Checkpoint reached: {_checkpointTimes[_currentCheckpoint]}s");
                _currentCheckpoint++;
                OnMissionPointReached?.Invoke();
            }
            
            yield return null;
        }
        
        if (_isRunning)
        {
            CompleteMission();
        }
    }
    
    private void CompleteMission()
    {
        _isRunning = false;
        Debug.Log($"[SurvivalMission] Survived for {_survivalDuration} seconds!");
        OnFinished?.Invoke();
        Destroy(gameObject, 0.5f);
    }
    
    private void OnGUI()
    {
        if (!_showTimer || !_isRunning) return;
        
        GUI.Box(new Rect(Screen.width / 2 - 100, 20, 200, 30), 
            $"Survive: {RemainingTime:F1}s");
    }
}