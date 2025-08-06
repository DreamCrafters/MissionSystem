using System;
using System.Collections;
using UnityEngine;

public class MoveToPointMission : MonoBehaviour, IMission
{
    [Header("Mission Settings")]
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _reachDistance = 0.5f;
    
    [Header("Optional")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private bool _destroyOnComplete = true;
    
    public event Action OnStarted;
    public event Action OnMissionPointReached;
    public event Action OnFinished;
    
    private Transform _player;
    private bool _isRunning;
    
    void IMission.Start()
    {
        StartMission();
    }
    
    private void StartMission()
    {
        Debug.Log($"[MoveToPointMission] Starting mission");
        
        if (_playerObject != null)
        {
            _player = _playerObject.transform;
        }
        else
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        if (_player == null)
        {
            Debug.LogError("[MoveToPointMission] Player not found!");
            OnFinished?.Invoke();
            return;
        }
        
        if (_targetPoint == null)
        {
            GameObject target = new GameObject("MissionTarget");
            target.transform.position = _player.position + new Vector3(
                UnityEngine.Random.Range(-10f, 10f),
                0,
                UnityEngine.Random.Range(-10f, 10f)
            );
            _targetPoint = target.transform;
        }
        
        _isRunning = true;
        OnStarted?.Invoke();
        
        StartCoroutine(MoveToTarget());
    }
    
    private IEnumerator MoveToTarget()
    {
        while (_isRunning && _player != null && _targetPoint != null)
        {
            float distance = Vector3.Distance(_player.position, _targetPoint.position);
            
            if (distance <= _reachDistance)
            {
                Debug.Log("[MoveToPointMission] Target reached!");
                OnMissionPointReached?.Invoke();
                CompleteMission();
                yield break;
            }
            
            Vector3 direction = (_targetPoint.position - _player.position).normalized;
            _player.position += direction * _moveSpeed * Time.deltaTime;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _player.rotation = Quaternion.Slerp(_player.rotation, targetRotation, Time.deltaTime * 5f);
            }
            
            yield return null;
        }
    }
    
    private void CompleteMission()
    {
        _isRunning = false;
        OnFinished?.Invoke();
        
        if (_destroyOnComplete)
        {
            if (_targetPoint != null)
            {
                Destroy(_targetPoint.gameObject);
            }
            Destroy(gameObject, 0.1f);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_targetPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_targetPoint.position, _reachDistance);
            Gizmos.DrawLine(_targetPoint.position, _targetPoint.position + Vector3.up * 3f);
        }
    }
}