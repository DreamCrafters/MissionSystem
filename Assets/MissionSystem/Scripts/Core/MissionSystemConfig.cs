using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionSystemConfig", menuName = "MissionSystem/Mission System Config")]
public class MissionSystemConfig : ScriptableObject
{
    [Header("System Settings")]
    [SerializeField] private string _systemName = "Mission System";
    
    [Header("Mission Chains")]
    [SerializeField] private List<MissionChainConfig> _parallelChains = new();
    
    [Header("System Options")]
    [SerializeField] private bool _autoStartOnLoad = true;
    [SerializeField] private bool _allowSimultaneousChains = true;
    
    public string SystemName => _systemName;
    public IReadOnlyList<MissionChainConfig> ParallelChains => _parallelChains;
    public bool AutoStartOnLoad => _autoStartOnLoad;
    public bool AllowSimultaneousChains => _allowSimultaneousChains;
    
    public bool Validate()
    {
        if (_parallelChains == null || _parallelChains.Count == 0)
        {
            return false;
        }
        
        bool isValid = true;
        for (int i = 0; i < _parallelChains.Count; i++)
        {
            if (_parallelChains[i] == null)
            {
                isValid = false;
            }
            else if (!_parallelChains[i].Validate())
            {
                isValid = false;
            }
        }
        
        return isValid;
    }
}