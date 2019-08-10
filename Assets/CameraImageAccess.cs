using UnityEngine;
using UnityEngine.UI;
using System.Collections;
 
using Vuforia;

public class CameraImageAccess : MonoBehaviour
{
 
    #region PRIVATE_MEMBERS
 
    private Vuforia.PIXEL_FORMAT mPixelFormat = Vuforia.PIXEL_FORMAT.UNKNOWN_FORMAT;
 
    private bool mAccessCameraImage = true;
    private bool mFormatRegistered = false;
 
    #endregion // PRIVATE_MEMBERS

    public static CameraImageAccess Instance;
    public RawImage rawImage;
    public static Texture2D texture;
 
    #region MONOBEHAVIOUR_METHODS
 
    void Start()
    {

        if (Instance == null){
            Instance = this;
        } else {
            Destroy(gameObject);
        }
 
        #if UNITY_EDITOR
        mPixelFormat = Vuforia.PIXEL_FORMAT.GRAYSCALE; // Need Grayscale for Editor
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.Alpha8, false);
        #else
        mPixelFormat = Vuforia.PIXEL_FORMAT.RGB888; // Use RGB888 for mobile
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        #endif
 
        // Register Vuforia life-cycle callbacks:
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPause);
 
    }
 
    #endregion // MONOBEHAVIOUR_METHODS
 
    #region PRIVATE_METHODS
 
    void OnVuforiaStarted()
    {
 
        // Try register camera image format
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
 
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError(
                "\nFailed to register pixel format: " + mPixelFormat.ToString() +
                "\nThe format may be unsupported by your device." +
                "\nConsider using a different pixel format.\n");
 
            mFormatRegistered = false;
        }
 
    }

    public static Texture2D GetLatestTexture(){
        Vuforia.Image image = CameraDevice.Instance.GetCameraImage(Instance.mPixelFormat);
 
        if (image != null)
        {

            byte[] pixels = image.Pixels;
            texture.Resize(image.Width, image.Height);
            texture.LoadRawTextureData(pixels);
            texture.Apply();
            // rawImage.texture = texture;
            // rawImage.material.mainTexture = texture;

            return texture;

        }

        return null;
    }
 
    /// <summary>
    /// Called each time the Vuforia state is updated
    /// </summary>
    void OnTrackablesUpdated()
    {
        if (mFormatRegistered)
        {
            if (mAccessCameraImage)
            {
                // Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);
 
                // if (image != null)
                // {
                //     Debug.Log(
                //         "\nImage Format: " + image.PixelFormat +
                //         "\nImage Size:   " + image.Width + "x" + image.Height +
                //         "\nBuffer Size:  " + image.BufferWidth + "x" + image.BufferHeight +
                //         "\nImage Stride: " + image.Stride + "\n"
                //     );

                //     byte[] pixels = image.Pixels;
                //     texture.Resize(image.Width, image.Height);
                //     texture.LoadRawTextureData(pixels);
                //     texture.Apply();
                //     rawImage.texture = texture;
                //     rawImage.material.mainTexture = texture;
 
                //     if (pixels != null && pixels.Length > 0)
                //     {
                //         Debug.Log(
                //             "\nImage pixels: " +
                //             pixels[0] + ", " +
                //             pixels[1] + ", " +
                //             pixels[2] + ", ...\n"
                //         );
                //     }

                // }
            }
        }
    }
 
    /// <summary>
    /// Called when app is paused / resumed
    /// </summary>
    void OnPause(bool paused)
    {
        if (paused)
        {
            Debug.Log("App was paused");
            UnregisterFormat();
        }
        else
        {
            Debug.Log("App was resumed");
            RegisterFormat();
        }
    }
 
    /// <summary>
    /// Register the camera pixel format
    /// </summary>
    void RegisterFormat()
    {
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = false;
        }
    }
 
    /// <summary>
    /// Unregister the camera pixel format (e.g. call this when app is paused)
    /// </summary>
    void UnregisterFormat()
    {
        Debug.Log("Unregistering camera pixel format " + mPixelFormat.ToString());
        CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
        mFormatRegistered = false;
    }
 
    #endregion //PRIVATE_METHODS
}