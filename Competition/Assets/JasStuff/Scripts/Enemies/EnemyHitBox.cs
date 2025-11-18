using System.Collections;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyBase owner;

    public void Init(EnemyBase enemy)
    {
        owner = enemy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        StaticEnemy staticEnemy = owner as StaticEnemy;

        // If this enemy has dialogue, play it FIRST
        if (staticEnemy != null && staticEnemy.attackDialogue != null)
        {
            // Prevent double-trigger
            if (DialogueManager.Instance.IsDialogueActive) return;

            // Disable movement & AI temporarily
            owner.enabled = false;

            // Start dialogue AND wait for it to finish
            DialogueManager.Instance.StartDialogue(
                staticEnemy.attackDialogue,
                () => { owner.StartCoroutine(DelayedTriggerAttack()); }
            );
        }
        else
        {
            // No dialogue ? attack immediately
            owner.TriggerAttack();
        }
    }

    private IEnumerator DelayedTriggerAttack()
    {
        // Wait one frame to ensure dialogue closed
        yield return null;

        // Re-enable enemy logic
        owner.enabled = true;

        Debug.Log("Dialogue finished ? triggering attack!");
        owner.TriggerAttack();
    }


}

