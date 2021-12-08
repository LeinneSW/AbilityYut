using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int Index = 0;
    public List<GameObject> Markers = new List<GameObject>();

    public GameObject Canvas;
    public List<GameObject> YutCanvas = new List<GameObject>();
    public GameObject YutFront, YutBack, YutBackWithBackDo, YutResultText, YutMarkerGuide;
    public GameObject YutObejct, YutObjectWithBackDo;

    private Text YutText { get; set; }

    public List<Yut.Result> results = new List<Yut.Result>();

    private void Start()
    {
        YutText = YutResultText.GetComponent<Text>();
    }

    public void ThrowYut()
    {
        StartCoroutine(CreateYut());
    }

    private IEnumerator CreateYut()
    {
        var backDoIndex = Random.Range(0, 4);
        var list = new List<Yut>();
        for (var i = 0; i < 4; ++i)
        {
            var yut = Instantiate(
                backDoIndex == i ? YutObjectWithBackDo : YutObejct,
                new Vector3(Random.Range(-1.5f, 1.5f), 3.5f, Random.Range(-1.5f, 1.5f)),
                Random.rotation
            );
            yut.GetComponent<Rigidbody>().AddForce(new(Random.Range(-235, 235f), 300, Random.Range(-235, 235f)));
            list.Add(yut.GetComponent<Yut>());
        }
        yield return new WaitForSeconds(2.55f);
        YutResultText.SetActive(true);

        var result = Yut.Result.MO;
        var isBack = false;
        for (int i = 0; i < 4; ++i)
        {
            var yut = list[i];
            if (yut == null)
            {
                result = Yut.Result.NAK;
                break;
            }
            result += (int)yut.StickResult;
            if (yut.IsBackMark && yut.StickResult == Yut.Stick.BACK)
            {
                isBack = true;
            }

            if (i == 3 && isBack && result == Yut.Result.DO)
            {
                result = Yut.Result.BACK_DO;
            }
        }
        YutText.text = result.ToFriendlyString();
        results.Add(result);
    }

    private IEnumerator DrawYut()
    {
        var list = Yut.ThrowYut(out Yut.Result result, out int backDoIndex);
        if (list != null)
        {
            for (int i = 0; i < 4; ++i)
            {
                var isBack = list[i] == Yut.Stick.BACK;
                var yutImage = Instantiate(
                    isBack ? (backDoIndex == i ? YutBackWithBackDo : YutBack) : YutFront,
                    new Vector3(isBack ? 18 : 0, 0),
                    Quaternion.identity
                );
                yutImage.transform.SetParent(YutCanvas[i].transform, false);
                yutImage.GetComponent<YutEffect>().RemoveTime = 0.3f * (3 - i) + 1.8f;
                yield return new WaitForSeconds(0.3f);
            }
        }
        YutResultText.SetActive(true);
        YutText.text = result.ToFriendlyString();
        results.Add(result);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);

            int index;
            var obj = hit.transform?.gameObject;
            if (obj != null && (index = Markers.IndexOf(obj)) >= 0)
            {
                obj.GetComponent<Marker>().OnClick(index, results.ToArray());
            }
        }
    }
}
