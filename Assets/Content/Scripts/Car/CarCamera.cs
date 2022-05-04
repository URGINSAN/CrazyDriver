using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public CameraFollow cam;
    public float distance = 6;
    public float height = 1;
    public Transform pitch;
    public Rigidbody rigid;

    void Start()
    {
        Application.targetFrameRate = 25;
    }
    public void SetCamera()
    {
        cam = Camera.main.GetComponent<CameraFollow>();
        cam.distance = distance;
        cam.height = height;
        cam.target = pitch;
        cam.parentRigidbody = rigid;
    }

    public void PitchDirection(bool forward)
    {
        if (forward)
        {
            pitch.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            pitch.transform.localEulerAngles = new Vector3(0, 180, 0);
        }
    }
}