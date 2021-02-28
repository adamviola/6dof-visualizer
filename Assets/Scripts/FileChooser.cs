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
            
        }  

        return (times, positions, rotations);
    }
}
