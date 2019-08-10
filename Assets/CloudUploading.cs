using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
// using Newtonsoft.Json.Linq;
using System.Net;
using System.Linq;
using System.Configuration;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class PostNewTrackableRequest
{
    public string name;
    public float width;
    public string image;
    public string application_metadata;
}

[System.SerializableAttribute]
public struct ProfileData {
    public string name;
    public string value;
}

[System.SerializableAttribute]
public struct ProfileDataList {
    public ProfileData[] profileDatasArray;
}
 
public class CloudUploading : CloudTrackableEventHandler
{
    public Button uploadButton;
    public Button postUploadButton;

    public Texture2D texture;
    public RawImage rawImage;
    public GameObject uploadMenu;
    public Text uploadStatusText;
    public Text openMenuStatusText;

    public InputField nameField;
    public InputField ageField;
    public InputField phoneField;
    public InputField addressField;
    public InputField icField;
    public InputField occupationField;
    public InputField biographyField;

    private string jsonData;
    
    private string access_key = "8f20171c732067642e3bad7969663882974d60e2";
    private string secret_key = "52c6967184954ee14049d49796b258f31174f57a";
    private string url = @"https://vws.vuforia.com";//@"<a href="https://vws.vuforia.com";//">https://vws.vuforia.com";</a>
    private string targetName = "Face_1"; // must change when upload another Image Target, avoid same as exist Image on cloud

    private byte[] requestBytesArray;

    public static int targetsInCamera;

