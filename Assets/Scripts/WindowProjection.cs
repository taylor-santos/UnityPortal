using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowProjection : MonoBehaviour
{
    public GameObject Quad;
    public float farPlane;
    public Vector3 X;
    public Vector3 dX;
    public Vector3 dY;
    private Matrix4x4 oldProj;
    public Matrix4x4 proj;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        oldProj = cam.projectionMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Quad.transform, Quad.transform.up);

        X = Quad.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
        dX = Quad.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));
        dY = Quad.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));

        X = cam.worldToCameraMatrix.MultiplyPoint(X);
        dX = cam.worldToCameraMatrix.MultiplyPoint(dX);
        dY = cam.worldToCameraMatrix.MultiplyPoint(dY);

        dX -= X;
        dY -= X;

        X *= 0.999f;
        dX *= 0.999f;
        dY *= 0.999f;

        if (farPlane <= 1)
        {
            farPlane = 2;
        }

        Matrix4x4 p = new Matrix4x4();
        float k = farPlane;

        p[0, 0] = 1;
        p[0, 1] = ((-dY[2]) * (dX[0] + 2 * X[0]) + dY[0] * (dX[2] + 2 * X[2])) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
        p[0, 2] = (dY[1] * (dX[0] + 2 * X[0]) - dY[0] * (dX[1] + 2 * X[1])) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
        p[1, 0] = (dX[2] * (dY[1] + 2 * X[1]) - dX[1] * (dY[2] + 2 * X[2])) / ((-dY[2]) * (dX[1] + 2 * X[1]) + dY[1] * (dX[2] + 2 * X[2]));
        p[1, 1] = ((-dX[2]) * (dY[0] + 2 * X[0]) + dX[0] * (dY[2] + 2 * X[2])) / ((-dY[2]) * (dX[1] + 2 * X[1]) + dY[1] * (dX[2] + 2 * X[2]));
        p[1, 2] = ((-dX[1]) * (dY[0] + 2 * X[0]) + dX[0] * (dY[1] + 2 * X[1])) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
        p[2, 0] = - (((1 + k) * (dX[2] * dY[1] - dX[1] * dY[2])) / ((-1 + k) * ((-dY[2]) * (dX[1] + 2 * X[1]) + dY[1] * (dX[2] + 2 * X[2]))));
        p[2, 1] = ((1 + k) * (dX[2] * dY[0] - dX[0] * dY[2])) / ((-1 + k) * ((-dY[2]) * (dX[1] + 2 * X[1]) + dY[1] * (dX[2] + 2 * X[2])));
        p[2, 2] = - (((1 + k) * (dX[1] * dY[0] - dX[0] * dY[1])) / ((-1 + k) * ((-dY[2]) * (dX[1] + 2 * X[1]) + dY[1] * (dX[2] + 2 * X[2]))));
        p[2, 3] = (2 * k * (dX[2] * (dY[1] * X[0] - dY[0] * X[1]) + dX[1] * ((-dY[2]) * X[0] + dY[0] * X[2]) + dX[0] * (dY[2] * X[1] - dY[1] * X[2]))) / ((-1 + k) * ((-dY[2]) * (dX[1] + 2 * X[1]) + dY[1] * (dX[2] + 2 * X[2])));
        p[3, 0] = (dX[2] * dY[1] - dX[1] * dY[2]) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
        p[3, 1] = ((-dX[2]) * dY[0] + dX[0] * dY[2]) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));
        p[3, 2] = (dX[1] * dY[0] - dX[0] * dY[1]) / (dY[2] * (dX[1] + 2 * X[1]) - dY[1] * (dX[2] + 2 * X[2]));


        proj = p;

        cam.projectionMatrix = proj;
        List<Vector3> frustum = new List<Vector3>();
        frustum.Add(new Vector3(-1, -1, -1));
        frustum.Add(new Vector3( 1, -1, -1));
        frustum.Add(new Vector3( 1,  1, -1));
        frustum.Add(new Vector3(-1,  1, -1));
        frustum.Add(new Vector3(-1, -1,  1f));
        frustum.Add(new Vector3( 1, -1,  1f));
        frustum.Add(new Vector3( 1,  1,  1f));
        frustum.Add(new Vector3(-1,  1,  1f));

        for (int i = 0; i < frustum.Count; i++)
        {
            frustum[i] = proj.inverse.MultiplyPoint(frustum[i]);
            frustum[i] = cam.cameraToWorldMatrix.MultiplyPoint(frustum[i]);
        }
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(frustum[i], frustum[(i + 1) % 4], Color.green);
            Debug.DrawLine(frustum[i + 4], frustum[(i + 1) % 4 + 4]);
            Debug.DrawLine(frustum[i], frustum[i + 4]);
        }
        Shader.SetGlobalMatrix("_projection", proj);
    }

    void OnPreRender()
    {
    }

    void OnPostRender()
    {
    }
}
