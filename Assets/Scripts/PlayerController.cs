using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine.InputSystem;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine;
using System.IO;

//namespace KinematicCharacterController.Walkthrough.AddingImpulses
//{
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

        private float dashCooldown = 0.5f;
        //private bool dashing = false;
        private bool dashReset;
        private bool dashEnd;
        private bool crouched = false;
        private bool slam = false;
        private bool focus = false;

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
            else if (playerState == State.AERIAL)
            {
                spell = GameObject.Instantiate(airSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
            }
            else if (playerState == State.GROUND)
            {
                if (!grounded)
                {
                    if (bhop)
                    {
                        spell = GameObject.Instantiate(airSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
                    }
                    else if (dashReset)
                    {
                        spell = GameObject.Instantiate(dashSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
                    }
                    else if (focus)
                    {
                        spell = spell = GameObject.Instantiate(focusSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
                    }
                    else
                    {
                        spell = GameObject.Instantiate(normalSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
                    }
                }
                else
                {
                    spell = GameObject.Instantiate(groundSpellPrefab, originTransform.position, Quaternion.identity, spellParent);
                }
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
            if (playerState == State.FOCUS)
            {
                //RaycastHit hit;
                Vector3 vec;
                //if (Physics.Raycast(centerTransform.position, (aim.position - centerTransform.position).normalized, out hit, sp.range))
                //{
                //vec = hit.point;
                //}
                //else
                //{
                vec = ((centerTransform.forward * sp.range) + centerTransform.position);
                //}

                sp.aimDirect = (vec - originTransform.position).normalized;
                //sp.aimDirect = (centerTransform.forward + sp.aim);
            }
            else
            {
                sp.aimDirect = ((spellAimTransform.position - originTransform.position).normalized + sp.aim);
            }
            //UnityEngine.Debug.Log(sp.end);
            //vec = spellAimTransform.position;

            Rigidbody srb = spell.GetComponent<Rigidbody>();
            //srb.velocity = (vec - originTransform.position).normalized * sp.travelTime;
            srb.velocity = sp.aimDirect * sp.travelTime;
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

            if (Input.GetMouseButtonDown(1))
            {
                crouched = true;
                TransitionState();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                crouched = false;
                if (bhop)
                {
                    //UnityEngine.Debug.Log("Switch to AERIAL!");
                    playerState = State.AERIAL;
                    Jump();
                }
                else if (focus)
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
                //dashing = true;
                dashReset = true;
                dashEnd = true;
                TransitionState();
            }

            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                //dashing = false;
                if (bhop)
                {
                    //UnityEngine.Debug.Log("Switch to AERIAL!");
                    playerState = State.AERIAL;
                    //Jump();
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
            if (dashCooldown <= 0.25f && dashCooldown > 0.0f && dashEnd && dashReset)
            {
                if (bhop)
                {
                    //UnityEngine.Debug.Log("Switch to AERIAL!");
                    playerState = State.AERIAL;
                    //Jump();
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
                dashCooldown = 0.5f;
                /*
                if (dashing)
                {
                    dashReset = true;
                    dashEnd = true;
                    TransitionState();
                }
                */
            }

            // HANDLE THE FOCUS INPUT **********************************************************************************

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                focus = true;
                //rb.velocity = Vector3.zero;
                if (dashEnd && dashCooldown >= 0.25f)
                {
                    dashEnd = false;
                    dashCooldown = 0.25f;
                }
                TransitionState();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
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
            if (!dashEnd)
            {
                moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
                moveDirection.y = 0.0f;
                if (playerState != State.FOCUS)
                {
                    if (grounded && (playerState == State.NORMAL))
                    {
                        rb.AddForce(moveDirection.normalized * speed * 10.0f * momentum, ForceMode.Force);
                    }
                    else if (grounded && playerState == State.GROUND)
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
            }

            if (!focus)
            {
                if (!grounded && (moveDirection.x != 0 || moveDirection.z != 0) && momentum <= maxMom)
                {
                    momentum += momentumClimb;
                }
                else if (grounded)
                {
                    if (crouched)
                    {
                        momentum += momentumClimb;
                    }
                    else
                    {
                        momentum -= momentumClimb;
                    }
                }

                if (momentum <= 1f)
                {
                    momentum = 1f;
                }
                else if (momentum >= maxMom && !(grounded && playerState == State.GROUND))
                {
                    momentum = maxMom;
                }
                else if (momentum >= 2.75f && grounded && playerState == State.GROUND)
                {
                    momentum = 2.75f;
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
                if (focus && flatVel.magnitude > speed / 2f)
                {
                    newVel = flatVel.normalized * speed / 2f;
                    rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                }
                else
                {
                    if (!(grounded && playerState == State.GROUND) && flatVel.magnitude > speed + (15f * (momentum - 1) / (maxMom - 1)))
                    {
                        newVel = flatVel.normalized * (speed + (15f * (momentum - 1) / (maxMom - 1)));
                        rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                    }
                    else if ((grounded && playerState == State.GROUND) && flatVel.magnitude > speed + (15f * (momentum - 1) / (maxMom - 1)))
                    {
                        newVel = flatVel.normalized * (speed + (15f * (momentum - 1) / (maxMom - 1)));
                        rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
                    }
                }
            }
            else
            {
                if (flatVel.magnitude > 45.0f)
                {
                    newVel = flatVel.normalized * 45.0f;
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
                    Jump();
                }

                if (playerState == State.NORMAL && !dashEnd)
                {
                    rb.drag = groundDrag;
                }
                else if (playerState == State.GROUND && !dashEnd)
                {
                    rb.drag = 0f;
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

                if (playerState == State.NORMAL && !dashEnd)
                {
                    rb.drag = groundDrag;
                    //rb.drag = 0f;
                    //momentum = 1.0f;
                }
                else if (playerState == State.GROUND && !dashEnd)
                {
                    rb.drag = 0f;
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
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * -jumpForce * 3f, ForceMode.Impulse);
        }

        private void Dash()
        {
            moveDirection = cameraTransform.right.normalized * horizontal + cameraTransform.forward.normalized * vertical;
            moveDirection *= 45.0f;
            moveDirection.y = rb.velocity.y;

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
//}
