using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anything with "Note:" is just my commentary, you can delete them afte reading
public class EnemyAI : MonoBehaviour
{
    //Note: here we use a simple enum state machine.  Ideally this enum has its own seperate script, even if it's just one line, but in this case it doesn't matter
    public enum EnemyStates { Patrol, ChaseAndAttack, Dead }

    public static event System.Action OnGuardHasSpottedPlayer;

    //Note: always have your fields (i.e. v ariables and properties and stuff) in this order: delegates, variables, properties
    //And in each of those categories, have them in this order: static, public/exposed serializeField, protected, private

    [Header("Parameters")] //Note: these headers are nice because they double as a comment 
    [SerializeField] float speed;
    [SerializeField] float turnDuration = 2f;
    [SerializeField] float turnSpeed = 90;
    [SerializeField] float timeToSpotPlayer = .8f;
    [SerializeField] float shootDistMax = 1f;
    [SerializeField] float shootDistMin = 0.5f;

    [SerializeField] bool oneHanded;
    [SerializeField] bool twoHanded;

    [SerializeField] Gun gun;

    [SerializeField] Light spotLight;
    [SerializeField] float spotlightDistance;
    [SerializeField] LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] Transform pathHolder;
    [SerializeField] Transform handHold;
    [SerializeField] Animator animator;

    //References
    Transform player;

    //Status (note: statuses are variables that constantly change during a gameplay session)
    bool reloading;
    float playerVisableTimer;
    float distanceToPlayer;
    int waypointIndex;

    //Cache (note: cache are things that gets calculated once at the start of the game and don't get changed again)
    float spotlightViewAngle;
    Color originalSpotlightColour;
    Vector3[] waypoints;

    public EnemyStates State { get; private set; } = EnemyStates.Patrol;

    #region MonoBehavior
    void Awake()
    {
        //Note: Awake / Start usually have 3 categories or sections of code: reference (ie. referencing objects, components, and classes), 
        //initialize (i.e. giving variables their starting values), and cache (i.e. caching some calculation so they don't have to be recalculated for the rest of the session
        //Reference
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //Cache
        spotlightViewAngle = spotLight.spotAngle;
        originalSpotlightColour = spotLight.color;
        spotlightDistance = spotLight.range;

        //Initialize waypoints
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        //Startup behaviors
        PlayWalkAnimation();

        transform.position = waypoints[0];
        transform.LookAt(currentWaypoint);
    }


    private void Update()
    {
        //Note: this is a kind of "per-frame" cache. 
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        StateUpdate();
        SpotlightUpdate();
    }
    #endregion

    void SpotlightUpdate()
    {
        if (State == EnemyStates.ChaseAndAttack)
        {
            playerVisableTimer += Time.deltaTime;
        }
        else
        {
            playerVisableTimer -= Time.deltaTime;
        }
        playerVisableTimer = Mathf.Clamp(playerVisableTimer, 0, timeToSpotPlayer);

        //lerp betqween the original and red if seen for more the a second
        spotLight.color = Color.Lerp(originalSpotlightColour, Color.red, playerVisableTimer / timeToSpotPlayer);

        if (playerVisableTimer >= timeToSpotPlayer)
        {
            //guard has spotted player so call event
            if (OnGuardHasSpottedPlayer != null)
            {
                OnGuardHasSpottedPlayer();
            }
        }
    }


    #region State machine
    void StateUpdate()
    {
        switch (State)
        {
            case EnemyStates.Patrol:
                PatrolUpdate();
                break;
            case EnemyStates.ChaseAndAttack:
                ChaseUpdate();
                break;
            case EnemyStates.Dead:
            default:
                //Do nothing 
                break;
        }
    }
    #endregion

    #region Patrol
    void PatrolUpdate()
    {
        if (IsPlayerInDetectionRange && HasLineOfSightToPlayer)
        {
            //Exit patrol state and chase
            State = EnemyStates.ChaseAndAttack;
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (ArrivedAtWaypoint)
            TurnTowardsNextWaypoint();
        else
            MoveToNextWaypoint();
    }

    void TurnTowardsNextWaypoint()
    {
        PlayIdleAnimation();
        if (TurnToFaceTarget(nextWaypoint))
        {
            IncrementWaypointIndex();
        }
    }


    void MoveToNextWaypoint()
    {
        PlayWalkAnimation();
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
    }
    #endregion

    #region Chase and attack
    void ChaseUpdate()
    {
        if (IsRoughlyFacingPlayer())
        {
            TurnToFaceTarget(player.position);
            if (distanceToPlayer > shootDistMax)
            {
                PlayWalkAnimation();
                transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
            else
            {
                PlayIdleAnimation();
                Shoot();
            }
        }
        else
        {
            PlayIdleAnimation();
            TurnToFaceTarget(player.position);
        }
    }

    void Shoot()
    {
        gun.Shoot();

        //if (gun.currentAmmoInMag <= 0)
        //{
        //    Reloading();
        //}
        //else
        //{
        //    gun.Shoot();
        //}
    }

    public void Reloading()
    {
        if (gun.Reload())
        {
            reloading = true;
        }

        if (reloading)
        {
            gun.FinishReload();
            reloading = false;
        }
    }
    #endregion

    #region Rotation
    bool TurnToFaceTarget(Vector3 target)
    {
        Vector3 tgtDir = target - transform.position;

        Quaternion tgtRot = Quaternion.LookRotation(tgtDir, Vector3.up);

        //If rotation completed, then snap towards the target rotation, otherwise smooth rotate towards it
        if (Vector3.Angle(transform.forward, tgtDir) < 5f)
        {
            transform.rotation = tgtRot;
            return true;
        }
        else
        {
            Vector3 curDir = Vector3.RotateTowards(transform.forward, tgtDir, turnSpeed * Time.deltaTime, 0.0f).normalized;
            Quaternion curRot = Quaternion.LookRotation(curDir, Vector3.up);

            transform.rotation = curRot;

            return false;
        }
    }
    #endregion

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(20, 20, 200, 20), "State: " + State);
    //    GUI.Label(new Rect(20, 40, 200, 20), "ArrivedAtWaypoint: " + ArrivedAtWaypoint);
    //    GUI.Label(new Rect(20, 60, 200, 20), "WaypointIndex: " + waypointIndex);
    //    GUI.Label(new Rect(20, 80, 200, 20), "nextWaypoint: " + nextWaypoint);
    //    GUI.Label(new Rect(20, 100, 200, 20), "waypoints.Length: " + waypoints.Length);
    //}

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            //draw spheres and join them in scene view
            Gizmos.DrawSphere(waypoint.position, 0.3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }

        //to connect the last and first waypoint
        Gizmos.DrawLine(previousPosition, startPosition);

        //visualise the spotlight
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * spotlightDistance);
    }

    #region Minor methods
    //These kind of properties and methods produce "self documenting code", which is a good practice as they replaces comments and make the code more readable.
    //For such kind of properties, you can put them at the bottom of the script or at the top along with other properties. 
    //Personally, I have all of the utility methods/properties and these types of self documentation methods/properties all the bottom of the script so that 
    //I don't have to look at them ever again unless something goes wrong.
    Vector3 directionToPlayer => player.position - transform.position;
    bool IsPlayerInDetectionRange => distanceToPlayer < spotlightDistance;
    float DistToWaypoint => Vector3.Distance(transform.position, currentWaypoint);
    Vector3 currentWaypoint => waypoints[waypointIndex];
    Vector3 nextWaypoint => waypoints[(waypointIndex + 1 < waypoints.Length) ? waypointIndex + 1 : 0];
    bool ArrivedAtWaypoint => transform.position == currentWaypoint;

    void PlayWalkAnimation()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);
    }

    void PlayIdleAnimation()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
    }

    void IncrementWaypointIndex()
    {
        //modulas operator or % means if previous value = post value go back to 0
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    bool IsRoughlyFacingPlayer()
    {
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        return (angleToPlayer < spotlightViewAngle / 2f);
    }

    bool IsFacingTarget(Vector3 target)
    {
        float angleToTarget = Vector3.Angle(transform.forward, target);
        return angleToTarget < 5f;
    }

    bool HasLineOfSightToPlayer => !Physics.Linecast(transform.position, player.position, obstacleLayer);

    #endregion
}

/*
     IEnumerator TurnToFaceTarget(Vector3 target)
    {
        Vector3 lookDir = (target - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg;

        //small angle used as sometimes there can be minor variations in calculation in eular angles and it might break
        //aditional mathf.abd was added as the charcter would not rotate anticlockwise as it would be under 0.05, as a - number
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }
 */

/*
     void RunAwayFromPlayer()
    {
        PlayWalkAnimation();
        Vector3 opposite = transform.position;
        opposite.x *= -1;
        opposite.z *= -1;

        if (IsFacingTarget(opposite))
        {
            PlayIdleAnimation();
            TurnToFaceTarget(opposite);
        }
        else
        {
            PlayWalkAnimation();
            transform.position = Vector3.MoveTowards(transform.position, opposite, speed * Time.deltaTime);
        }
    }
 */