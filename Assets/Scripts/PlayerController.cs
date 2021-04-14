using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float rotationSpeed = 450f;
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -8f;
    public float jumpPower = 3f;

    public float timeRemaining = 10;
    public bool timerIsRunning = false;
    public TMP_Text timeText;

    public bool oneHanded;
    public bool twoHanded;

    public Transform handHold;
    public GameCamera shake;
    public GameObject light;
    public GameObject torchTimer;

    public AudioSource warning;
    public AudioSource torchStart;
    public AudioSource jump;
    public AudioSource step;

    private float acceleration = 5f;
    private Vector3 currentVelocityModifier;    
    private Quaternion targetRotation;

    public Gun[] guns;

    private bool reloading;
    private Gun currentGun;
    private CharacterController controller;
    private Camera _camera;
    private GameGUI gUI;

    public Animator animator;

    public Transform groundCheck;

    public LayerMask groundMask;

    //public static float jumpPower = 3f;
    public float groundDistance = 0.4f;

    public static bool isGrounded = true;

    Vector3 velocity;

    int gunCheckID;

    void Start()
    {
        _camera = Camera.main;
        controller = GetComponent<CharacterController>();

        gUI = GameObject.FindGameObjectWithTag("GUI").GetComponent<GameGUI>();

        //start game with handgun
        Equip(0);
        oneHanded = true;
        animator.SetBool("isIdle", true);
    }

    void Update()
    {
        //two different control types, one follow mouse one using wasd
        ControlMouse();
        //ControlWASD();
        if (currentGun)
        {
            if (Input.GetButtonDown("Shoot"))
            {
                currentGun.Shoot();
                ////camera jumping to original position
                //StartCoroutine(shake.Shake(0.15f, 0.4f));
            }
            else if (Input.GetButton("Shoot"))
            {
                currentGun.ShootContinuous();
            }

            if(Input.GetButtonDown("Reload"))
            {
                if(currentGun.Reload())
                {
                    reloading = true;
                    StartCoroutine(ReloadAnimation());
                }
            }
            if(reloading)
            {
                currentGun.FinishReload();
                reloading = false;
            }
        }

        IEnumerator ReloadAnimation()
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isReloadPistol", true);
            yield return new WaitForSeconds(2f);
            animator.SetBool("isReloadPistol", false);
            animator.SetBool("isIdle", true);
        }

        //using 1 - 9 to change guns depending on what the palyer has
        for (int gunCheckID = 0; gunCheckID < guns.Length; gunCheckID++)
        {   
            if (Input.GetKeyDown((gunCheckID + 1) + "") || Input.GetKeyDown("[" + (gunCheckID + 1) + "]"))
            {
                Equip(gunCheckID);
                break;
            }
        }

        //Jump
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //jump power calculation
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
        }

        //adding gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isJumping", true);
        }
        if (isGrounded == false)
        {
            //jump.Play();
            animator.SetBool("isIdle", false);
            animator.SetBool("isJumping", true);            
        }
        else if (isGrounded == true)
        {
            animator.SetBool("isJumping", false);
        }

        ////to change stance to a different animation 2 handed
        //if (gunCheckID == 1)
        //{
        //    oneHanded = false;
        //    twoHanded = true;
        //}

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(PlayerLight());
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
    }

    IEnumerator PlayerLight()
    {
        torchStart.Play();
        torchTimer.SetActive(true);
        light.SetActive(true);
        timerIsRunning = true;
        yield return new WaitForSeconds(10f);
        warning.Play();
        torchTimer.SetActive(false);
        light.SetActive(false);
        timeRemaining = 10;
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}", seconds);
    }

    //player follows mouse
    void ControlMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _camera.transform.position.y - transform.position.y));
        targetRotation = Quaternion.LookRotation(mousePosition - new Vector3(transform.position.x, 0, transform.position.z));
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        currentVelocityModifier = Vector3.MoveTowards(currentVelocityModifier, input, acceleration * Time.deltaTime);
        Vector3 motion = currentVelocityModifier;
        motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? 0.7f : 1;
        motion *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;
        motion += Vector3.up * gravity;

        controller.Move(motion * Time.deltaTime);

        if (oneHanded == true)
        {
            twoHanded = false;
            //animation
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
            {
                //step.Play();
                animator.SetBool("isIdle", false);
                animator.SetBool("isWalking", true);                
            }
            else
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", true);
            }
        }
        else if (twoHanded == true)
        {
            oneHanded = false;
            //animation
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S))
            {
                animator.SetBool("is2HandedIdle", false);
                animator.SetBool("is2HandedWalking", true);
            }
            else
            {
                animator.SetBool("is2HandedWalking", false);
                animator.SetBool("is2HandedIdle", true);
            }
        }
    }

    //player follows WASD
    void ControlWASD()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (input != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(input);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
        }

        currentVelocityModifier = Vector3.MoveTowards(currentVelocityModifier, input, acceleration * Time.deltaTime);
        Vector3 motion = currentVelocityModifier;
        motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? 0.7f : 1;
        motion *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;
        motion += Vector3.up * gravity;

        controller.Move(motion * Time.deltaTime);
    }

    //equiping a gun to the hand slot!
    void Equip(int i)
    {
        if(currentGun)
        {
            Destroy(currentGun.gameObject);
        }

        currentGun = Instantiate(guns[i], handHold.position, handHold.rotation) as Gun;
        currentGun.transform.parent = handHold;
        currentGun.gUI = gUI;
    }
}
