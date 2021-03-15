using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Trace : MonoBehaviour
{

    public TimeManager manager;

    float[] times;
    Vector3[] positions;
    Quaternion[] rotations;
    public LineRenderer line;

    public MeshRenderer cube;
    public GameObject quad1;
    public GameObject quad2;

    int lastCount = 0;
    
    public void Init(float[] times, Vector3[] positions, Quaternion[] rotations) {
        this.times = times;
        this.positions = positions;
        this.rotations = rotations;

        line = GetComponent<LineRenderer>();
        line.positionCount = positions.Length;
        line.SetPositions(positions);

        lastCount = positions.Length;

        Update();
    }

    
    void Update()
    {
        if (manager.time < times[0]) {
            if (line.enabled) {
                line.enabled = false;
                if (cube != null) {
                    cube.enabled = false;
                    quad1.SetActive(false);
                    quad2.SetActive(false);
                }
            }
            return;

        } else if (!line.enabled && manager.time >= times[0]) {
            line.enabled = true;
            if (cube != null) {
                cube.enabled = true;
            quad1.SetActive(true);
            quad2.SetActive(true);
            }
        }


        int newCount = Array.BinarySearch(times, manager.time);

        if (newCount < 0) {
            newCount = newCount * -1 - 1; // Not sure why this is
        } else {
            newCount += 1;
        }


        newCount = Mathf.Min(newCount, times.Length);

        if (newCount != lastCount) {

            line.positionCount = newCount;

            if (newCount > lastCount) {
                for (int i = lastCount; i < newCount; i++) {
                    line.SetPosition(i, positions[i]);
                }
            }

            lastCount = newCount;
        }

        if (times[newCount - 1] != manager.time) {
            int start = newCount - 1;
            int end = Mathf.Min(newCount, times.Length - 1);
            float t = (manager.time - times[start]) / (times[end] - times[start]);

            transform.position = Vector3.Lerp(positions[start], positions[end], t);
            transform.rotation = Quaternion.Slerp(rotations[start], rotations[end], t);
        } else {
            transform.position = positions[newCount - 1];
            transform.rotation = rotations[newCount - 1];
        }
    }
}
