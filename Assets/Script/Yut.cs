using UnityEngine;

public class Yut : MonoBehaviour
{
    public enum Result
    {
        NAK = -2,
        BACK_DO,
        MO,
        DO,
        GAE,
        GEOL,
        YUT
    }

    public enum Stick
    {
        NONE = -1,
        FRONT,
        BACK
    }

    public bool isBackMark = false;
    public Stick stickResult = Stick.NONE;

    private Rigidbody rb = null;
    public Rigidbody Rigid
    {
        get
        {
            rb ??= GetComponent<Rigidbody>();
            return rb;
        }
    }
    public AudioSource yutThrow, yutFall;

    private void Update()
    {
        if (transform.position.y < -1)
        {
            Destroy(gameObject);
            return;
        }

        if (stickResult == Stick.NONE && transform.position.y < 2 && Rigid.velocity.magnitude <= 0.05)
        {
            var rotate = transform.up.normalized;
            stickResult = rotate.y >= 0.5 ? Stick.FRONT : Stick.BACK;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Board"))
        {
            yutFall.Play();
        }
    }
}

public static class ResultExtensions
{
    public static string ToFriendlyString(this Yut.Result result)
    {
        switch (result)
        {
            case Yut.Result.NAK:
                return "낙";
            case Yut.Result.BACK_DO:
                return "뒷도";
            case Yut.Result.MO:
                return "모";
            case Yut.Result.DO:
                return "도";
            case Yut.Result.GAE:
                return "개";
            case Yut.Result.GEOL:
                return "걸";
            default:
                return "윷";
        }
    }

    public static int GetValue(this Yut.Result result)
    {
        switch (result)
        {
            case Yut.Result.NAK:
                return 0;
            case Yut.Result.MO:
                return 5;
            default:
                return (int)result;
        }
    }
}
