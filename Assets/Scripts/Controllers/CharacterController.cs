using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class CharacterController : MonoBehaviour
{
    //prefabs
    [SerializeField] private Transform pf_Bullet;
    private BulletParametersSO currentBullet;

    //init
    public CharacterParametersSO parametersSO;
    private NavMeshAgent agent;

    public delegate void Die(int team);
    public event Die DieEvent;

    //states
    public enum CharacterStates
    {
        Idle,
        Move,
        isMoving,
        Atack,
        Dead,
    }
    public CharacterStates currentState { get; private set; }

    private bool inTower;

    //point for transform positions
    private Vector3? currentMovePoint;
    private Vector3? currentAtackPoint;
    private Vector3? lookPosition;

    //temp var
    private float atackTime;
    private float rotateSpeed = 10f;

    //animations and riggin
    private Animator animator;
    private RigBuilder rigBuilder;
    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private Transform raycastTarget;

    //ragdoll
    private Collider mainCollider;
    private Collider[] ragdollColliders;
    private Rigidbody rb;
    private Rigidbody[] rbs;


    public void Init(CharacterParametersSO charSO)
    {

        parametersSO = charSO;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentState = CharacterStates.Idle;

        mainCollider = GetComponent<Collider>();
        ragdollColliders = GetComponentsInChildren<Collider>(true);
        rb = GetComponent<Rigidbody>();
        rbs = GetComponentsInChildren<Rigidbody>(true);

        rigBuilder = GetComponent<RigBuilder>();
        rigBuilder.enabled = false;

        if (parametersSO.team != 1)
        {
            SetBullet(PlayerControlHandler.instance.bulletParametersSO[0]);
        }

        GetComponentInChildren<SpriteRenderer>().color = parametersSO.team == 1 ? Color.green : Color.red;

        SetRagdol(false);


    }

    public void SetBullet(BulletParametersSO bulletParametersSO)
    {
        currentBullet = bulletParametersSO;
    }

    #region Public methods
    public void SetMovePoint(Vector3? point)
    {
        currentMovePoint = point;
        lookPosition = point;
    }

    public void SetAtackPoint(Vector3? point)
    {
        currentAtackPoint = point;
        lookPosition = point;
    }

    public void SetState(CharacterStates newState)
    {
        currentState = newState;
    }

    public void GetImpact(Vector3 forceDir, Vector3 point)
    {
        StartCoroutine(StartDeath(forceDir, point));      
    }

    #endregion

    IEnumerator StartDeath(Vector3 forceDir, Vector3 point)
    {

        //transform.forward = forceDir;

        rb.constraints = RigidbodyConstraints.None;
        //forceDir = transform.up * 5000;

        //Debug.DrawLine(transform.position, transform.forward * 2, Color.cyan, 5f);
        //Debug.DrawLine(point, point * 2, Color.white, 5f);
        //Debug.DrawLine(forceDir, forceDir + (forceDir.normalized * 2), Color.yellow, 5f);

        rb.AddForceAtPosition(forceDir, point);

        yield return new WaitForSeconds(0.5f);

        SetRagdol(true);
        SetRagdolImpact(forceDir, point);
            

        SetState(CharacterStates.Dead);
    }

    private void SetRagdol(bool value)
    {
        rigBuilder.enabled = false;

        foreach (var rgCollider in ragdollColliders)
        {
            rgCollider.isTrigger = !value;
        }

        foreach (var rbCh in rbs)
        {
            rbCh.useGravity = value;
            rbCh.isKinematic = !value;
        }

        mainCollider.isTrigger = value;
        rb.useGravity = !value;
        rb.isKinematic = value;
        animator.enabled = !value;
    }

    private void SetRagdolImpact(Vector3 forceDir, Vector3 point)
    {
        foreach (var rbCh in rbs)
        {
            rbCh.AddTorque(forceDir);
        }
    }


    private void Update()
    {
        Rotate();

        switch (currentState)
        {
            case CharacterStates.Idle:
                Idle();
                break;
            case CharacterStates.Move:
                Move();
                break;
            case CharacterStates.isMoving:
                isMoving();
                break;
            case CharacterStates.Atack:
                Atack();
                break;
            case CharacterStates.Dead:
                Dead();
                break;
            default:
                break;
        }
    }


    private bool CanShootTarget(Vector3 position, CharacterController other)
    {
        Vector3 direction = (position - raycastTarget.position).normalized;

        Ray ray = new Ray(raycastTarget.position, direction); //look from eyes

        Debug.DrawRay(raycastTarget.position, direction * 100, Color.cyan, 5f);

        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);


        float minDist = Mathf.Infinity;

        RaycastHit currentHit = new RaycastHit();

        foreach (var hit in hits)
        {
            //Debug.Log(hit.collider.name);
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 1f);

            if (hit.collider != null)
            {
                float dist = Vector3.Distance(ray.origin, hit.point);

                if (dist < minDist)
                {
                    minDist = dist;
                    currentHit = hit;
                }  
            }
        }

        CharacterController charCntr = currentHit.collider.GetComponentInParent<CharacterController>();

        if (charCntr != null && charCntr == other)
        {
            Debug.DrawLine(ray.origin, position, Color.red, 5f);
            return true;
        }

        //Debug.DrawLine(ray.origin, position, Color.white, 1f);

        return false;

    }

    private void LookAtTarget()
    {

        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);

        foreach (var collider in colliders)
        {
            CharacterController charCntr = collider.GetComponent<CharacterController>();

            if (charCntr == null) continue;

            if (charCntr.parametersSO.team != parametersSO.team)
            {
                currentAtackPoint = charCntr.transform.position + Vector3.up;

                if (CanShootTarget(currentAtackPoint.Value, charCntr))
                {
                    lookPosition = currentAtackPoint.Value;
                    SetState(CharacterStates.Atack);
                    break;
                    //Debug.Log("target found");
                }
                else
                    currentAtackPoint = null;
                
            }
        }

    }


    #region State machine
    private void Idle()
    {
        //stop moving if someone collide and add force
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rigBuilder.enabled = false;

        if (inTower && currentAtackPoint == null)
        {
           //Debug.Log("look for target");
           LookAtTarget();
        }
    }

    private void Atack()
    {
        if (currentAtackPoint == null) return;

        if (lookPosition != null) return; //we are not in direction towards enemy

        atackTime -= Time.deltaTime;

        if (atackTime > 0) return;

        rigBuilder.enabled = true;

        atackTime = parametersSO.atackSpeed;

        Transform GO = Instantiate(pf_Bullet);
        GO.position = muzzlePosition.transform.position;
        GO.GetComponent<BulletHandler>().Init(currentAtackPoint.Value, parametersSO.team, currentBullet);

        Debug.DrawLine(currentAtackPoint.Value, currentAtackPoint.Value + Vector3.up, Color.white, 1f);
        currentAtackPoint = null;

        rb.velocity = Vector3.zero;
    }

    private void Rotate()
    {

        if (lookPosition == null) return;

        Vector3 rotateDirection = (lookPosition.Value - transform.position).normalized;

        //create the rotation we need to be in to look at the target
        var lookRotation = Quaternion.LookRotation(rotateDirection);

        //rotate us over time according to speed until we are in the required rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);

        float angle = Quaternion.Angle(transform.rotation, lookRotation);

        if (angle <= 15f)
        {
            //Debug.Log("look at target end");
            lookPosition = null;
        }

    }

    private void Move()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rigBuilder.enabled = false;
        animator.SetBool("isMoving", true);
        
        agent.speed = parametersSO.movementSpeed;
        agent.ResetPath();
        agent.SetDestination(currentMovePoint.Value);
        
        SetState(CharacterStates.isMoving);
    }

    private void isMoving()
    {
        rb.angularVelocity = Vector3.zero; // dont loose speed at climbing edges

        float dist = agent.remainingDistance;

        if (dist <= 0.5f)
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isMoving", false);
        SetState(CharacterStates.Idle);
    }

    private void Dead()
    {
        DieEvent?.Invoke(parametersSO.team);
        Destroy(this);
    }

    #endregion


    #region Collision detection

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tower")
        {
            //Debug.Log("in tower mode");
            currentAtackPoint = null;
            inTower = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Tower")
        {
            inTower = false;
        }
    }

    #endregion

}
