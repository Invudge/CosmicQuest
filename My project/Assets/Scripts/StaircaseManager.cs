using UnityEngine;
using System.Collections.Generic;

public class StaircaseManager : MonoBehaviour
{
    public static StaircaseManager Instance;
    private Dictionary<string, bool> unlockedStairs = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsUnlocked(string stairId)
    {
        return unlockedStairs.ContainsKey(stairId) && unlockedStairs[stairId];
    }

    public void Unlock(string stairId)
    {
        if (!unlockedStairs.ContainsKey(stairId)) unlockedStairs[stairId] = false;
        unlockedStairs[stairId] = true;
    }
}