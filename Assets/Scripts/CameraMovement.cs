using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float m_MouseSens = 100f;
    float xRotation = 0f;

    private string m_MouseXName;
    private string m_MouseYName;
    private float m_MouseXVal;
    private float m_MouseYVal;

    //private float rotateSpeed = 4f;
    //private float maxTurn = 3f;

    public Transform playerBody;

    // Start is called before the first frame update
    private void Start()
    {
        //get the mouse x input name
        m_MouseXName = "Mouse X";
        //get the mouse y input name
        m_MouseYName = "Mouse Y";
        //lock cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //get both mouse x and y values
        m_MouseXVal = Input.GetAxis(m_MouseXName);
        m_MouseYVal = Input.GetAxis(m_MouseYName);
        // Move the camera.
        // Adjust the rigidbodies position and orientation in Update. 
        CameraMove();
    }

    private void CameraMove()
    {
        //mouse x movement 
        float mouseXMov = m_MouseXVal * m_MouseSens * Time.deltaTime;
        //mouse y movement
        float mouseYMov = m_MouseYVal * m_MouseSens * Time.deltaTime;

        //set rotation decrease criteria and limit the rotation so you can't look behind
        xRotation -= mouseYMov;
        xRotation = Mathf.Clamp(xRotation, -60f, 50f);

        //Quaternion of x rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //rotate the body
        playerBody.Rotate(Vector3.up * mouseXMov);
        //playerBody.Rotate(Vector3.right * mouseYMov); //--> makes it rotate everywhere(because camera is attached?)
        //m_Rigidbody.MoveRotation(m_Rigidbody.rotation * playerRotation);
    }
}
