using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Slot : MonoBehaviour
{
    static Color[] colors;
    
    static Slot() {
        colors = new Color[]{ Color.red, Color.yellow, Color.green, Color.blue, Color.white };
    }

    public TimeManager manager;
    public Trace[] traces;
    public Button colorButton;
    public Toggle visibilityToggle;
    public Text text;

    int colorIndex = -1;

    void Start()
    {
        OnClickColor();
    }

    public void OnToggleVisibility() {
        foreach (Trace trace in traces) {
            trace.gameObject.SetActive(visibilityToggle.isOn);
        }
    }

    public void OnClickColor() {
        colorIndex = (colorIndex + 1) % colors.Length;
        foreach (Trace trace in traces) {
            Color color = colors[colorIndex];

            // trace.line.material.color =  color;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] {new GradientColorKey(color, 0f)}, new GradientAlphaKey[]{new GradientAlphaKey(1f, 0f)});
            trace.line.colorGradient = grad;

            ColorBlock colorBlock = ColorBlock.defaultColorBlock;
            colorBlock.disabledColor = color;
            colorBlock.highlightedColor = color;
            colorBlock.normalColor = color;
            colorBlock.pressedColor = color;
            colorBlock.selectedColor = color;

            colorButton.colors = colorBlock;
        }

    }

    public void OnClickDelete() {
        foreach (Trace trace in traces) {
            manager.DeleteTrace(trace.gameObject);
            GameObject.Destroy(trace.gameObject);
        }
        GameObject.Destroy(gameObject);
    }
}
