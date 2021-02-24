using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{

    public GameObject trace;
    public Button uploadButton;
    public Button pauseButton;
    public Text pauseText;
    public Button speedButton;
    public Text speedText;
    public Text timeText;
    public Slider timeline;
    public RectTransform timelineRectTransform;


    float speed = 1f;
    bool paused = true;
    public float time = 0;
    float maxTime = 1;
    bool dragging = false;

    // Start is called before the first frame update
    void Start()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
        speedButton.onClick.AddListener(OnSpeedClicked);
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused) {
            time = Mathf.Min(time + Time.deltaTime * speed, maxTime);
            timeline.value = time / maxTime;

            if (time >= maxTime) {
                SetPaused(true);
            }
        } else if (dragging) {
            time = timeline.value * maxTime;
        }

        ((RectTransform)timeline.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width - 40);
    }

    public void OnTimelineDown() {
        SetPaused(true);
        dragging = true;
    }

    public void OnTimelineUp() {
        time = timeline.value * maxTime;
        dragging = false;
    }

    void OnPauseClicked() {
        SetPaused(!paused);

        if (time >= maxTime) {
            timeline.value = 0f;
            time = 0f;
        }
    }

    void SetPaused(bool paused) {
        this.paused = paused;
        pauseText.text = paused ? "Pl" : "| |";
    }


    void OnSpeedClicked() {
        speed = Mathf.Approximately(speed, 4f) ? 0.25f : speed * 2;

        if (speed < 0.3f) {
            speedText.text = "x0.25";
        } else if (speed < 0.6f) {
            speedText.text = "x0.5";
        } else if (speed < 1.1f) {
            speedText.text = "x1";
        } else if (speed < 2.1f) {
            speedText.text = "x2";
        } else {
            speedText.text = "x4";
        }

    }

    float StoF(string str) {
        return float.Parse(str.TrimStart());
    }

    public void AddTrace(string content) {
        string[] lines = content.Split('\n');
        int length = lines.Length - 1;

        float[] times = new float[length];
        Vector3[] positions = new Vector3[length];
        Quaternion[] rotations = new Quaternion[length];

        char[] ends = new char[]{'[', ']'};
        for (int i = 0; i < length; i++) {
            string line = lines[i].Trim(ends);
            string[] cols = line.Split(',');

            times[i] = StoF(cols[2]);
            positions[i] = new Vector3(StoF(cols[3]), StoF(cols[4]), StoF(cols[5]));

            rotations[i] = Quaternion.Euler(StoF(cols[6]), StoF(cols[7]), StoF(cols[8]));
        }


        float lastTime = times[times.Length - 1];
        if (lastTime > maxTime) {
            maxTime = lastTime;
            timeline.value = time / maxTime;
        }

        GameObject trace = GameObject.Instantiate(this.trace);
        trace.SetActive(true);

        trace.GetComponent<Trace>().Init(times, positions, rotations);
    }


}
