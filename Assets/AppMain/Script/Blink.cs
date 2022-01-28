using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//TextかImageを点滅させる
public class Blink : MonoBehaviour
{
    public float speed = 1.0f;

    private TextMeshProUGUI Text;
    private Image image;
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        Text = this.gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Text.color = GetAlphaColor(Text.color);
    }

    //Alpha値を更新してColorを返す
    Color GetAlphaColor(Color color)
    {
        time += Time.deltaTime * 5.0f * speed;
        color.a = Mathf.Sin(time) * 0.5f + 0.5f;

        return color;
    }
}