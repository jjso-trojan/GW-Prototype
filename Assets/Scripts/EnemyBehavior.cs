using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    private float timer;
    private float tim;
    public bool hit = false;
    public float hitTimer = 1.0f;

    [SerializeField]
    private bool swap = false;
    public float airControl = 1f;
    private Rigidbody rb;
    public float gravityScale = 1.0f;
    public int hits = 0;
    private bool ground = false;

    // Global Gravity doesn't appear in the inspector. Modify it here in the code
    // (or via scripting) to define a different default gravity for all objects.

    //private float globalGravity = -9.81f;
    private float globalGravity = -6.5f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tim = timer;
    }

    // Update is called once per frame
    void Update()
    {
        if (ground)
        {
            airControl = 0f;
        }
        else
        {
            airControl = 0f;
        }

        if (hit)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0f)
            {
                hitTimer = 1f;
                hit = false;
            }
        }
        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);
        if (swap && !hit)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = tim;
                swap = false;
            }
        }
        else if (!swap && !hit)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = tim;
                swap = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (swap)
        {
            rb.AddForce(-Vector3.right * 50f * airControl, ForceMode.Force);
        }
        else
        {
            rb.AddForce(Vector3.right * 50f * airControl, ForceMode.Force);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        rb.drag = 0f;
        ground = true;
        hits = 0;
        gravityScale = 1f;
        airControl = 0f;
    }

    private void OnCollisionExit(Collision other)
    {
        rb.drag = 0f;
        ground = false;
        //airControl = 0f;
    }
}
