using System.Collections.Generic;
using System.Linq;
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

        //DontDestroyOnLoad(gameObject);
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
    public List<PlayerLevelApplier> GetRuntimeParty()
    {
        List<PlayerLevelApplier> appliers = new List<PlayerLevelApplier>();

        var playerObjs = FindObjectsOfType<PlayerLevelApplier>();
        Debug.Log($"[Party] Found {playerObjs.Length} PlayerLevelAppliers in scene.");

        List<NewCharacterDefinition> fullParty = GetFullParty();

        foreach (var member in fullParty)
        {
            bool found = false;
            foreach (var obj in playerObjs)
            {
                if (obj.definition == member)
                {
                    appliers.Add(obj);
                    found = true;
                    Debug.Log($"[Party] Matched {member.displayName} with {obj.name}");
                    break;
                }
            }

            if (!found)
                Debug.LogWarning($"[Party] Could not find runtime object for {member.displayName}");
        }

        return appliers;
    }

    public NewCharacterDefinition GetDefinitionFor(Combatant unit)
    {
        var applier = unit.GetComponent<PlayerLevelApplier>();
        if (applier && applier.definition != null)
            return applier.definition;

        foreach (var member in GetFullParty())
        {
            if (member == null) continue;
            if (unit.name.Contains(member.displayName))
                return member;
        }
        return null;
    }
    public void ResetPartyPositions(Vector3 checkpointPos)
    {
        GameObject leaderObj = GameObject.FindGameObjectWithTag("Player");
        if (leaderObj == null) return;

        leaderObj.transform.position = checkpointPos;

        List<NewCharacterDefinition> fullParty = GetFullParty();
        Transform lastTarget = leaderObj.transform;

        int index = 0;

        foreach (var def in partyMembers)
        {
            var follower = FindObjectsOfType<PlayerLevelApplier>()
                            .FirstOrDefault(a => a.definition == def);

            if (follower == null)
            {
                Debug.LogWarning($"[Party] Missing runtime follower for {def.displayName}");
                continue;
            }

            Vector3 spawnPos = checkpointPos + new Vector3(-1.2f * (index + 1), 0f, 0f);
            follower.transform.position = spawnPos;

            var pf = follower.GetComponent<PartyFollower>();
            if (pf != null)
                pf.SetTarget(lastTarget);

            lastTarget = follower.transform;
            index++;
        }

        Debug.Log("[Party] Party reset to checkpoint.");
    }

}
