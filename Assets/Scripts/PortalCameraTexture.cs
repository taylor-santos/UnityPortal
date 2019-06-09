using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCameraTexture : MonoBehaviour
{
    public List<Renderer> PortalRenderers;
    public Vector3 Position;
    public Vector3 Normal;

    
    private Camera renderCam;
    private int screenWidth, screenHeight;
    private Matrix4x4 projection;

    void Start()
    {
        renderCam = GetComponent<Camera>();
        projection = renderCam.projectionMatrix;
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        renderCam.targetTexture = new RenderTexture(
            screenWidth * 2,
            screenHeight * 2,
            16 // Set to 24 if stencil buffer is needed
        );
    }

    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
    {
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(pos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    void SetObliqueMatrix(Camera cam)
    {
        cam.projectionMatrix = projection;
        if (Vector3.Dot(cam.transform.forward, Position - cam.transform.position) >= 0)
        {
            cam.projectionMatrix = cam.CalculateObliqueMatrix(
                CameraSpacePlane(cam, Position, Normal)
            );
        }
    }

    void Update()
    {
        if (Screen.width != screenWidth || Screen.height != screenHeight)
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            renderCam.targetTexture.Release();
            renderCam.targetTexture = new RenderTexture(
                screenWidth * 2,
                screenHeight * 2,
                16 // Set to 24 if stencil buffer is needed
            );
        }
        SetObliqueMatrix(renderCam);
    }

    void OnPostRender()
    {
        foreach (Renderer r in PortalRenderers) {
            r.material.SetTexture("_MainTex", renderCam.targetTexture);
            r.material.SetFloat("_Crop", 0f);
            r.material.SetFloat("_Attenuation", 1f);
        }
    }
}
