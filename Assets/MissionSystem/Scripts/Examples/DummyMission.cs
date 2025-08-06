using System;
using System.Collections;
using UnityEngine;

public class DummyMission : MonoBehaviour, IMission
{
    [SerializeField] private string _missionId = "Dummy";
    [SerializeField] private float _duration = 2f;
    
    public event Action OnStarted;
    public event Action OnMissionPointReached;
    public event Action OnFinished;
    
    void IMission.Start()
    {
        Debug.Log($"[DummyMission] {_missionId} started");
        OnStarted?.Invoke();
        StartCoroutine(ExecuteMission());
    }
    
    private IEnumerator ExecuteMission()
    {
        yield return new WaitForSeconds(_duration / 2);
        
        Debug.Log($"[DummyMission] {_missionId} halfway");
        OnMissionPointReached?.Invoke();
        
        yield return new WaitForSeconds(_duration / 2);
        
        Debug.Log($"[DummyMission] {_missionId} finished");
        OnFinished?.Invoke();
        
        Destroy(gameObject, 0.1f);
    }
}