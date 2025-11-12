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
        Debug.Log($"=== SetupParty called ===");
        Debug.Log($"Leader: {leader.displayName} (ID: {leader.GetInstanceID()})");
        Debug.Log($"Party members being added:");

        this.leader = leader;
        this.partyMembers.Clear();

        foreach (var def in partyMembers)
        {
            Debug.Log($"  - {def.displayName} (ID: {def.GetInstanceID()}) - Is Leader? {def == leader}");

            if (def == leader) continue;
            if (!this.partyMembers.Contains(def))
                this.partyMembers.Add(def);
        }

        Debug.Log($"Final party setup:");
        Debug.Log($"  Leader: {this.leader.displayName}");
        foreach (var member in this.partyMembers)
        {
            Debug.Log($"  Member: {member.displayName}");
        }
    }

    public NewCharacterDefinition GetLeader() { return leader; }

    public List<NewCharacterDefinition> GetFullParty()
    {
        List<NewCharacterDefinition> full = new List<NewCharacterDefinition>();
        if (leader != null)
            full.Add(leader);

        foreach (var member in partyMembers)
        {
            if (member != null)
                full.Add(member);
        }

        return full;
    }
}
