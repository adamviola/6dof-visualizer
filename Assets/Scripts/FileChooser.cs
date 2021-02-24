using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

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
}
