using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeNotifier : MonoBehaviour
{
    [SerializeField]
    private Transform aim;
    [SerializeField]
    private Transform barrel;
    [SerializeField]
    private GameObject spellPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Vector3 path = (aim.position - barrel.position).normalized;
        Ray r = new Ray(barrel.position, path * 20f);
        if (Physics.Raycast(r, out hit, 20f))
        {
            //UnityEngine.Debug.DrawRay(barrel.position, (hit.point - barrel.position).normalized * hit.distance);
        }
        else
        {
            //UnityEngine.Debug.DrawRay(barrel.position, path * 20f);
        }
    }
}
