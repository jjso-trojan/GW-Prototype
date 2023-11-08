using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.Experimental.AI;
//using Unity.Collections.Allocator;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    private float timer;
    //private float tim;
    public bool hit = false;
    public float hitTimer = 1.0f;

    [SerializeField]
    private bool swap = false;

    [SerializeField]
    private float yOffset;
    public float airControl = 1f;
    private Rigidbody rb;
    public float gravityScale = 1.0f;
    public int hits = 0;
    [SerializeField]
    private bool ground = true;
    //public float timeFrame = 0f;


    //private NavMeshQuery nq;
    //private Experimental.AI.NavMeshWorld world;
    //private Unity.Collections.Allocator alloc;


    [SerializeField]
    public NavMeshAgent navMeshAgent;
    [SerializeField]
    private Transform target;
    //public float prev = 0f;

    //private float globalGravity = -9.81f;
    //private float globalGravity = -6.5f;

    //private NavMeshPath path;
    //private int index;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        //nq = NavMeshQuery(world, alloc, 10);
        //path = new NavMeshPath();
        rb = GetComponent<Rigidbody>();
        //timer = 0f;
        //index = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (navMeshAgent.enabled && navMeshAgent.isOnOffMeshLink)
        {
            //OffMeshLinkData off = navMeshAgent.currentOffMeshLinkData;
            //UnityEngine.Debug.Log(navMeshAgent.isOnOffMeshLink);
            //navMeshAgent.Warp(transform.position);
            //navMeshAgent.ActivateCurrentOffMeshLink(false);
            navMeshAgent.enabled = false;
            //UnityEngine.Debug.Log(navMeshAgent.isOnOffMeshLink);
            rb.isKinematic = false;
            transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            //off.activated = true;
            rb.velocity = ((Vector3.forward * 20f) + (Vector3.up * 20.0f));
        }
        /*
        timer += Time.deltaTime;
        if (timer > 0.5f)
        {
            timer = 0f;
            if (Vector3.Distance(transform.position, target.position) > 10.0f)
            {
                index = 1;
                NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
            }
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
            UnityEngine.Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        */

        if (ground)
        {
            if (!hit)
            {
                //prev = 0f;
                navMeshAgent.enabled = true;
                rb.isKinematic = true;
            }
        }
        
        /*
        if (ground)
        {
            airControl = 1f;
        }
        else
        {
            airControl = 0.2f;
        }
        */

        if (hit)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0f)
            {
                hits = 0;
                hitTimer = 1f;
                hit = false;
                //airControl = 1f;
                //prev = rb.velocity.y;
                //navMeshAgent.baseOffset = f;
                if (ground)
                {
                    navMeshAgent.enabled = true;
                    rb.isKinematic = true;
                }
                //navMeshAgent.enabled = true;
                //rb.isKinematic = true;
            }
        }
        /*
        if (swap)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = tim;
                swap = false;
            }
        }
        else if (!swap)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = tim;
                swap = true;
            }
        }
        */
    }

    void FixedUpdate()
    {
        if (navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(target.position);
        }
        /*
        if (path.corners.Length != 0 && index < path.corners.Length)
        {
            Vector3 trans = path.corners[index];
            transform.position = Vector3.MoveTowards(transform.position, trans, 10f * Time.deltaTime);
            if (Vector3.Distance(transform.position, trans) <= 10.0f)
            {
                index++;
            }
        }
        */
        //Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        //rb.AddForce(gravity, ForceMode.Acceleration);

        /*
        if (swap && !hit)
        {
            rb.AddForce(-Vector3.right * 50f * airControl, ForceMode.Force);
        }
        else if (!swap && !hit)
        {
            rb.AddForce(Vector3.right * 50f * airControl, ForceMode.Force);
        }
        */

        /*
        float tim = Time.time - timeFrame;
        navMeshAgent.baseOffset += (prev * (tim)) + (globalGravity * (tim) * (tim))/2;
            //navMeshAgent.baseOffset += (globalGravity * Time.deltaTime);
        prev += (globalGravity * (tim));

        if (navMeshAgent.baseOffset <= 0.85f)
        {
            navMeshAgent.baseOffset = 0.85f;
        }
        */
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            ground = true;
            //hits = 0;
            //gravityScale = 1f;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            ground = true;
            //hits = 0;
            //gravityScale = 1f;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        //rb.drag = 1f;
        if (other.gameObject.tag == "Ground")
        {
            ground = false;
        }
    }
}
