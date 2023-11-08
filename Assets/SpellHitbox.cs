using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine;

//namespace KinematicCharacterController.Walkthrough.AddingImpulses
//{
    public class SpellHitbox : MonoBehaviour, ISpell
    {
        [SerializeField]
        private GameObject parent;
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
        public Vector3 knock;
        [SerializeField]
        public Vector3 aim;
        [SerializeField]
        public float k;
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
                    other.gameObject.GetComponent<EnemyBehavior>().airControl = 0.2f;
                    //other.gameObject.GetComponent<EnemyBehavior>().navMeshAgent.ActivateCurrentOffMeshLink(false);
                    /*if (other.gameObject.GetComponent<EnemyBehavior>().navMeshAgent.isOnOffMeshLink)
                    {
                        other.gameObject.GetComponent<EnemyBehavior>().navMeshAgent.Warp(other.gameObject.transform.position);
                        UnityEngine.Debug.Log(other.gameObject.GetComponent<EnemyBehavior>().navMeshAgent.isOnOffMeshLink);
                    }
                    */
                    //other.gameObject.GetComponent<EnemyBehavior>().prev = knockBack.y;
                    //other.gameObject.GetComponent<EnemyBehavior>().timeFrame = Time.time;
                    //other.gameObject.GetComponent<EnemyBehavior>().navMeshAgent.baseOffset = 10f;
                    other.gameObject.GetComponent<EnemyBehavior>().navMeshAgent.enabled = false;
                    other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                    other.gameObject.GetComponent<Rigidbody>().velocity = ((dir * knockBack.z) + (Vector3.up * knockBack.y)).normalized * k;
                    //other.gameObject.GetComponent<MyCharacterController>().UpdateVelocity(((dir * knockBack.z) + (Vector3.up * knockBack.y)).normalized * k);
                    //other.gameObject.GetComponent<MyCharacterController>().Motor.ForceUnground(0.1f);
                    //other.gameObject.GetComponent<MyCharacterController>().AddVelocity(((dir * knockBack.z) + (Vector3.up * knockBack.y)).normalized * k);
                    Destroy(gameObject);
                }
                UnityEngine.Debug.Log("HELOOOOOOOO");
                Destroy(parent);
            }
        }
    }
//}
