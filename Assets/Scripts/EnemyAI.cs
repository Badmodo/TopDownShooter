using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static event System.Action OnGuardHasSpottedPlayer;

    public float speed;
    public float wait = 2f;
    public float turnSpeed = 90;
    public float timeToSpotPlayer = .8f;
    public float stoppingDistance = 1f;
    public float retreatDistance = 0.5f;

    public bool oneHanded;
    public bool twoHanded;

    public Transform handHold;

    private bool reloading;
    public Gun currentGun;

    public Light spotLight;
    public float viewDistance;
    public LayerMask viewMask;

    float viewAngle;
    float playerVisableTimer;

    public Transform pathHolder;
    Transform player;
    Color ogiginalSpotlightColour;
    private Vector3[] waypoints;

    public Animator animator;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag ("Player").transform;
        viewAngle = spotLight.spotAngle;
        ogiginalSpotlightColour = spotLight.color;

        //creating an array of all the points in the path
        waypoints = new Vector3[pathHolder.childCount];
        for (int i =0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        FollowThePath();
        animator.SetBool("isWalking", true);
        ////for testing
        //StartCoroutine(Shoot());
    }

    private void Update()
    {
        if(CanSeePlayer())
        {
            playerVisableTimer += Time.deltaTime;
        }
        else
        {
            playerVisableTimer -= Time.deltaTime;
        }
        playerVisableTimer = Mathf.Clamp(playerVisableTimer, 0, timeToSpotPlayer);
        //lerp betqween the original and red if seen for more the a second
        spotLight.color = Color.Lerp(ogiginalSpotlightColour, Color.red, playerVisableTimer / timeToSpotPlayer);

        if(playerVisableTimer >= timeToSpotPlayer)
        {
            //guard has spotted player so call event
            if (OnGuardHasSpottedPlayer != null)
            {
                OnGuardHasSpottedPlayer();
            }
        }
    }

    public void Reloading()
    {        
        if (currentGun.Reload())
        {
             reloading = true;
        }
        
        if (reloading)
        {
            currentGun.FinishReload();
            reloading = false;
        }
    }    

    bool CanSeePlayer ()
    {
        if(Vector3.Distance(transform.position, player.position) < viewAngle)
        {

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if(angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                //check line of sight
                if(!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    //starts shooting as soon as light turns red
                    //StartCoroutine(Shoot());
                    if (Vector3.Distance(transform.position, player.position) > stoppingDistance)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
                    }
                    else if (Vector3.Distance(transform.position, player.position) < stoppingDistance && Vector3.Distance(transform.position, player.position) > retreatDistance)
                    {
                        transform.position = this.transform.position;
                    }
                    else if (Vector3.Distance(transform.position, player.position) > retreatDistance)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, player.position, -speed * Time.deltaTime);
                    }
                    return true;
                }
            }
        }

        return false;
    }

    //corotine to set next waypoint and how long they will stay at that waypoint
    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while(true)
        {
            //move towrads next waypoint at a set speed
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if(transform.position == targetWaypoint)
            {
                //modulas operator or % means if previous value = post value go back to 0
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                //how long the AI will stay at waypoint
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", true);
                yield return new WaitForSeconds(wait);
                animator.SetBool("isIdle", false);
                animator.SetBool("isWalking", true);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;
        }
    }

    //coroutine to set facing the next waypoint
    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(directionToLookTarget.z, directionToLookTarget.x) * Mathf.Rad2Deg;

        //small angle used as sometimes there can be minor variations in calculation in eular angles and it might break
        //aditional mathf.abd was added as the charcter would not rotate anticlockwise as it would be under 0.05, as a - number
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    IEnumerator Shoot()
    {
        if (currentGun.GetComponent<Gun>().currentAmmoInMag >= 0)
        {
            Reloading();
        }
        currentGun.GetComponent<Gun>().Shoot();
        yield return new WaitForSeconds(1f);
        //StartCoroutine(Shoot());
    }

    public void FollowThePath()
    {
        if (CanSeePlayer() == true)
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalking", true);
            StopAllCoroutines();
        }
        else
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isWalking", true);
            StartCoroutine(FollowPath(waypoints));
        }
    }

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
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
