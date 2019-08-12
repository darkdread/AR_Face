using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceScan : MonoBehaviour
{

    public float SCAN_DURATION = 4; //seconds
    public float mTime;
    public bool mMovingDown = true;

    public Camera m_Camera;
    
    // Update is called once per frame
    void Update()
    {
        float u = mTime / SCAN_DURATION;
        mTime += Time.deltaTime;
        if (u > 1)
        {
            // invert direction
            mMovingDown = !mMovingDown;
            u = 0;
            mTime = 0;
        }

        // Get the main camera
        float viewAspect = m_Camera.pixelWidth / (float)m_Camera.pixelHeight;
        float fovY = Mathf.Deg2Rad * m_Camera.fieldOfView;
        float depth = 1.02f * m_Camera.nearClipPlane;
        float viewHeight = 2 * depth * Mathf.Tan(0.5f * fovY);
        float viewWidth = viewHeight * viewAspect;

        // Position the mesh
        float y = -0.5f * viewHeight + u * viewHeight;
        if (mMovingDown)
        {
            y *= -1;
        }

        transform.localPosition = new Vector3(0, y, depth);

        // Scale the quad mesh to fill the camera view
        float scaleX = 1.02f * viewWidth;
        float scaleY = scaleX / 32;
        transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
    }
}
