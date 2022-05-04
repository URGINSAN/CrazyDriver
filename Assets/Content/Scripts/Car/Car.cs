using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    private CarCamera carCamera;
    [Header("Acceleration")]
    public float acceleration;
    public float[] gears;
    public int currentGear;
    private float hV;
    private float vV;
    public float topSpeed = 180;
    private float topReverseSpeed = 40;
    private float currentMaxSpeed;
    [Header("RPM")]
    public float RPM;
    public float rpmDecriseFactor = 100;
    public float maxRPM = 5000;
    [Header("Braking")]
    public float brakeFactor = 50;
    [Header("Steer")]
    public float steerAccuracy = 1;
    public float maxSteer = 25;
    [Header("Wheels")]
    public WheelCollider[] wheelColliders;
    public Transform[] wheels;
    public Skid[] skids;
    [Header("Other")]
    private bool gearUp;
    public float speed;
    public float skidWhen = 1;
    private Rigidbody rigid;
    public float skidFactor;
    private bool reverse;
    public GameObject collGO;
    public Vector3 com;
    private CarHealth health;
    [Header("Sounds")]
    public AudioSource engineSound;
    public AudioSource skidSound;
    public AnimationCurve[] pitchFromRPM;
    public AudioSource sfx;
    public AudioClip lowColl;
    public AudioClip highColl;

    public float driftValue, driftValue2;

    public enum CarType
    {
        front = 0,
        back = 1,
        full = 2
    }
    public CarType carType;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        health = GetComponent<CarHealth>();
        CarEnter();
    }

    public void CarEnter()
    {
        carCamera = GetComponent<CarCamera>();
        carCamera.SetCamera();
    }

    private void Update()
    {
        hV = Input.GetAxis("Horizontal");
        vV = Input.GetAxis("Vertical");
        float bV = 1;
        
        if (Mathf.Abs(rigid.angularVelocity.y) > skidWhen)
        {
            skidFactor = 1;

            for (int i = 0; i < skids.Length; i++)
            {
                if (wheelColliders[i].isGrounded)
                    skids[i].SetState(true);
            }
        }
        else
        {
            skidFactor = 0;

            for (int i = 0; i < skids.Length; i++)
            {
                skids[i].SetState(false);
            }
        }

        speed = rigid.velocity.magnitude * 4;


        if (transform.InverseTransformDirection(rigid.velocity).z > 0)
        {
            if (speed > 1)
                carCamera.PitchDirection(true);

            currentMaxSpeed = topSpeed;
            reverse = false;
        }
        else
        {
            if (speed > 1)
                carCamera.PitchDirection(false);

            currentMaxSpeed = topReverseSpeed;
            reverse = true;
        }
        
        if (reverse)
        {
            if (vV > 0)
            {
                for (int i = 0; i < wheelColliders.Length; i++)
                {
                    wheelColliders[i].brakeTorque = 200;
                }
            }
            else
            {
                for (int i = 0; i < wheelColliders.Length; i++)
                {
                    wheelColliders[i].brakeTorque = 0;
                }
            }
        }
        else
        {
            if (vV < 0)
            {
                for (int i = 0; i < wheelColliders.Length; i++)
                {
                    wheelColliders[i].brakeTorque = 200;
                }
            }
            else
            {
                for (int i = 0; i < wheelColliders.Length; i++)
                {
                    wheelColliders[i].brakeTorque = 0;
                }
            }
        }

        for (int i = 0; i < 2; i++)
        {
            wheelColliders[i].steerAngle = Mathf.Lerp(wheelColliders[i].steerAngle, maxSteer * hV, Time.deltaTime * steerAccuracy);
        }
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            UpdateWheelPose(wheelColliders[i], wheels[i]);
        }

        if (!Input.GetKey(KeyCode.Space))
        {
            for (int i = 2; i < 4; i++)
            {
                WheelFrictionCurve myWfc = wheelColliders[i].sidewaysFriction;
                myWfc.extremumSlip = 0.6f;
                wheelColliders[i].sidewaysFriction = myWfc;
                wheelColliders[i].brakeTorque = 0;
            }
        }
        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 2; i < 4; i++)
            {
                WheelFrictionCurve myWfc = wheelColliders[i].sidewaysFriction;
                myWfc.extremumSlip = 3.0f;
                wheelColliders[i].sidewaysFriction = myWfc;
                wheelColliders[i].brakeTorque = 200;
            }
        }

        if (speed < currentMaxSpeed)
        {
            if (!gearUp)
                acceleration = vV * bV * gears[currentGear];
        }
        else
        {
            acceleration = 0;
        }
        
        if (RPM >= maxRPM - 100 && currentGear < gears.Length - 1)
        {
            gearUp = true;
            StartCoroutine(GearUpIE());
            currentGear++;
            //RPM = 3500;
        }
        if (RPM <= 2500 && currentGear > 0)
        {
            gearUp = true;
            StartCoroutine(GearUpIE());
            currentGear--;
            //RPM = 3500;
        }

        RPM = Mathf.Lerp(RPM, Mathf.Abs(wheelColliders[0].rpm) * gears[currentGear] / rpmDecriseFactor, Time.deltaTime);
        RPM = Mathf.Clamp(RPM, 800, maxRPM);

        engineSound.pitch = pitchFromRPM[currentGear].Evaluate(RPM / 1500);
        //engineSound.pitch = RPM / 1500;// Mathf.Lerp(engineSound.pitch, RPM / 1500, Time.deltaTime);
        //engineSound.pitch = Mathf.Clamp(engineSound.pitch, 1, 3);
        skidFactor = Mathf.Clamp(skidFactor, 0, 1);
        skidSound.volume = Mathf.Lerp(skidSound.volume, skidFactor, Time.deltaTime * 5);
    }

    IEnumerator GearUpIE()
    {
        gearUp = true;
        acceleration = 0;
        RPM = 3000;
        yield return new WaitForSeconds(1f);
        gearUp = false;
    }

    private void FixedUpdate()
    {
        rigid.centerOfMass = com;

        if (gearUp)
            return;

        if (carType == CarType.front)
        {
            for (int i = 0; i < 2; i++)
            {
                wheelColliders[i].motorTorque = acceleration;
            }
            //RPM = (wheelColliders[0].rpm + wheelColliders[1].rpm) / rpmDecriseFactor * gears[currentGear];
        }
        if (carType == CarType.back)
        {
            for (int i = 2; i < 4; i++)
            {
                wheelColliders[i].motorTorque = acceleration;
            }
            //RPM = (wheelColliders[2].rpm + wheelColliders[3].rpm) / rpmDecriseFactor * gears[currentGear];
        }
        if (carType == CarType.full)
        {
            for (int i = 0; i < 4; i++)
            {
                wheelColliders[i].motorTorque = acceleration;
            }
            //RPM = (wheelColliders[2].rpm + wheelColliders[3].rpm) / rpmDecriseFactor * gears[currentGear];
        }
    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (speed > 30)
        {
            sfx.PlayOneShot(highColl);
        }
        else
        {
            sfx.PlayOneShot(lowColl);
        }

        if (health)
            health.GetDamage(speed);
        Instantiate(collGO, collision.contacts[0].point, Quaternion.identity);
    }
}