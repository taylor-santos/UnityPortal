using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraClip : MonoBehaviour
{

    void OnPreRender()
    {
        
    }

    void OnPostRender()
    {
        Shader.SetGlobalVector("_PlanePosition", new Vector4(0, 1000, 0, 0));
        Shader.SetGlobalVector("_PlaneNormal", new Vector4(0, -1, 0, 0));
    }
}
