using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionChainConfig", menuName = "MissionSystem/Mission Chain Config")]
public class MissionChainConfig : ScriptableObject
{
    [Header("Chain Settings")]
    [SerializeField] private string _chainName;
    [SerializeField] private string _chainDescription;
    
    [Header("Missions")]
    [SerializeField] private List<MissionConfig> _missions = new();
    
    [Header("Chain Options")]
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private float _initialDelay = 0f;
    
    public string ChainName => _chainName;
    public string ChainDescription => _chainDescription;
    public IReadOnlyList<MissionConfig> Missions => _missions;
    public bool AutoStart => _autoStart;
    public float InitialDelay => _initialDelay;
    
    public bool Validate()
    {
        if (_missions == null || _missions.Count == 0)
        {
            return false;
        }
        
        for (int i = 0; i < _missions.Count; i++)
        {
            if (_missions[i] == null)
            {
                return false;
            }
        }
        
        return true;
    }
}