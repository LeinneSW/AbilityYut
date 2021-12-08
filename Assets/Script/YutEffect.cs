using UnityEngine;
using UnityEngine.UI;

public class YutEffect : MonoBehaviour
{
    private Image Image;
    private Color Color;
    private bool IsSpawned = false;

    private float Tick = 0;
    public float RemoveTime = 0;

    private void Start()
    {
        Image = GetComponent<Image>();
        Color = Image.color;
        Color.a = 0;
        Image.color = Color;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsSpawned && Color.a <= 0)
        {
            //Destroy(gameObject);
            return;
        }

        if (!IsSpawned)
        {
            Color.a += Time.deltaTime * 3.33f;
            Image.color = Color;
            if(Color.a > 1)
            {
                IsSpawned = true;
            }
            return;
        }

        Tick += Time.deltaTime;
        if (Tick >= RemoveTime)
        {
            Color.a -= Time.deltaTime * 3.33f;
            Image.color = Color;
            return;
        }
        return;
    }
}
