using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private float camSensitivity = 5;

    private Rigidbody _rb;
    private Camera _cam;
    private Transform _transform;
    private float yaw;
    private float pitch;

    // Use this for initialization
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = GetComponentInChildren<Camera>();
        _transform = GetComponent<Transform>();
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        float MovementX = Input.GetAxis("Horizontal");
        float MovementY = Input.GetAxis("Vertical");

        _rb.velocity = (transform.forward * MovementY + transform.right * MovementX) * speed * Time.deltaTime;

        yaw += camSensitivity * Time.deltaTime * Input.GetAxis("Mouse X");


        pitch -= camSensitivity * Time.deltaTime * Input.GetAxis("Mouse Y");
        if (pitch > 60)
            pitch = 60;
        else if (pitch < -80)
            pitch = -80;

        transform.eulerAngles = new Vector3(0, yaw, 0);
        _cam.gameObject.transform.eulerAngles = new Vector3(pitch, _cam.gameObject.transform.eulerAngles.y, 0);


    }


    private void FixedUpdate()
    {

        ClientSend.SendTransform(_transform.localPosition, _transform.localRotation);
    }

    private void OnGUI()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


}
