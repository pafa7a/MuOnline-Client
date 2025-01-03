using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{

    public float updateInterval = 0.2f; //How often should the number update

    public bool show = true;

    public TMP_Text txt;
    float time = 0.0f;
    int frames = 0;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Prevent destruction on scene load
    }

    // Update is called once per frame
    void Update()
    {
        txt.gameObject.SetActive(show);
        if (!show) {
            return;
        }
        time += Time.unscaledDeltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (time >= updateInterval)
        {
            float fps = (int)(frames / time);
            time = 0.0f;
            frames = 0;

            txt.text = fps.ToString() + " FPS";
        }
    }
}