    private void Awake(){
        texture = CameraImageAccess.texture;
        
        // By default, the camera loses track of an item during initialization.
        targetsInCamera = 1;
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

    public void OpenUploadMenu()
    {
        // Close upload menu if already open.
        if (uploadMenu.activeSelf){
            uploadMenu.SetActive(false);
            return;
        }

        // Only post target if there is no other targets in camera.
        if (targetsInCamera > 0){
            Debug.Log("There must be no targets present to upload!");
            openMenuStatusText.text = "There must be no targets present to upload!";

            return;
        }

        if (true){
            nameField.text = "John Cena";
            ageField.text = "18";
            phoneField.text = "92859238";
            addressField.text = "Boon Lay";
            icField.text = "19";
            occupationField.text = "Dungeon Master";
            biographyField.text = "I'm hired for people to fulfill their fantasies, their deep dark fantasies. I was gonna be a movie star, you know, modeling and acting. After a hundred and two additions and small parts I decided y'know I had enough, Then I got in to Escort world. The client requests contain a lot of fetishes, so I just decided to go y'know... full Master, and change my entire house into a dungeon, uh... Dungeon Master. Now with a full dungeon in my house and It's going really well. Fisting is 300 bucks, and usually the guy is pretty much hard on pop to get really relaxed y'know and I have this long latex glove that goes all the way up to my armpit and then I put on a surgical latex glove up to my wrist and, just lube it up, and it's a long process y'know to get your...get your whole arm up there, but it's an intense feeling for the other person. I think for myself too, you go in places that even though it's physical with your hand but for some reason it's also more emotional it's more psychological too, and we both get you know to the same place it's really strange at the same time and I find sessions like that really exhausting. I don't know I feel kinda naked because I am looking at myself for the first time, well not myself but this aspect of my life for the first time and it's been harsh... three to five years already? I never thought about it... Kinda sad I feel kinda sad right now, I don't know why";
        }

        texture = CameraImageAccess.GetLatestTexture();
        rawImage.texture = texture;
        rawImage.material.mainTexture = texture;

        uploadMenu.SetActive(true);
    }

    // Default pattern: Starts with any letter.
    private bool ValidateString(string input, string pattern = @"^[A-Za-z]"){
        Regex regex = new Regex(pattern);

        return regex.IsMatch(input);
    }

    private bool VerifyUpload(){
        bool isValid = true;

        if (!ValidateString(nameField.text)){
            Debug.Log("Validation failed for name field {" + nameField.text + "}");
            isValid = false;
        }

        // Matches any pattern with numbers only.
        if (!ValidateString(ageField.text, @"^\d+$")){
            Debug.Log("Validation failed for age field {" + ageField.text + "}");
            isValid = false;
        }

        if (!ValidateString(phoneField.text, @"^\d+$")){
            Debug.Log("Validation failed for phone field {" + phoneField.text + "}");
            isValid = false;
        }

        if (!ValidateString(addressField.text)){
            Debug.Log("Validation failed for address field {" + addressField.text + "}");
            isValid = false;
        }

        if (!ValidateString(icField.text, @"^\d+$")){
            Debug.Log("Validation failed for ic field {" + icField.text + "}");
            isValid = false;
        }

        if (!ValidateString(occupationField.text)){
            Debug.Log("Validation failed for occupation field {" + occupationField.text + "}");
            isValid = false;
        }

        if (!ValidateString(biographyField.text)){
            Debug.Log("Validation failed for biography field {" + biographyField.text + "}");
            isValid = false;
        }

        return isValid;
    }

    public void CallPostTarget(){
        if (texture == null){
            Debug.Log("Cannot post empty image!");
            return;
        }

        if (VerifyUpload() == false){
            Debug.Log("Fields mandatory!");
            return;
        }

        postUploadButton.interactable = false;
        uploadStatusText.text = "Uploading...";

        targetName = nameField.text;

        // Generate list of ProfileData.
        ProfileDataList profileDatas = new ProfileDataList{
            profileDatasArray = new ProfileData[7]
        };

        // Initialize variables for list.
        ProfileData nameData = new ProfileData();
        nameData.name = "name";
        nameData.value = nameField.text;
        profileDatas.profileDatasArray[0] = nameData;

        ProfileData ageData = new ProfileData();
        ageData.name = "age";
        ageData.value = ageField.text;
        profileDatas.profileDatasArray[1] = ageData;

        ProfileData phoneData = new ProfileData();
        phoneData.name = "phone";
        phoneData.value = ageField.text;
        profileDatas.profileDatasArray[2] = phoneData;

        ProfileData addressData = new ProfileData();
        addressData.name = "address";
        addressData.value = ageField.text;
        profileDatas.profileDatasArray[3] = addressData;

        ProfileData icData = new ProfileData();
        icData.name = "ic";
        icData.value = ageField.text;
        profileDatas.profileDatasArray[4] = icData;
        
        ProfileData occupationData = new ProfileData();
        occupationData.name = "occupation";
        occupationData.value = ageField.text;
        profileDatas.profileDatasArray[5] = occupationData;

        ProfileData biographyData = new ProfileData();
        biographyData.name = "biography";
        biographyData.value = ageField.text;
        profileDatas.profileDatasArray[6] = biographyData;

        jsonData = JsonUtility.ToJson(profileDatas);
        print(jsonData);

        StartCoroutine (PostNewTarget());
    }

    protected override void OnTrackingFound(){
        base.OnTrackingFound();

        uploadButton.interactable = false;
        targetsInCamera += 1;
        print(string.Format("Targets found: {0}", targetsInCamera));
    }

    protected override void OnTrackingLost(){
        base.OnTrackingLost();

        targetsInCamera -= 1;
        if (targetsInCamera == 0){
            uploadButton.interactable = true;
        }

        print(string.Format("Targets lost: {0}", targetsInCamera));
    }
    
    IEnumerator PostNewTarget()
    {
        print("CustomMessage: PostNewTarget()");
    
        string requestPath = "/targets";
        string serviceURI = url + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

        // texture = RTImage(Camera.main);
        // texture = CameraImageAccess.texture;
    
        // if your texture2d has RGb24 type, don't need to redraw new texture2d
        Texture2D tex = new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
        tex.SetPixels(texture.GetPixels());
        tex.Apply();
        byte[] image = tex.EncodeToPNG();
    

        string metadataStr = jsonData;
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = targetName;
        model.width = 64.0f; // don't need same as width of texture
        model.image = System.Convert.ToBase64String(image);
        // model.image = "iVBORw0KGgoAAAANSUhEUgAAAoAAAAHgCAIAAAC6s0uzAAATWElEQVR4Ae3VwQkAIAwEQbX/niM24X4mDRwMgd0zsxwBAgQIECDwV+D8nbNGgAABAgQIPAEB9gcECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBAQYD9AgAABAgQCAQEO0E0SIECAAAEB9gMECBAgQCAQEOAA3SQBAgQIEBBgP0CAAAECBAIBAQ7QTRIgQIAAAQH2AwQIECBAIBAQ4ADdJAECBAgQEGA/QIAAAQIEAgEBDtBNEiBAgAABAfYDBAgQIEAgEBDgAN0kAQIECBC4/agGvRypoyYAAAAASUVORK5CYII=";

        model.application_metadata = System.Convert.ToBase64String(metadata);
        //string requestBody = JsonWriter.Serialize(model);
        string requestBody = JsonUtility.ToJson(model);

        // print(requestBody);
    
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

        // print(stringToSign);
    
        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);
    
        headers["Authorization"] = string.Format("VWS {0}:{1}", access_key, signature);
    
        Debug.Log("<color=green>Signature: "+signature+"</color>");

        foreach(System.Collections.Generic.KeyValuePair<string, string> kvp in headers){
            print(string.Format("{0}: {1}", kvp.Key, kvp.Value));
        }

        // print(headers);
        // yield break;
    
        WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(requestBody), headers);
        yield return request;
    
        if (request.error != null)
        {
            Debug.Log("request error: " + request.error);

            string errorText = request.error == "403 Forbidden" ? "Name already exists" : request.error;
            uploadStatusText.text = errorText;
        }
        else
        {
            Debug.Log("request success");
            Debug.Log("returned data" + request.text);
            uploadStatusText.text = "Uploaded!";
        }

        postUploadButton.interactable = true;
    }
}
