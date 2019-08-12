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
 
    private void Awake(){
        // OnPause(true);
        // OnPause(false);
        // CameraDevice.Instance.Deinit();
        // CameraDevice.Instance.Init();
    }

    void Start()
    {

        if (Instance == null){
            Instance = this;
            Debug.Log("CustomMessage: Start CameraImageAccess");
        } else {
            Debug.Log("CustomMessage: Destroy CameraImageAccess");
            Destroy(gameObject);

            return;
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

        // Remove the pixel format (in case it was previously set); 
        // note that we are passing "false" here.
        // OnPause(true);
        // OnPause(false);
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

    public static Texture2D RotateTexture(Texture2D t)
    {
        Texture2D newTexture = new Texture2D(t.height, t.width, t.format, false);

        for(int i=0; i<t.width; i++)
        {
            for(int j=0; j<t.height; j++)
            {
                newTexture.SetPixel(j, i, t.GetPixel(t.width-i, j));
            }
        }
        newTexture.Apply();
        return newTexture;
    }

    public static Texture2D FlipTexture(Texture2D t)
    {
        Texture2D newTexture = new Texture2D(t.height, t.width, t.format, false);

        Color[] originalPixels = t.GetPixels();
        Color[] newPixels = new Color[originalPixels.Length];

        int width = t.width;
        int rows = t.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                newPixels[x + y * width] = originalPixels[x + (rows - y -1) * width];
            }
        }

        newTexture.SetPixels(newPixels);
        newTexture.Apply();
        return newTexture;
    }

    public static void FlipTexture(Texture2D original, bool vertically)
    {
        var originalPixels = original.GetPixels();

        Color[] newPixels = new Color[originalPixels.Length];

        int width = original.width;
        int rows = original.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (vertically){
                    newPixels[x + y * width] = originalPixels[x + (rows - y -1) * width];
                } else {
                    newPixels[x + y * width] = originalPixels[width - 1 - x + y * width];
                }
            }
        }

        original.SetPixels(newPixels);
        original.Apply();
    }

    public static Texture2D GetLatestTexture(){
        Instance.RegisterFormat();

        Vuforia.Image image = CameraDevice.Instance.GetCameraImage(Instance.mPixelFormat);
        Debug.Log("CustomMessage: Pixel format: " + Instance.mPixelFormat);
        Debug.Log("CustomMessage: Pixel count: " + image.Pixels.Length);
 
        if (image != null)
        {
            byte[] pixels = image.Pixels;
            
            texture.Resize(image.Width, image.Height);
            texture.LoadRawTextureData(pixels);
            texture.Apply();

            texture = RotateTexture(texture);
            FlipTexture(texture, false);
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