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


        for (int i = 0; i < length; i++) {
            timeEnumerator.MoveNext();
            xEnumerator.MoveNext();
            yEnumerator.MoveNext();
            zEnumerator.MoveNext();
            rollEnumerator.MoveNext();
            pitchEnumerator.MoveNext();
            yawEnumerator.MoveNext();


            times[i] = timeEnumerator.Current.First.Value<float>();
            positions[i] = new Vector3(xEnumerator.Current.First.Value<float>(), yEnumerator.Current.First.Value<float>(), zEnumerator.Current.First.Value<float>());
            rotations[i] = Quaternion.Euler(rollEnumerator.Current.First.Value<float>(), pitchEnumerator.Current.First.Value<float>(), yawEnumerator.Current.First.Value<float>());

        }  

        return (times, positions, rotations);
    }
}
