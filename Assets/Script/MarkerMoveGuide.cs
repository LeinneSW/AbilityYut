using UnityEngine;
using UnityEngine.UI;
using static Marker;

public class MarkerMoveGuide : MonoBehaviour
{
    public Point point;
    public Marker marker;

    private Yut.Result _result;
    public Yut.Result Result
    {
        get => _result;
        set
        {
            _result = value;
            GetComponentInChildren<Text>().text = value.ToFriendlyString();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Destroy(gameObject, 0.2f);
        }
    }

    public void OnClick()
    {
        marker.Move(Result, point);
    }
}
