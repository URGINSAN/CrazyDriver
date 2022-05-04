using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skid : MonoBehaviour
{
    public TrailRenderer trail;
    public ParticleSystem particle;

    private void Start()
    {
        SetState(false);
    }

    public void SetState(bool state)
    {
        if (state)
        {
            trail.emitting = true;
            particle.Play();
        }
        else
        {
            trail.emitting = false;
            particle.Stop();
        }
    }
}