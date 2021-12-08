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

    public static Stick ThrowYutStick()
    {
        return new System.Random().NextDouble() >= 0.611015470351657 ? Stick.FRONT : Stick.BACK;
    }

    public static Stick[] ThrowYut(out Result result, out int backDoIndex)
    {
        backDoIndex = Random.Range(0, 4);
        if (Random.Range(0, 200) == 0) // 0.5% È®·ü·Î ³«
        {
            result = Result.NAK;
            return null;
        }
        var intResult = 0;
        var isBackDo = false;
        var yutList = new Stick[]
        {
            ThrowYutStick(),
            ThrowYutStick(),
            ThrowYutStick(),
            ThrowYutStick()
        };
        for (int i = 0; i < 4; ++i)
        {
            var stick = yutList[i];
            intResult += (int)stick;
            if (stick == Stick.BACK && i == backDoIndex)
            {
                isBackDo = true;
            }
        }
        result = isBackDo && intResult == 1 ? Result.BACK_DO : (Result)intResult;
        return yutList;
    }

    public float Tick = 0;
    public bool IsBackMark = false;
    public Stick StickResult = Stick.NONE;

    private void Update()
    {
        if (transform.position.y < -1)
        {
            Destroy(gameObject);
            return;
        }

        if (Tick == -1) return;
        Tick += Time.deltaTime;
        if (Tick >= 2.5)
        {
            Tick = -1;
            var rotate = transform.up.normalized;
            StickResult = rotate.y >= 0.1 ? Stick.FRONT : Stick.BACK;
            //Debug.Log("Rotate: " + rotate + " Result: " + StickResult);
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
                return "³«";
            case Yut.Result.BACK_DO:
                return "µÞµµ";
            case Yut.Result.MO:
                return "¸ð";
            case Yut.Result.DO:
                return "µµ";
            case Yut.Result.GAE:
                return "°³";
            case Yut.Result.GEOL:
                return "°É";
            default:
                return "À·";
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
