using System.Collections.Generic;
using UnityEngine;

public class PlayerParty : MonoBehaviour
{
    public static PlayerParty instance;

    [Header("Party Members")]
    [SerializeField] private NewCharacterDefinition _leader; // selected from character selection
    [SerializeField] private List<NewCharacterDefinition> _partyMembers;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SetupParty(NewCharacterDefinition leader, List<NewCharacterDefinition> partyMembers)
    {
        _leader = leader;
        _partyMembers = new List<NewCharacterDefinition>();

        foreach(var def in partyMembers)
        {
            if (def == leader) continue;
            _partyMembers.Add(def);
        }
    }

    public GameObject GetLeader()
    {
        return _leader.playerPrefab;
    }

    public List<NewCharacterDefinition> GetFullParty()
    {
        List<NewCharacterDefinition> full = new List<NewCharacterDefinition>();
        full.Add(_leader);
        full.AddRange(_partyMembers);
        return full;
    }
}
