using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRemove : MonoBehaviour
{
    public float RemoveTime;

    private void Start()
    {
        Destroy(gameObject, RemoveTime);
    }
}
