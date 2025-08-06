using System;
using System.Collections;
using UnityEngine;

public class CollectItemsMission : MonoBehaviour, IMission
{
    [Header("Mission Settings")]
    [SerializeField] private string _itemTag = "Collectible";
    [SerializeField] private int _requiredItemCount = 5;
    [SerializeField] private float _collectRadius = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject _collectEffectPrefab;
    [SerializeField] private AudioClip _collectSound;
    
    public event Action OnStarted;
    public event Action OnMissionPointReached;
    public event Action OnFinished;
    
    private int _collectedCount = 0;
    private Transform _player;
    private AudioSource _audioSource;
    
    public int CollectedCount => _collectedCount;
    public int RequiredCount => _requiredItemCount;
    public float Progress => (float)_collectedCount / _requiredItemCount;
    
    void IMission.Start()
    {
        StartMission();
    }
    
    private void StartMission()
    {
        Debug.Log($"[CollectItemsMission] Starting - need to collect {_requiredItemCount} items");
        
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_player == null)
        {
            Debug.LogError("[CollectItemsMission] Player not found!");
            OnFinished?.Invoke();
            return;
        }
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        OnStarted?.Invoke();
        StartCoroutine(CollectionRoutine());
    }
    
    private IEnumerator CollectionRoutine()
    {
        while (_collectedCount < _requiredItemCount)
        {
            GameObject[] collectibles = GameObject.FindGameObjectsWithTag(_itemTag);
            
            foreach (var item in collectibles)
            {
                if (item == null) continue;
                
                float distance = Vector3.Distance(_player.position, item.transform.position);
                if (distance <= _collectRadius)
                {
                    CollectItem(item);
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        CompleteMission();
    }
    
    private void CollectItem(GameObject item)
    {
        _collectedCount++;
        
        Debug.Log($"[CollectItemsMission] Collected {_collectedCount}/{_requiredItemCount}");
        
        if (_collectEffectPrefab != null)
        {
            var effect = Instantiate(_collectEffectPrefab, item.transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        if (_collectSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_collectSound);
        }
        
        Destroy(item);
        
        OnMissionPointReached?.Invoke();
    }
    
    private void CompleteMission()
    {
        Debug.Log("[CollectItemsMission] All items collected!");
        OnFinished?.Invoke();
        Destroy(gameObject, 0.5f);
    }
}