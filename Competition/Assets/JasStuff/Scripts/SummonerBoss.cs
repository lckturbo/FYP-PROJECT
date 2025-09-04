using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonerBoss : EnemyBase
{
    // summon minions
    // summon -> attack -> summon
    // if more than 5 basic summoned, will stop summoning, resume summoning when 1 basic dies
    protected override void Attack()
    {
        base.Attack();

    }
}
