using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MoveCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    public float rotationSpeed;
    public Transform combatAim;
    float xRot = 0.0f;
    float yRot = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        CameraRotation();
    } 

    void CameraRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 300;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 300;

        yRot += mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        

        //transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        //player.rotation = Quaternion.Euler(0, yRot, 0);
        //combatAim.rotation = Quaternion.Euler(xRot, yRot, 0);
    }
}
