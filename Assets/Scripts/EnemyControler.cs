using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates{ GUARD, PATROL, CHASE, DEAD}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyControler : MonoBehaviour,IEndGameObsever
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    protected CharacterStats characterStats;
    private Collider coll;

    [Header("Basic Settings")]
    public float sightRadius;
    public bool isGuard;
    private float speed;
    protected GameObject attackTarget;
    public float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;
    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 startPoint;

    //配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDie;
    bool playerDead;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<BoxCollider>();
        speed = agent.speed;
        startPoint = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //场景切换后修改
        GameManager.Instance.AddObsever(this);
    }
    //切换场景时启用
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObsever(this);
    //}

    private void OnDisable()
    {
        if (!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObsever(this);
    }

    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
        {
            isDie = true;
        }
        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        
    }

    void SwitchAnimation()
    {
        anim.SetBool("walk", isWalk);
        anim.SetBool("chase", isChase);
        anim.SetBool("follow", isFollow);
        anim.SetBool("critical", characterStats.isCritical);
        anim.SetBool("die", isDie);
    }

    void SwitchStates()
    {
        if (isDie)
        {
            enemyStates = EnemyStates.DEAD;
        }
        //如果发现player，切换到追击的状态
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            //Debug.Log("Find Player!!!");
        }
        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if (transform.position != startPoint)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = startPoint;
                    if (Vector3.SqrMagnitude(startPoint - transform.position) <= agent.stoppingDistance)
                    { 
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;
                //判断是否到了随机巡逻点
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                Chase();
                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
            default:
                break;
        }
    }

    void Chase()
    {
        isWalk = false;
        isChase = true;
        agent.speed = speed;
        //追击Player
        //一定距离后回到之前的状态
        //在攻击范围内则攻击
        if (!FoundPlayer())
        {
            //回到之前的状态
            isFollow = false;
            if (remainLookAtTime >0)
            {
                agent.destination = transform.position;
                remainLookAtTime -= Time.deltaTime;
            }
            else if(isGuard)
            {
                enemyStates = EnemyStates.GUARD;
            }
            else
            {
                enemyStates = EnemyStates.PATROL;
            }

            agent.destination = transform.position;
        }
        else
        {
            isFollow = true;
            agent.isStopped = false;
            agent.destination = attackTarget.transform.position;
        }

        if (TargetInAttackRange()||TargetInSkillRange())
        {
            isFollow = false;
            agent.isStopped = true;
            if (lastAttackTime < 0)
            {
                lastAttackTime = characterStats.attackData.coolDown;

                //暴击判断
                characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                //执行攻击
                Attack();
                
            }
        }
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInSkillRange())
        {
            //技能攻击动画
            anim.SetTrigger("skill");
        }
        if (TargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("attack");
        }
        
    }
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        }
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        }
        else
            return false;
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(startPoint.x + randomX, transform.position.y, startPoint.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    //Animation Event

    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //获胜动画
        //停止所有移动
        //停止Agent
        isChase = false;
        isWalk = false;
        attackTarget = null;
        anim.SetBool("win", true);
        playerDead = true;
        Debug.Log("playerDead" + playerDead);
    }
}
