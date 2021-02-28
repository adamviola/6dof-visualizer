using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public TimeManager manager;
    public GameObject trace;
    public GameObject slot;
    public Transform root;
    public GameObject currentScene;
    Slot[] slots;

    int requests = 0;

    void Start()
    {
        slots = new Slot[50];
    }

    void Update()
    {
        // if (request != null && request.isDone) {
        //     Debug.Log(request.asset);
        // }
    }

    public void ChangeScene(string scene) {
        if ((currentScene != null && currentScene.name.Equals(scene)) || requests > 0) {
            return;
        }

        foreach (Slot slot in slots) {
            if (slot != null) {
                slot.OnClickDelete();
            }
        }
        slots = new Slot[50];


        if (currentScene != null) {
            currentScene.SetActive(false);
        }

        Transform nextScene = root.Find(scene);

        if (nextScene == null) {
            currentScene = null;
        } else {
            currentScene = nextScene.gameObject;
            currentScene.SetActive(true);
        }

        requests = 50;
        for (int i = 1; i <= 50; i++) {
            ResourceRequest request = Resources.LoadAsync<TextAsset>(scene + "/" + i);
            request.completed += CompletedRequest;
        }
    }

    void CompletedRequest(AsyncOperation op) {
        ResourceRequest request = (ResourceRequest) op;

        TextAsset content = (TextAsset) request.asset;

        (float[], Vector3[], Quaternion[]) parsedContent = FileChooser.ParseData(content.text);
        float[] times = parsedContent.Item1;
        Vector3[] positions = parsedContent.Item2;
        Quaternion[] rotations = parsedContent.Item3;


        GameObject trace = GameObject.Instantiate(this.trace);
        // trace.SetActive(true);
        trace.GetComponent<Trace>().Init(times, positions, rotations);

        // traceLengths.Add(trace, lastTime);

        GameObject slot = GameObject.Instantiate(this.slot, this.slot.transform.parent);
        Slot slotScript = slot.GetComponent<Slot>();
        slotScript.trace = trace.GetComponent<Trace>();
        slotScript.visibilityToggle.isOn = false;
        slotScript.text.text = content.name;
        slot.SetActive(true);


        int i = int.Parse(content.name);
        ((RectTransform) slot.transform).anchoredPosition += new Vector2(0, -20 * i + 20);

        slots[i-1] = slotScript;
        requests--;

        float lastTime = times[times.Length - 1];
        if (lastTime > manager.maxTime) {
            manager.maxTime = lastTime;
            manager.timeline.value = manager.time / manager.maxTime;
        }
        manager.traceLengths.Add(trace, lastTime);
    }

    public void ToggleAll() {
        bool target = false;

        foreach (Slot slot in slots) {
            if (slot != null) {
                if (!slot.visibilityToggle.isOn) {
                    target = true;
                    break;
                }
            }
        }

        foreach (Slot slot in slots) {
            if (slot != null) {
                slot.visibilityToggle.isOn = target;
                slot.OnToggleVisibility();
            }
        }
    }
}

// update maxTime
// store trace and slot so they can be deleted on scene switch
