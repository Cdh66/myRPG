using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyControler
{
    [Header("Skill")]
    public float kickForce = 25;

    public GameObject rockPrefab;

    public Transform handPos;

    //Animation Event
    public void KickOff()
    {
        #region 待修改
        //if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        //{
        //    var targetStats = attackTarget.GetComponent<CharacterStats>();

        //    Vector3 directin = attackTarget.transform.position - transform.position;
        //    directin.Normalize();

        //    targetStats.GetComponent<NavMeshAgent>().isStopped = true;
        //    targetStats.GetComponent<NavMeshAgent>().velocity = directin * kickForce;
        //    attackTarget.GetComponent<Animator>().SetTrigger("dizzy");
        //    targetStats.TakeDamage(characterStats, targetStats);
        //}
        #endregion
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            transform.LookAt(attackTarget.transform);

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    //Animatin Event
    public void ThrowRock()
    {
        if(attackTarget != null)
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
