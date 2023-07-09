using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour, ISpell
{
    [SerializeField]
    private float ran;
    [SerializeField]
    private float hit;
    [SerializeField]
    private float tt;
    [SerializeField]
    private float cr;
    [SerializeField]
    private int dam;
    [SerializeField]
    private Vector3 knock;
    [SerializeField]
    public Vector3 aim;
    [SerializeField]
    private float k;
    [SerializeField]
    private float status;
    [SerializeField]
    private int mana;
    public Vector3 dir;

    public float range { get; set; }
    public float hitbox { get; set; }
    public float travelTime { get; set; }
    public float castRate { get; set; }
    public int damage { get; set; }
    public Vector3 knockBack { get; set; }
    public Vector3 aimDirect { get; set; }
    public float statusBuild { get; set; }
    public int manaUse { get; set; }
    private Vector3 start;

    //ISpell(float range,float hitbox,float travelTime,float castRate, int damage, Vector3 knockback,float statusBuild,int manaUse);

    // Start is called before the first frame update
    void OnEnable()
    {
        range = ran;
        hitbox = hit;
        travelTime = tt;
        castRate = cr;
        damage = dam;
        knockBack = knock;
        //aimDirect = (dir + aim);
        statusBuild = status;
        manaUse = mana;

        start = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, start) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player" && other.gameObject.tag != "Spell")
        {
            if (other.gameObject.tag == "Enemy")
            {
                int hits = other.gameObject.GetComponent<EnemyBehavior>().hits;
                if (hits > 0)
                {
                    //other.gameObject.GetComponent<EnemyBehavior>().gravityScale += 0.25f;
                }
                other.gameObject.GetComponent<EnemyBehavior>().hit = true;
                other.gameObject.GetComponent<EnemyBehavior>().hitTimer = 1.0f;
                other.gameObject.GetComponent<EnemyBehavior>().airControl = 0f;
                other.gameObject.GetComponent<Rigidbody>().velocity = ((dir * knockBack.z) + (Vector3.up * knockBack.y)).normalized * k;
            }
            UnityEngine.Debug.Log("HELOOOOOOOO");
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "Player" && other.gameObject.tag != "Spell")
        {
            if (other.gameObject.tag == "Enemy")
            {
                int hits = other.gameObject.GetComponent<EnemyBehavior>().hits;
                if (hits > 0)
                {
                    //other.gameObject.GetComponent<EnemyBehavior>().gravityScale += 0.25f;
                }
                other.gameObject.GetComponent<EnemyBehavior>().hit = true;
                other.gameObject.GetComponent<EnemyBehavior>().hitTimer = 1.0f;
                other.gameObject.GetComponent<EnemyBehavior>().airControl = 0f;
                other.gameObject.GetComponent<Rigidbody>().velocity = ((dir * knockBack.z) + (Vector3.up * knockBack.y)).normalized * k;
            }
            UnityEngine.Debug.Log("HELOOOOOOOO");
            Destroy(gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player" && other.gameObject.tag != "Spell")
        {
            if (other.gameObject.tag == "Enemy")
            {
                int hits = other.gameObject.GetComponent<EnemyBehavior>().hits;
                if (hits > 0)
                {
                    //other.gameObject.GetComponent<EnemyBehavior>().gravityScale += 0.25f;
                }
                other.gameObject.GetComponent<EnemyBehavior>().hit = true;
                other.gameObject.GetComponent<EnemyBehavior>().hitTimer = 1.0f;
                other.gameObject.GetComponent<EnemyBehavior>().airControl = 0f;
                other.gameObject.GetComponent<Rigidbody>().velocity = ((dir * knockBack.z) + (Vector3.up * knockBack.y)).normalized * k;
            }
            UnityEngine.Debug.Log("HELOOOOOOOO");
            Destroy(gameObject);
        }
    }
}
