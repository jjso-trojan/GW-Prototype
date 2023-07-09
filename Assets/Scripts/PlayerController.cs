using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine.InputSystem;
using UnityEngine;
using System.IO;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private Rigidbody rb;
    private float horizontal;
    private float vertical;
    private bool grounded = false;
    private bool bhop = false;
    private float groundDrag;
    private Vector3 moveDirection;
    private Transform cameraTransform;

    [SerializeField]
    private Transform aim;
    [SerializeField]
    private Transform orientation;
    [SerializeField]
    private Transform originTransform;
    [SerializeField]
    private Transform spellParent;
    [SerializeField]
    private Transform centerTransform;
    [SerializeField]
    private Transform spellAimTransform;

    // SPELLS
    [SerializeField]
    private GameObject normalSpellPrefab;
    [SerializeField]
    private GameObject airSpellPrefab;
    [SerializeField]
    private GameObject dashSpellPrefab;
    [SerializeField]
    private GameObject groundSpellPrefab;
    [SerializeField]
    private GameObject focusSpellPrefab;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float momentum = 1.0f;
    [SerializeField]
    private float maxMom;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float aircontrol;
    [SerializeField]
    private float momentumClimb;

    private float rotSpeed = 100f;

    private float dashCooldown = 0.4f;
    private bool dashReset;
    private bool dashEnd;
    private bool crouched = false;
    private bool slam = false;
    private bool focus = false;
    private Vector3 vec;

    private InputAction shootAction;


    enum State
    {
        NORMAL,
        AERIAL,
        GROUND,
        DASH,
        FOCUS
    }
    State playerState = State.NORMAL;

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        groundDrag = 5f;
        shootAction = playerInput.actions["Cast"];
    }

    private void OnEnable()
    {
        shootAction.performed += _ => startShoot();
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => startShoot();
    }

    private void startShoot()
    {
        GameObject spell;
        if (playerState == State.NORMAL)
        {
            spell = GameObject.Instantiate(normalSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
        }
        else if(playerState == State.AERIAL)
        {
            spell = GameObject.Instantiate(airSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
        }
        else if (playerState == State.GROUND)
        {
            spell = GameObject.Instantiate(groundSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
        }
        else if (playerState == State.DASH)
        {
            spell = GameObject.Instantiate(dashSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
        }
        else
        {
            spell = GameObject.Instantiate(focusSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
        }
        Spell sp = spell.GetComponent<Spell>();
        sp.transform.forward = orientation.forward;
        sp.dir = transform.forward;
        //sp.aimDirect = (transform.forward + sp.aim);
        //UnityEngine.Debug.Log(sp.end);

        Rigidbody srb = spell.GetComponent<Rigidbody>();
        srb.velocity = (vec - originTransform.position).normalized * sp.travelTime;
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        SpeedControl();

        RaycastHit hit;
        
        if (Physics.Raycast(centerTransform.position, (spellAimTransform.position - centerTransform.position).normalized, out hit, 1000f))
        {
            UnityEngine.Debug.DrawRay(originTransform.position, (hit.point - originTransform.position).normalized * hit.distance);
            vec = hit.point;
        }
        else
        {
            UnityEngine.Debug.DrawRay(originTransform.position, (spellAimTransform.position - originTransform.position).normalized * 1000f);
            vec = spellAimTransform.position;
        }

        // Handle Jump Input *********************************************************************************

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bhop = true;
            TransitionState();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            bhop = false;
            TransitionState();
        }

        // Handle Crouch Input *********************************************************************************

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            crouched = true;
            TransitionState();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            crouched = false;
            if (bhop)
            {
                //UnityEngine.Debug.Log("Switch to AERIAL!");
                playerState = State.AERIAL;
            }
            else if(focus)
            {
                //UnityEngine.Debug.Log("Switch to FOCUS!");
                playerState = State.FOCUS;
            }
            else
            {
                //UnityEngine.Debug.Log("Switch to NORMAL!");
                playerState = State.NORMAL;
            }
        }

        // HANDLE THE DASH INPUT *********************************************************************************

        if (!dashReset && Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashReset = true;
            dashEnd = true;
            TransitionState();
        }

        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (bhop)
            {
                //UnityEngine.Debug.Log("Switch to AERIAL!");
                playerState = State.AERIAL;
            }
            else if (focus)
            {
                //UnityEngine.Debug.Log("Switch to FOCUS!");
                playerState = State.FOCUS;
            }
            else if (crouched)
            {
                //UnityEngine.Debug.Log("Switch to GROUND!");
                playerState = State.GROUND;
            }
            else
            {
                //UnityEngine.Debug.Log("Switch to NORMAL!");
                playerState = State.NORMAL;
            }
        }

        if (dashReset)
        {
            dashCooldown -= Time.deltaTime;
        }
        if (dashCooldown <= 0.15f && dashCooldown > 0.0f && dashEnd && dashReset)
        {
            if (bhop)
            {
                //UnityEngine.Debug.Log("Switch to AERIAL!");
                playerState = State.AERIAL;
            }
            else if (focus)
            {
                //UnityEngine.Debug.Log("Switch to FOCUS!");
                playerState = State.FOCUS;
            }
            else if (crouched)
            {
                //UnityEngine.Debug.Log("Switch to GROUND!");
                playerState = State.GROUND;
            }
            else
            {
                //UnityEngine.Debug.Log("Switch to NORMAL!");
                playerState = State.NORMAL;
            }

            if (grounded)
            {
                rb.drag = groundDrag;
            }
            dashEnd = false;
        }
        else if (dashCooldown <= 0.0f && dashReset)
        {
            dashReset = false;
            dashCooldown = 0.4f;
        }

        // HANDLE THE FOCUS INPUT **********************************************************************************

        if (Input.GetMouseButtonDown(1))
        {
            focus = true;
            TransitionState();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            focus = false;
            if (bhop)
            {
                //UnityEngine.Debug.Log("Switch to AERIAL!");
                playerState = State.AERIAL;
            }
            else if (crouched)
            {
                //UnityEngine.Debug.Log("Switch to GROUND!");
                playerState = State.GROUND;
            }
            else
            {
                //UnityEngine.Debug.Log("Switch to NORMAL!");
                playerState = State.NORMAL;
            }
        }

    }

    private void FixedUpdate()
    {
        if (!dashEnd && !slam)
        {
            moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
            moveDirection.y = 0.0f;
            if(playerState != State.FOCUS){
                if (grounded && (playerState == State.NORMAL || (playerState == State.GROUND && momentum >= 1.0f)))
                {
                    rb.AddForce(moveDirection.normalized * speed * 10.0f, ForceMode.Force);
                }
                else if (grounded && crouched && playerState == State.GROUND)
                {
                    rb.AddForce(moveDirection.normalized * speed * 10.0f * aircontrol * 2.5f * momentum, ForceMode.Force);
                }
                else if (!grounded && (playerState != State.AERIAL || playerState != State.NORMAL))
                {
                    rb.AddForce(moveDirection.normalized * speed * 10.0f * aircontrol * momentum, ForceMode.Force);
                }
            }
            else
            {
                rb.AddForce(moveDirection.normalized * speed * 5.0f, ForceMode.Force);
            }

            if (!grounded && momentum <= maxMom && (moveDirection.x != 0 || moveDirection.z != 0))
            {
                momentum += momentumClimb;
            }
            else if (grounded && crouched && momentum <= maxMom)
            {
                momentum -= momentumClimb/2f;
                if (momentum <= 1.0f)
                {
                    //rb.velocity = Vector3.zero;
                    momentum = 1.0f;
                }
            }
            else if (focus && momentum <= maxMom)
            {
                momentum += momentumClimb * 1.5f;
            }
        }
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSpeed);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        Vector3 newVel;
        if (!dashEnd)
        {
            if (focus)
            {
                if (flatVel.magnitude > speed / 2f)
                {
                    newVel = flatVel.normalized * speed / 2f;
                    rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                }
            }
            else {
                if (grounded)
                {
                    if (flatVel.magnitude > speed && playerState == State.NORMAL)
                    {
                        newVel = flatVel.normalized * speed;
                        rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                    }
                    else if (flatVel.magnitude > speed + (10f * (momentum - 1) / (maxMom - 1)) && (playerState == State.GROUND || playerState == State.AERIAL))
                    {
                        newVel = flatVel.normalized * (speed + (10f * (momentum - 1) / (maxMom - 1)));
                        rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                    }
                }
                else if (!grounded)
                {
                    if (flatVel.magnitude > speed + (10f * (momentum - 1) / (maxMom - 1)))
                    {
                        newVel = flatVel.normalized * (speed + (10f * (momentum - 1) / (maxMom - 1)));
                        rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                    }

                }
            }
        }
        else
        {
            if (flatVel.magnitude > 40.0f)
            {
                newVel = flatVel.normalized * 40.0f;
                rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Ground")
        {
            slam = false;
            grounded = true;
            //UnityEngine.Debug.Log(bhop);
            if (bhop)
            {
                //UnityEngine.Debug.Log("JUMP NOW!");
                Jump();
            }

            if (playerState == State.NORMAL)
            {
                rb.drag = groundDrag;
                momentum = 1.0f;
            }
            else if (playerState == State.GROUND)
            {
                rb.drag = 1f;
            }
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.tag == "Ground")
        {
            slam = false;
            grounded = true;
            //UnityEngine.Debug.Log(bhop);
            if (bhop)
            {
                //UnityEngine.Debug.Log("JUMP RIGHT NOW!");
                Jump();
            }

            if (playerState == State.NORMAL)
            {
                rb.drag = groundDrag;
                momentum = 1.0f;
            }
            else if (playerState == State.GROUND)
            {
                rb.drag = 1f;
            }
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.tag == "Ground")
        {
            rb.drag = 0.0f;
            grounded = false;
        }
    }

    private void Jump()
    {
        //UnityEngine.Debug.Log("JUMP!");
        if (!crouched && grounded && bhop)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void Slam()
    {
        slam = true;
        momentum -= 0.5f;
        if (momentum <= 1.0f)
        {
            momentum = 1.0f;
        }
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * -jumpForce * 3.0f, ForceMode.Impulse);
    }

    private void Slide()
    {
        //rb.drag = 0.1f;
    }

    private void Dash()
    {
        moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
        moveDirection *= 45.0f;
        moveDirection.y = rb.velocity.y;
        //momentum -= 1.25f;
        //if (momentum <= 1.0f)
        //{
            //momentum = 1.0f;
        //}

        rb.drag = 0.0f;
        rb.velocity = moveDirection;
    }

    private void TransitionState()
    {
        if (bhop || crouched || (dashEnd && dashReset) || focus)
        {
            if (bhop && playerState != State.AERIAL)
            {
                //UnityEngine.Debug.Log("Switch to AERIAL!");
                playerState = State.AERIAL;
                Jump();
            }
            if (crouched)
            {
                //UnityEngine.Debug.Log("Switch to GROUND!");
                playerState = State.GROUND;
                if (!grounded)
                {
                    Slam();
                }
                else
                {
                    Slide();
                }
            }
            if (dashEnd && dashReset)
            {
                moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
                if (moveDirection.x != 0 || moveDirection.z != 0)
                {
                    //UnityEngine.Debug.Log("Switch to DASH!");
                    playerState = State.DASH;
                    Dash();
                }
            }
            if (focus)
            {
                //UnityEngine.Debug.Log("Switch to FOCUS!");
                playerState = State.FOCUS;
            }
        }
        else
        {
            //UnityEngine.Debug.Log("Switch to NORMAL!");
            playerState = State.NORMAL;
        }
    }

}
