using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    public float delay = 1.2f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}