using System.Collections.Generic;
using UnityEngine;

public class PlayerParty : MonoBehaviour
{
    public static PlayerParty instance;

    [Header("Party Members")]
    [SerializeField] private NewCharacterDefinition leader; // selected from character selection
    [SerializeField] private List<NewCharacterDefinition> partyMembers;

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
        this.leader = leader;

        foreach (var def in partyMembers)
        {
            if (def == leader) continue;
            if (!this.partyMembers.Contains(def))
                this.partyMembers.Add(def);
        }
    }

    public NewCharacterDefinition GetLeader()
    {
        return leader;
    }

    public List<NewCharacterDefinition> GetFullParty()
    {
        List<NewCharacterDefinition> full = new List<NewCharacterDefinition>();
        full.Add(leader);
        full.AddRange(partyMembers);
        return full;
    }
}
