using System.Collections;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyBase owner;

    private void Awake()
    {
        // Auto-find the owner if Init() wasn't called yet
        if (owner == null)
        {
            owner = GetComponentInParent<EnemyBase>();
        }
    }

    public void Init(EnemyBase enemy)
    {
        owner = enemy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        StaticEnemy staticEnemy = owner as StaticEnemy;

        if (staticEnemy != null && staticEnemy.attackDialogue != null)
        {
            if (DialogueManager.Instance.IsDialogueActive) return;

            owner.enabled = false;

            DialogueManager.Instance.StartDialogue(
                staticEnemy.attackDialogue,
                () => { owner.StartCoroutine(DelayedTriggerAttack()); }
            );
        }
        else
        {
            owner.TriggerAttack();
        }
    }

    private IEnumerator DelayedTriggerAttack()
    {
        yield return null;

        owner.enabled = true;

        Debug.Log("Dialogue finished ? triggering attack!");
        owner.TriggerAttack();
    }


}

