using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlaneProjection : MonoBehaviour
{
    public GameObject Plane;
    public bool Method = true;
    public GameObject Perspective;
    public Vector3 Position;
    public Vector3 Normal;
    public List<Renderer> PortalRenderers;
    public float Attenuation;

    public List<Vector3> Ps;
    private Camera perspectiveCam;
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
            screenWidth,
            //(int)(transform.localScale.x / transform.localScale.y * screenWidth),
            screenHeight,
            16 // Set to 24 if stencil buffer is needed
        );

        Ps = new List<Vector3>(4);
        for (int i = 0; i < 4; i++)
        {
            Ps.Add(new Vector4());
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
                screenWidth,
                screenHeight,
                16 // Set to 24 if stencil buffer is needed
            );
        }
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
        Debug.Log(Vector3.Dot(cam.transform.forward, Position - cam.transform.position));
        if (Vector3.Dot(cam.transform.forward, Position - cam.transform.position) < 0)
        {
            cam.enabled = false;
        }
        else
        {
            cam.enabled = true;
            cam.projectionMatrix = projection;
            cam.projectionMatrix = cam.CalculateObliqueMatrix(
                CameraSpacePlane(cam, Position, Normal)
            );
        }
    }

    void LateUpdate()
    {
        renderCam.transform.LookAt(Plane.transform, Plane.transform.up);
        if (Method)
        {
            Vector3 X = Plane.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
            Vector3 dX = Plane.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
            Vector3 dY = Plane.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));

            X = renderCam.worldToCameraMatrix.MultiplyPoint(X);
            dX = renderCam.worldToCameraMatrix.MultiplyPoint(dX);
            dY = renderCam.worldToCameraMatrix.MultiplyPoint(dY);

            dX -= X;
            dY -= X;
            if (Vector3.Cross(dX, dY).z < 0)
            {
                X += dX;
                dX = -dX;
                foreach (Renderer r in PortalRenderers)
                {
                    r.material.SetVector("_Top", new Vector4(1, 0, 0, 0));
                    r.material.SetVector("_Bottom", new Vector4(1, 1, 0, 1));
                }
            } else
            {
                foreach (Renderer r in PortalRenderers)
                {
                    r.material.SetVector("_Top", new Vector4(0, 0, 1, 0));
                    r.material.SetVector("_Bottom", new Vector4(0, 1, 1, 1));
                }
            }

            Matrix4x4 p = new Matrix4x4();
            float k = 10;

            /* Mathematica code used to generate the necessary projection matrix:
                P := Array[p, {4, 4}, {0, 0}] /. {p[0, 0] -> 1, p[0, 3] -> 0, p[1, 3] -> 0, p[3, 3] -> 0}
                a := Array[X, 3, 0]
                b := Array[dX, 3, 0]
                c := Array[dY, 3, 0]

                Q = Solve[
                    P.Append[a, 1]         == P[[4]].Append[a, 1]         {-1, -1, -1, 1} &&
                    P.Append[k a, 1]       == P[[4]].Append[k a, 1]       {-1, -1,  1, 1}  &&
                    P.Append[a + b, 1]     == P[[4]].Append[a + b, 1]     { 1, -1, -1, 1}  &&
                    P.Append[k a + k b, 1] == P[[4]].Append[k a + k b, 1] { 1, -1,  1, 1}   &&
                    P.Append[a + c, 1]     == P[[4]].Append[a + c, 1]     {-1,  1, -1, 1}  &&
                    P.Append[k a + k c, 1] == P[[4]].Append[k a + k c, 1] {-1,  1,  1, 1}, 
                    DeleteCases[Flatten[P], 0 | 1]
                ]

                R = FullSimplify[Q][[1]]
            */

            p[0, 0] = 1;
            p[0, 1] = (dY[2] * (dX[0] + 2 * X[0]) - dY[0] * (dX[2] + 2 * X[2])) / (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]);
            p[0, 2] = (dY[1] * (dX[0] + 2 * X[0]) - dY[0] * (dX[1] + 2 * X[1])) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
            p[1, 0] = (dX[2] * (dY[1] + 2 * X[1]) - dX[1] * (dY[2] + 2 * X[2])) / (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]);
            p[1, 1] = ((-dX[2]) * (dY[0] + 2 * X[0]) + dX[0] * (dY[2] + 2 * X[2])) / (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]);
            p[1, 2] = ((-dX[1]) * (dY[0] + 2 * X[0]) + dX[0] * (dY[1] + 2 * X[1])) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
            p[2, 0] = -(((dX[2] * dY[1] - dX[1] * dY[2]) * (1 + k)) / ((-1 + k) * (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2])));
            p[2, 1] = ((dX[2] * dY[0] - dX[0] * dY[2]) * (1 + k)) / ((-1 + k) * (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]));
            p[2, 2] = ((dX[1] * dY[0] - dX[0] * dY[1]) * (1 + k)) / ((-1 + k) * (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2])));
            p[2, 3] = (2 * k * (dX[2] * dY[1] * X[0] - dX[1] * dY[2] * X[0] - dX[2] * dY[0] * X[1] + dX[0] * dY[2] * X[1] + dX[1] * dY[0] * X[2] - dX[0] * dY[1] * X[2])) / ((-1 + k) * (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]));
            p[3, 0] = ((-dX[2]) * dY[1] + dX[1] * dY[2]) / (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]);
            p[3, 1] = (dX[2] * dY[0] - dX[0] * dY[2]) / (dX[2] * dY[1] - dY[2] * (dX[1] + 2 * X[1]) + 2 * dY[1] * X[2]);
            p[3, 2] = (dX[1] * dY[0] - dX[0] * dY[1]) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
            renderCam.projectionMatrix = p;

            List<Vector3> frustum = new List<Vector3>();
            frustum.Add(new Vector3(-1, -1, -1));
            frustum.Add(new Vector3(1, -1, -1));
            frustum.Add(new Vector3(1, 1, -1));
            frustum.Add(new Vector3(-1, 1, -1));
            frustum.Add(new Vector3(-1, -1, 1f));
            frustum.Add(new Vector3(1, -1, 1f));
            frustum.Add(new Vector3(1, 1, 1f));
            frustum.Add(new Vector3(-1, 1, 1f));

            for (int i = 0; i < frustum.Count; i++)
            {
                frustum[i] = p.inverse.MultiplyPoint(frustum[i]);
                frustum[i] = renderCam.cameraToWorldMatrix.MultiplyPoint(frustum[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                Debug.DrawLine(frustum[i], frustum[(i + 1) % 4], Color.green);
                Debug.DrawLine(frustum[i + 4], frustum[(i + 1) % 4 + 4]);
                Debug.DrawLine(frustum[i], frustum[i + 4]);
            }
        } else { 
            Ps[0] = Plane.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
            Ps[1] = Plane.transform.TransformPoint(new Vector3( 0.5f, -0.5f, 0));
            Ps[2] = Plane.transform.TransformPoint(new Vector3( 0.5f,  0.5f, 0));
            Ps[3] = Plane.transform.TransformPoint(new Vector3(-0.5f,  0.5f, 0));

            for (int i = 0; i < 4; i++)
            {
                Ps[i] = renderCam.worldToCameraMatrix.MultiplyPoint(Ps[i]);
                Ps[i] = renderCam.projectionMatrix.MultiplyPoint(Ps[i]);
                Ps[i] = (Ps[i] + Vector3.one) / 2;
            }
            foreach (Renderer r in PortalRenderers)
            {
                r.material.SetVector("_Top", new Vector4(Ps[0].x, Ps[0].y, Ps[1].x, Ps[1].y));
                r.material.SetVector("_Bottom", new Vector4(Ps[3].x, Ps[3].y, Ps[2].x, Ps[2].y));
            }
        
            SetObliqueMatrix(renderCam);
        }
    }

    void OnPreRender()
    {
        foreach (Renderer r in PortalRenderers)
        {
            r.material.SetFloat("_Crop", 1f);
            r.material.SetFloat("_Attenuation", Attenuation);
            r.material.SetTexture("_MainTex", renderCam.targetTexture);
        }
    }

    void OnPostRender()
    {
    }
}
