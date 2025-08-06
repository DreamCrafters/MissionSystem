using UnityEngine;

[CreateAssetMenu(fileName = "MissionConfig", menuName = "MissionSystem/Mission Config")]
public class MissionConfig : ScriptableObject
{
    [Header("Mission Settings")]
    [SerializeField] private string _missionName;
    [SerializeField] private string _missionDescription;
    
    [Header("Timing")]
    [SerializeField] private float _startDelay = 0f;
    
    [Header("Mission Implementation")]
    [SerializeField] private GameObject _missionPrefab;
    
    public string MissionName => _missionName;
    public string MissionDescription => _missionDescription;
    public float StartDelay => _startDelay;
    public GameObject MissionPrefab => _missionPrefab;
    
    public IMission CreateMissionInstance()
    {
        if (_missionPrefab == null)
        {
            return null;
        }
        
        GameObject instance = Instantiate(_missionPrefab);
        instance.name = $"Mission_{_missionName}";
        
        if (instance.TryGetComponent<IMission>(out var mission) == false)
        {
            Destroy(instance);
            return null;
        }
        
        return mission;
    }
}