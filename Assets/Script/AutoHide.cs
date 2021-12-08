using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float RemoveTime;
    private float Tick = 0;

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        Tick += Time.deltaTime;
        if(Tick >= RemoveTime)
        {
            gameObject.SetActive(false);
            Tick = 0;
        }
    }

}
