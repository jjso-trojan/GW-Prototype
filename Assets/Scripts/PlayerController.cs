using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public Transform orientation;
    private float horizontal;
    private float vertical;
    [SerializeField]
    private bool grounded = false;
    [SerializeField]
    private bool bhop = false;
    [SerializeField]
    public float groundDrag;
    private Vector3 moveDirection;
    private Transform cameraTransform;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float momentum = 1.0f;
    [SerializeField]
    private float maxMom = 2.0f;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float aircontrol;
    [SerializeField]
    private float momentumClimb;

    public float rotSpeed = 100f;

    public float dashCooldown = 0.75f;
    [SerializeField]
    public bool dashReset;
    [SerializeField]
    public bool dashEnd;
    [SerializeField]
    private bool crouched = false;
    [SerializeField]
    private bool slam = false;

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
    void Start()
    {
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        SpeedControl();

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
            TransitionState();
        }

        // HANDLE THE DASH INPUT *********************************************************************************

        if (!dashReset && Input.GetKeyDown(KeyCode.LeftShift))
        {
            dashReset = true;
            dashEnd = true;
            TransitionState();
        }
        // Causing normal states when spammed.
        else if (Input.GetKeyUp(KeyCode.LeftShift) && !dashEnd)
        {
            if (bhop)
            {
                playerState = State.AERIAL;
            }
            else
            {
                playerState = State.NORMAL;
            }
        }

        if (dashReset)
        {
            dashCooldown -= Time.deltaTime;
        }
        if (dashCooldown <= 0.5f && dashCooldown > 0.0f && dashEnd && dashReset)
        {
            TransitionState();

            if (grounded)
            {
                rb.drag = groundDrag;
            }
            dashEnd = false;
            //rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
        }
        else if (dashCooldown <= 0.0f && dashReset)
        {
            dashReset = false;
            dashCooldown = 0.75f;
        }

    }

    private void FixedUpdate()
    {
        if (!dashEnd && !slam)
        {
            moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
            moveDirection.y = 0.0f;

            if (grounded && playerState == State.NORMAL)
            {
                rb.AddForce(moveDirection.normalized * speed * 10.0f, ForceMode.Force);
            }
            else if (grounded && crouched && playerState == State.GROUND)
            {
                rb.AddForce(moveDirection.normalized * speed * 15.0f * aircontrol * momentum, ForceMode.Force);
            }
            else if (!grounded && (playerState != State.AERIAL || playerState != State.NORMAL))
            {
                rb.AddForce(moveDirection.normalized * speed * 10.0f * aircontrol * momentum, ForceMode.Force);
            }

            if (!grounded && !dashReset && momentum <= maxMom && (moveDirection.x != 0 || moveDirection.z != 0))
            {
                momentum += momentumClimb;
            }
            else if (grounded && crouched && momentum <= maxMom)
            {
                momentum -= momentumClimb;
                if (momentum <= 1.0f)
                {
                    momentum = 1.0f;
                }
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
            if (grounded)
            {
                if (flatVel.magnitude > speed && playerState == State.NORMAL)
                {
                    newVel = flatVel.normalized * speed;
                    rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                }
                else if (flatVel.magnitude > speed + (12.5f * (momentum - 1)/(maxMom - 1)) && (playerState == State.GROUND || playerState == State.AERIAL))
                {
                    newVel = flatVel.normalized * (speed + (12.5f * (momentum - 1)/(maxMom - 1)));
                    rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                }
            }
            else if(!grounded)
            {
                if (flatVel.magnitude > (speed + (12.5f * (momentum - 1) / (maxMom - 1))))
                {
                    newVel = flatVel.normalized * (speed + (12.5f * (momentum - 1) / (maxMom - 1)));
                    rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
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
            if (bhop)
            {
                Jump();
            }

            if (playerState == State.NORMAL)
            {
                rb.drag = groundDrag;
                momentum = 1.0f;
            }
            else if (playerState == State.GROUND)
            {
                rb.drag = 0.5f;
            }
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.tag == "Ground")
        {
            slam = false;
            grounded = true;
            if (bhop)
            {
                Jump();
            }

            if (playerState == State.NORMAL)
            {
                rb.drag = groundDrag;
                momentum = 1.0f;
            }
            else if (playerState == State.GROUND)
            {
                rb.drag = 0.5f;
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
        if (momentum >= maxMom)
        {
            momentum = maxMom;
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
        moveDirection *= 40.0f;
        moveDirection.y = rb.velocity.y;
        momentum -= 1.25f;
        if (momentum <= 1.0f)
        {
            momentum = 1.0f;
        }

        rb.drag = 0.0f;
        rb.velocity = moveDirection;
    }

    private void TransitionState()
    {
        if (bhop && (playerState == State.GROUND || playerState == State.NORMAL || playerState == State.DASH))
        {
            UnityEngine.Debug.Log("Switch to AERIAL!");
            playerState = State.AERIAL;
            //Jump();
        }
        else if (crouched && (playerState == State.NORMAL || playerState == State.AERIAL || playerState == State.DASH))
        {
            UnityEngine.Debug.Log("Switch to GROUND!");
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
        else if (dashEnd && dashReset && (playerState == State.NORMAL || playerState == State.AERIAL || playerState == State.GROUND))
        {
            moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
            if (moveDirection.x != 0 || moveDirection.z != 0)
            {
                UnityEngine.Debug.Log("Switch to DASH!");
                playerState = State.DASH;
                Dash();
            }
        }
        else
        {
            UnityEngine.Debug.Log("Switch to NORMAL!");
            playerState = State.NORMAL;
        }
    }

}
