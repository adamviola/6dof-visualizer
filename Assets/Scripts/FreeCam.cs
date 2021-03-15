using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A simple free camera to be added to a Unity game object.
/// 
/// Keys:
///	wasd / arrows	- movement
///	q/e 			- up/down (local space)
///	r/f 			- up/down (world space)
///	pageup/pagedown	- up/down (world space)
///	hold shift		- enable fast movement mode
///	right mouse  	- enable free look
///	mouse			- free look / rotation
///     
/// </summary>
public class FreeCam : MonoBehaviour
{
    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    public float movementSpeed = 10f;

    /// <summary>
    /// Speed of camera movement when shift is held down,
    /// </summary>
    public float fastMovementSpeed = 100f;

    /// <summary>
    /// Sensitivity for free look.
    /// </summary>
    public float freeLookSensitivity = 3f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel.
    /// </summary>
    public float zoomSensitivity = 10f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel (fast mode).
    /// </summary>
    public float fastZoomSensitivity = 50f;

    /// <summary>
    /// Set to true when free looking (on right mouse button).
    /// </summary>
    private bool looking = false;

    public SceneManager sceneManager;
    public TimeManager timeManager;

    private bool cinematic = false;
    private float cinY = 0;
    private float cinDist = 0;
    private float cinAngle = 0;
    private Vector3 cinTarget = Vector3.zero;
    private Vector3 cinFocus = Vector3.zero;

    void Update()
    {
        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
        {
            transform.position = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
        {
            transform.position = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
        }

        if (looking)
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
            transform.position = transform.position + transform.forward * axis * zoomSensitivity;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartLooking();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            StopLooking();
        }

        if (Input.GetKey(KeyCode.C)) {     
            if (!cinematic) {
                GameObject scene = sceneManager.currentScene;
                if (scene == null)
                    return;

                cinematic = true;

                string sceneName = scene.name;
                Vector3 focus = Vector3.zero;
                if (sceneName.Equals("planets")) {
                    focus = new Vector3(0, 0, -2f);
                } else if (sceneName.Equals("apples")) {
                    focus = Vector3.zero;
                } else if (sceneName.Equals("food")) {
                    focus = new Vector3(0, 0, -1.5f);
                } else if (sceneName.Equals("random")) {
                    focus = Vector3.zero;
                } else if (sceneName.Equals("bird")) {
                    focus = new Vector3(0, 0, -0.5f);
                } else {
                    focus = Vector3.zero;
                }

                Vector3 pos = focus - transform.position;

                Vector3 v1 = transform.rotation * Vector3.forward;
                Vector3 v2 = Vector3.Cross(v1, Vector3.up).normalized;

                float b = (pos.z - pos.x * v1.z / v1.x) / (v2.z - v2.x * v1.z / v1.x);
                float a = (pos.x - b * v2.x) / v1.x;

                cinFocus = new Vector3(focus.x, a * v1.y + b * v2.y + transform.position.y, focus.z);

                cinY = transform.position.y;
                pos = transform.position - cinFocus;
                cinAngle = Mathf.Atan2(pos.z, pos.x);
                cinDist = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);

                timeManager.OnPauseClicked();
            }       
        } else if (Input.anyKey) {
            cinematic = false;
        }

        if (cinematic) {
            cinAngle += Time.deltaTime * 0.25f;

            transform.position = new Vector3(cinDist * Mathf.Cos(cinAngle), cinY - cinFocus.y, cinDist * Mathf.Sin(cinAngle)) + cinFocus;
            transform.rotation = Quaternion.LookRotation(cinFocus - transform.position, Vector3.up);
        }
    }

    void OnDisable()
    {
        StopLooking();
    }

    /// <summary>
    /// Enable free looking.
    /// </summary>
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Disable free looking.
    /// </summary>
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}