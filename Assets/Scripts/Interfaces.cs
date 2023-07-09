using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interfaces : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public interface ISpell
{
    float range { get; set; }
    float hitbox { get; set; }
    float travelTime { get; set; }
    float castRate { get; set; }
    int damage { get; set; }
    Vector3 knockBack { get; set; }
    Vector3 aimDirect { get; set; }
    float statusBuild { get; set; }
    int manaUse { get; set; }
}
