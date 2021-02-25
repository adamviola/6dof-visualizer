using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class TimeManager : MonoBehaviour
{
    public RectTransform content;
    public GameObject slot;
    public GameObject trace;
    public Button uploadButton;
    public Button pauseButton;
    public Text pauseText;
    public Button speedButton;
    public Text speedText;
    public Text timeText;
    public Slider timeline;
    public RectTransform timelineRectTransform;
    public Dictionary<GameObject, float> traceLengths;


    float speed = 1f;
    bool paused = true;
    public float time = 0;
    public float maxTime = 1;
    bool dragging = false;
    int numUploaded = 0;

    // Start is called before the first frame update
    void Start()
    {
        traceLengths = new Dictionary<GameObject, float>();
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


        timeText.text = (int) time / 60 + ":" + ((int) time % 60 < 10 ? "0" : "") + (int)time % 60 + " / " + (int) maxTime / 60 + ":" + ((int) maxTime % 60 < 10 ? "0" : "") + (int)maxTime % 60;
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
        pauseText.text = paused ? ">" : "| |";
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
        // string[] lines = content.Split('\n');
        // int length = lines.Length - 1;

        

        // char[] ends = new char[]{'[', ']'};
        // for (int i = 0; i < length; i++) {
        //     string line = lines[i].Trim(ends);
        //     string[] cols = line.Split(',');

        //     times[i] = StoF(cols[2]);
        //     positions[i] = new Vector3(StoF(cols[3]), StoF(cols[4]), StoF(cols[5]));

        //     rotations[i] = Quaternion.Euler(StoF(cols[6]), StoF(cols[7]), StoF(cols[8]));
        // }

        (float[], Vector3[], Quaternion[]) parsedContent = FileChooser.ParseData(content);
        float[] times = parsedContent.Item1;
        Vector3[] positions = parsedContent.Item2;
        Quaternion[] rotations = parsedContent.Item3;

        float lastTime = times[times.Length - 1];
        if (lastTime > maxTime) {
            maxTime = lastTime;
            timeline.value = time / maxTime;
        }

        GameObject trace = GameObject.Instantiate(this.trace);
        trace.SetActive(true);
        trace.GetComponent<Trace>().Init(times, positions, rotations);

        traceLengths.Add(trace, lastTime);

        GameObject slot = GameObject.Instantiate(this.slot, this.slot.transform.parent);
        slot.GetComponent<Slot>().trace = trace.GetComponent<Trace>();
        slot.SetActive(true);

        ((RectTransform) slot.transform).anchoredPosition += new Vector2(0, -20 * numUploaded);


        numUploaded++;

        this.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, numUploaded * 20);
    }

    public void DeleteTrace(GameObject trace) {
        float length = traceLengths[trace];
        traceLengths.Remove(trace);

        if (Mathf.Approximately(maxTime, length)) {
            float newMax = 1;
            foreach(float l in traceLengths.Values) {
                if (l > newMax) {
                    newMax = l;
                }
            }
            maxTime = newMax;
            timeline.value = time / maxTime;
        }
    }


}
