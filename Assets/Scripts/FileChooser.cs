using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;

public class FileChooser : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern string FileReaderCaptureClick();

    public TimeManager manager;

    public void FileSelected(string contents) {
        manager.AddTrace(contents);
    }

    public void OnButtonPointerDown() {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Open file","","");
        if (!System.String.IsNullOrEmpty(path)) {

            StreamReader reader = new StreamReader(path); 
            string content = reader.ReadToEnd();
            reader.Close();
            FileSelected(content);
        }
#else
        FileReaderCaptureClick();;
#endif
        
    }

    public static (float[], Vector3[], Quaternion[]) ParseData(string data) {
        JObject json = JObject.Parse(data);

        int length = ((JObject)json["ts"]).Count;
        float[] times = new float[length];
        Vector3[] positions = new Vector3[length];
        Quaternion[] rotations = new Quaternion[length];
        
        IEnumerator<JToken> timeEnumerator = json["ts"].Children().GetEnumerator();
        IEnumerator<JToken> xEnumerator = json["x"].Children().GetEnumerator();
        IEnumerator<JToken> yEnumerator = json["y"].Children().GetEnumerator();
        IEnumerator<JToken> zEnumerator = json["z"].Children().GetEnumerator();
        IEnumerator<JToken> rollEnumerator = json["roll"].Children().GetEnumerator();
        IEnumerator<JToken> pitchEnumerator = json["pitch"].Children().GetEnumerator();
        IEnumerator<JToken> yawEnumerator = json["yaw"].Children().GetEnumerator();

        float x = 0;
        float y = 0;
        float z = 0;

        for (int i = 0; i < length; i++) {
            timeEnumerator.MoveNext();
            xEnumerator.MoveNext();
            yEnumerator.MoveNext();
            zEnumerator.MoveNext();
            rollEnumerator.MoveNext();
            pitchEnumerator.MoveNext();
            yawEnumerator.MoveNext();

            times[i] = timeEnumerator.Current.First.Value<float>();
            positions[i] = new Vector3(-xEnumerator.Current.First.Value<float>(), yEnumerator.Current.First.Value<float>(), zEnumerator.Current.First.Value<float>());

            // https://developer.apple.com/documentation/scenekit/scnnode/1407980-eulerangles
            // Traces from the apple scene confirm that yaw is correct
            // Traces from the food scene confirm that pitch is correct
            // I'm perplexed about some traces from bird where the roll varies significantly
            x = pitchEnumerator.Current.First.Value<float>();
            y = yawEnumerator.Current.First.Value<float>();
            z = rollEnumerator.Current.First.Value<float>(); 

            Vector3 forward = Quaternion.AngleAxis(x, Vector3.right) * (Quaternion.AngleAxis(-y, Vector3.up) * (Quaternion.AngleAxis(-z, Vector3.forward) * Vector3.forward));
            Vector3 up = Quaternion.AngleAxis(x, Vector3.right) * (Quaternion.AngleAxis(-y, Vector3.up) * (Quaternion.AngleAxis(-z, Vector3.forward) * Vector3.up));
            rotations[i] = Quaternion.LookRotation(forward, up);

            // Quaternion test = Quaternion.AngleAxis(x, Vector3.right) * (Quaternion.AngleAxis(-y, Vector3.up) * (Quaternion.AngleAxis(-z, Vector3.forward) * Quaternion.identity));
            if (i <= 1) {
                Debug.Log(Quaternion.LookRotation(forward, up).ToString("F5"));
                Debug.Log(Quaternion.Angle(Quaternion.LookRotation(forward, up), new Quaternion(-0.002f, 0.606f, 0.202f, 0.770f)));
            }
        }  

        return (times, positions, rotations);
    }

    public static ((float[], Vector3[], Quaternion[])[], HashSet<int>) ParsePredictions(string data) {
        JObject json = JObject.Parse(data);

        int numTraces = json.Count;
        (float[], Vector3[], Quaternion[])[] traces = new (float[], Vector3[], Quaternion[])[numTraces];

        IEnumerator<JToken> traceEnumerator = json.Children().GetEnumerator();

        HashSet<int> gts = new HashSet<int>();

        int index = 0;
        while (traceEnumerator.MoveNext()) {
            JObject trace = (JObject) traceEnumerator.Current.First;
            int length = trace.Count;
            float[] times = new float[length];
            Vector3[] positions = new Vector3[length];
            Quaternion[] rotations = new Quaternion[length];

            string title = trace.Path;
            if (title.Contains("gt")) {
                gts.Add(index);
            }

            IEnumerator<JToken> frameEnumerator = trace.Children().GetEnumerator();
            for (int i = 0; i < length; i++) {
                frameEnumerator.MoveNext();
                JToken frame = frameEnumerator.Current.First;

                times[i] = frame["ts"].Value<float>();
                positions[i] = new Vector3(-frame["x"].Value<float>(), frame["y"].Value<float>(), frame["z"].Value<float>());
                times[i] = frame["ts"].Value<float>();
                times[i] = frame["ts"].Value<float>();

                float pitch = frame["pitch"].Value<float>();
                float yaw = frame["yaw"].Value<float>();
                float roll = frame["roll"].Value<float>(); 

                Vector3 forward = Quaternion.AngleAxis(pitch, Vector3.right) * (Quaternion.AngleAxis(-yaw, Vector3.up) * (Quaternion.AngleAxis(-roll, Vector3.forward) * Vector3.forward));
                Vector3 up = Quaternion.AngleAxis(pitch, Vector3.right) * (Quaternion.AngleAxis(-yaw, Vector3.up) * (Quaternion.AngleAxis(-roll, Vector3.forward) * Vector3.up));
                rotations[i] = Quaternion.LookRotation(forward, up);
            }

            traces[index] = (times, positions, rotations);
            index += 1;
        }

        return (traces, gts);
    }
}
