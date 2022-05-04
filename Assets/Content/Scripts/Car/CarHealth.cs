using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarHealth : MonoBehaviour
{
    public float health;
    public GameObject smokeGO;
    public GameObject fireGO;

    private void Start()
    {
    }

    public void GetDamage(float speed)
    {
        health -= speed / 10;
        health = Mathf.Clamp(health, 0, 100);

        if (health < 40)
            smokeGO.SetActive(true);
        if (health < 10)
            fireGO.SetActive(true);
    }
}