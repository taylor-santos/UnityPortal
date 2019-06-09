using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    public GameObject Other;
    public int Depth;

    class PortalCamera
    {
        public Camera camera;
        public PortalCameraTexture controller;
    }

    private List<PortalCamera> cameras;
    private Camera finalCamera;
    private CameraPlaneProjection finalRenderer;
    private Matrix4x4 initialProjection;

    void Start()
    {
        if (Depth < 0) Depth = 0;
        cameras = new List<PortalCamera>(Depth + 2);

        cameras.Add(new PortalCamera());
        cameras[0].camera = Camera.main;

        //Shader clipShader = Shader.Find("Standard (Clipped)");

        for (int i = 1; i < Depth + 1; i++)
        {
            cameras.Add(new PortalCamera());

            GameObject camObj = new GameObject("Camera " + i + " (" + name + ")");
            cameras[i].camera = camObj.AddComponent<Camera>();
            cameras[i].camera.CopyFrom(Camera.main);
            cameras[i].camera.transform.parent = Other.transform;
            cameras[i].camera.depth = cameras[i - 1].camera.depth - 1;
            //cameras[i].camera.SetReplacementShader(clipShader, "RenderType");
            cameras[i].camera.cullingMask &= ~(1 << Other.layer);

            cameras[i].controller = camObj.AddComponent<PortalCameraTexture>();
            cameras[i].controller.PortalRenderers = new List<Renderer>();
            cameras[i].controller.PortalRenderers.Add(GetComponent<Renderer>());
        }
        GameObject finalObj = new GameObject("Camera " + (Depth + 1) + " (" + name + ")");
        finalCamera = finalObj.AddComponent<Camera>();
        finalCamera.CopyFrom(Camera.main);
        finalCamera.fieldOfView = 30;
        finalCamera.transform.parent = Other.transform;
        finalCamera.depth = cameras[Depth].camera.depth - 1;
        //finalCamera.SetReplacementShader(clipShader, "RenderType");
        finalCamera.cullingMask &= ~(1 << Other.layer);

        finalRenderer = finalObj.AddComponent<CameraPlaneProjection>();
        finalRenderer.Plane = Other;
        finalRenderer.Perspective = cameras[Depth].camera.gameObject;
        finalRenderer.PortalRenderers = new List<Renderer>();
        finalRenderer.PortalRenderers.Add(GetComponent<Renderer>());
    }
    
    void Update()
    {
        Vector3 position = Other.transform.position;
        Vector3 normal = Other.transform.forward;
        Vector3 disp = transform.position - Camera.main.transform.position;
        if (Vector3.Dot(disp, transform.forward) < 0)
            normal = -normal;
        position -= normal * 0.01f;

        for (int i = 1; i < Depth+1; i++)
        {
            cameras[i].camera.transform.parent = Other.transform;
            cameras[i].camera.transform.localPosition =
                transform.InverseTransformPoint(
                    cameras[i - 1].camera.transform.position
                );
            cameras[i].camera.transform.localRotation =
                Quaternion.Inverse(transform.rotation) * 
                cameras[i - 1].camera.transform.rotation;

            cameras[i].camera.transform.parent = null;
            cameras[i].camera.transform.localScale = Vector3.one;

            cameras[i].controller.Position = position;
            cameras[i].controller.Normal = normal;
        }
        finalCamera.transform.parent = Other.transform;
        finalCamera.transform.localPosition =
            transform.InverseTransformPoint(
                cameras[Depth].camera.transform.position
            );
        finalCamera.transform.localRotation =
            Quaternion.Inverse(transform.rotation) *
            cameras[Depth].camera.transform.rotation;
        finalCamera.transform.parent = null;
        //finalCamera.transform.LookAt(position);
        finalCamera.transform.localScale = Vector3.one;
        finalRenderer.Position = position;
        finalRenderer.Normal = normal;
        finalRenderer.Attenuation = Mathf.Min(1f, Other.transform.localScale.z / transform.localScale.z);
    }
}
