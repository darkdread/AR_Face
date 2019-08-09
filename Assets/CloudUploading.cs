﻿using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
// using Newtonsoft.Json.Linq;
using System.Net;
using System.Linq;
using System.Configuration;

public class PostNewTrackableRequest
{
    public string name;
    public float width;
    public string image;
    public string application_metadata;
}
 
public class CloudUploading : MonoBehaviour
{
 
    public Texture2D texture;
    
    private string access_key = "8f20171c732067642e3bad7969663882974d60e2";
    private string secret_key = "52c6967184954ee14049d49796b258f31174f57a";
    private string url = @"http://vws.vuforia.com";//@"<a href="https://vws.vuforia.com";//">https://vws.vuforia.com";</a>
    private string targetName = "Face_7"; // must change when upload another Image Target, avoid same as exist Image on cloud

    private byte[] requestBytesArray;

    private void Awake(){
        texture = CameraImageAccess.texture;
    }

    // Take a "screenshot" of a camera's Render Texture.
    private Texture2D RTImage(Camera camera)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image;
    }

    public void CallPostTarget()
    {
        StartCoroutine (PostNewTarget());
    }
    
    IEnumerator PostNewTarget()
    {
        print("CustomMessage: PostNewTarget()");
        
        string requestPath = "/targets";
        string serviceURI = url + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());
    
        Debug.Log(date);

        // texture = RTImage(Camera.main);
        texture = CameraImageAccess.texture;
    
        // if your texture2d has RGb24 type, don't need to redraw new texture2d
        Texture2D tex = new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
        tex.SetPixels(texture.GetPixels());
        tex.Apply();
        byte[] image = tex.EncodeToPNG();
    

        string metadataStr = "Vuforia metadata"; //May use for key,name...in game
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = targetName;
        model.width = 64.0f; // don't need same as width of texture
        // model.image = System.Convert.ToBase64String(image);
        model.image = "iVBORw0KGgoAAAANSUhEUgAAAoAAAAHgCAIAAAC6s0uzAAATWElEQVR4Ae3VwQkAIAwEQbX/niM24X4mDRwMgd0zsxwBAgQIECDwV+D8nbNGgAABAgQIPAEB9gcECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBC4/agGvRypoyYAAAAASUVORK5CYII=";
    
        model.application_metadata = System.Convert.ToBase64String(metadata);
        //string requestBody = JsonWriter.Serialize(model);
        string requestBody = JsonUtility.ToJson(model);

        print(requestBody);
    
        WWWForm form = new WWWForm ();
    
        var headers = form.headers;
        byte[] rawData = form.data;
        headers["host"]= url;
        headers["date"] = date;
        headers["Content-Type"]= contentType;
    
        HttpWebRequest httpWReq = (HttpWebRequest)HttpWebRequest.Create(serviceURI);
    
        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }
    
        string contentMD5 = sb.ToString();
    
        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);

        // string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}", httpAction, contentMD5, contentType, requestPath);

        print(stringToSign);
    
        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        // string signature = System.Convert.ToBase64String(sha1Hash);
        string signature = System.Convert.ToBase64String(sha1Hash);
    
        headers["Authorization"] = string.Format("VWS {0}:{1}", access_key, signature);
    
        Debug.Log("<color=green>Signature: "+signature+"</color>");

        foreach(System.Collections.Generic.KeyValuePair<string, string> kvp in headers){
            print(string.Format("{0}: {1}", kvp.Key, kvp.Value));
        }

        print(headers);
        // yield break;
    
        WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(requestBody), headers);
        yield return request;
    
        if (request.error != null)
        {
            Debug.Log("request error: " + request.error);
        }
        else
        {
            Debug.Log("request success");
            Debug.Log("returned data" + request.text);
        }
    }
}
