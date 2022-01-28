using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public GameObject Camera;
    public GameObject Canvas;

    // Update is called once per frame
    void Update()
    {       
        if (Camera)
        {
            Canvas.transform.position = Camera.transform.position + Camera.transform.forward * 1.0f;
            Quaternion CameraRot = Camera.transform.rotation;
            CameraRot.x = 0f;   // Canvasが斜めにならないように調整
            CameraRot.z = 0f;   // Canvasが斜めにならないように調整
            Canvas.transform.rotation = CameraRot;
        }
                
    }
}
