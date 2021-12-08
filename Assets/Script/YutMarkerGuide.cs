using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YutMarkerGuide : MonoBehaviour
{
    public GameObject marker;
    public Marker.Point point;

    private Marker Marker;

    private void Start()
    {
        Marker = marker.GetComponent<Marker>();   
    }

    public void OnClick()
    {
        Marker.Move(point);
        Debug.Log(point);
    }
}
