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
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class PostNewTrackableRequest
{
    public string name;
    public float width;
    public string image;
    public string application_metadata;
}

[SerializableAttribute]
public struct ProfileData {
    public string name;
    public string value;
}

[SerializableAttribute]
public struct ProfileDataList {
    public ProfileData[] profileDatasArray;
}

[Serializable]
public class VWSResponse
{
	public string result_code;
	public string transaction_id;
	public string target_id;
	public VWSTargetRecord target_record;
	public string[] similar_targets;
	public string[] results;
	public string status;


	public VWSResponse(string result_code)
	{
		this.result_code = result_code;
	}
}

[Serializable]
public class VWSTargetRecord
{
	public string target_id;
	public string name;
	public float width;
	public int tracking_rating;
	public bool active_flag;
	public string reco_rating;
}
 
public class CloudUploading : CloudTrackableEventHandler
{
    public Button uploadButton, editButton;
    public Button postUploadButton, putEditButton, deleteDeleteButton;

    public Texture2D texture;
    public RawImage rawImage;
    public GameObject uploadMenu, editMenu;
    public GameObject cloudRecognition;
    public Text uploadStatusText;
    public Text openMenuStatusText;

    public InputField nameField;
    public InputField ageField;
    public InputField phoneField;
    public InputField addressField;
    public InputField icField;
    public InputField occupationField;
    public InputField biographyField;

    public CloudContentManager cloudContentManager;
    public UDTEventHandler userDefinedTargetHandler;

    private string jsonData;
    
    private string access_key = "8f20171c732067642e3bad7969663882974d60e2";
    private string secret_key = "52c6967184954ee14049d49796b258f31174f57a";
    private string url = @"https://vws.vuforia.com";//@"<a href="https://vws.vuforia.com";//">https://vws.vuforia.com";</a>
    private string targetName = "Face_1"; // must change when upload another Image Target, avoid same as exist Image on cloud

    private byte[] requestBytesArray;
    private bool forceTrackerDisabledSeconds;

    public static int targetsInCamera;

    public static Vuforia.TargetFinder.CloudRecoSearchResult currentImageData;

    private void Awake(){
        texture = CameraImageAccess.texture;
    
        // By default, the camera loses track of an item during initialization.
        targetsInCamera = 1;

        // Edit menu is only active when a target is active.
        editMenu.SetActive(false);
    }

    public void ToggleUploadMenu()
    {
        // Close upload menu if already open.
        if (uploadMenu.activeSelf){
            uploadMenu.SetActive(false);

            EnableCloudAndTracker(true);

            return;
        }

        // Only post target if there is no other targets in camera.
        if (targetsInCamera > 0){
            Debug.Log("There must be no targets present to upload!");
            openMenuStatusText.text = "There must be no targets present to upload!";

            return;
        }

        // Only post target if camera quality is medium or high.
        if (userDefinedTargetHandler.m_FrameQuality != Vuforia.ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM && 
            userDefinedTargetHandler.m_FrameQuality != Vuforia.ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH){
            Debug.Log("Camera frame quality: " + userDefinedTargetHandler.m_FrameQuality);
            openMenuStatusText.text = "Camera frame must be in medium or high quality!";

            return;
        }

        // Clear input fields.
        SetInputFields(uploadMenu, true);

        texture = CameraImageAccess.GetLatestTexture();
        rawImage.texture = texture;
        rawImage.material.mainTexture = texture;

        uploadMenu.SetActive(true);

        EnableCloudAndTracker(false);
    }

    private void SetInputFields(GameObject whichCanvas, bool clear = true){
        // Get all input fields of canvas.
        InputField[] profileFields = whichCanvas.transform.GetComponentsInChildren<InputField>(false);

        nameField = profileFields[0];
        ageField = profileFields[1];
        phoneField = profileFields[2];
        addressField = profileFields[3];
        icField = profileFields[4];
        occupationField = profileFields[5];
        biographyField = profileFields[6];

        // nameField.text = "Van";
        // ageField.text = "18";
        // phoneField.text = "1";
        // addressField.text = "s";
        // icField.text = "24";
        // occupationField.text = "ssd";
        // biographyField.text = "ssds";

        if (clear){
            nameField.text = "";
            ageField.text = "";
            phoneField.text = "";
            addressField.text = "";
            icField.text = "";
            occupationField.text = "";
            biographyField.text = "";
            return;
        }

        // Fill input fields using current image's metadata.
        if (currentImageData != null && currentImageData.MetaData.Length > 0){
            ProfileDataList profileDataList = JsonUtility.FromJson<ProfileDataList>(currentImageData.MetaData);
            
            nameField.text = profileDataList.profileDatasArray[0].value;
            ageField.text = profileDataList.profileDatasArray[1].value;
            phoneField.text = profileDataList.profileDatasArray[2].value;
            addressField.text = profileDataList.profileDatasArray[3].value;
            icField.text = profileDataList.profileDatasArray[4].value;
            occupationField.text = profileDataList.profileDatasArray[5].value;
            biographyField.text = profileDataList.profileDatasArray[6].value;
        }
    }

    // By the way, if cloud is disabled, vuforia will use targets cached locally.
    private void EnableCloudAndTracker(bool enable){
        if (forceTrackerDisabledSeconds){
            if (enable){
                Debug.Log("Trying to enable cloud and tracker, but it is forced to be disabled.");
            }
            return;
        }

        // Disable/Enable cloud.
        m_CloudRecoBehaviour.CloudRecoEnabled = enable;

        // Enable tracker.
        if (enable){
            Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().Start();

            // Enable the image target builder again. It gets disabled when the object tracker stops. This is
            // used to send current camera frame quality down.
            Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().ImageTargetBuilder.StartScan();
        } else {
            Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().Stop();
        }
    }

    public void ToggleEditMenu()
    {
        // Close upload menu if already open.
        if (editMenu.activeSelf){
            editMenu.SetActive(false);

            // If no targets are active, enable upload button.
            if (targetsInCamera == 0){
                uploadButton.interactable = true;
            }

            // If no targets are active, disable edit button.
            if (targetsInCamera == 0){
                editButton.interactable = false;
            }

            EnableCloudAndTracker(true);

            return;
        }

        // Only edit target if it exists.
        if (targetsInCamera < 1){
            Debug.Log("There must be a target to edit!");
            openMenuStatusText.text = "There must be a target to edit!";

            return;
        }

        // Set current input fields to target profile data.
        SetInputFields(editMenu, false);

        texture = CameraImageAccess.GetLatestTexture();
        rawImage.texture = texture;
        rawImage.material.mainTexture = texture;

        editMenu.SetActive(true);

        EnableCloudAndTracker(false);
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
            uploadStatusText.text = "Validation failed for name field!";
            // isValid = false;
            return false;
        }

        // Matches any pattern with numbers only.
        if (!ValidateString(ageField.text, @"^\d+$")){
            Debug.Log("Validation failed for age field {" + ageField.text + "}");
            uploadStatusText.text = "Validation failed for age field!";
            // isValid = false;
            return false;
        }

        if (!ValidateString(phoneField.text, @"^\d+$")){
            Debug.Log("Validation failed for phone field {" + phoneField.text + "}");
            uploadStatusText.text = "Validation failed for phone field!";
            // isValid = false;
            return false;
        }

        if (!ValidateString(addressField.text)){
            Debug.Log("Validation failed for address field {" + addressField.text + "}");
            uploadStatusText.text = "Validation failed for address field!";
            // isValid = false;
            return false;
        }

        if (!ValidateString(icField.text, @"^\d+$")){
            Debug.Log("Validation failed for ic field {" + icField.text + "}");
            uploadStatusText.text = "Validation failed for ic field!";
            // isValid = false;
            return false;
        }

        if (!ValidateString(occupationField.text)){
            Debug.Log("Validation failed for occupation field {" + occupationField.text + "}");
            uploadStatusText.text = "Validation failed for occupation field!";
            // isValid = false;
            return false;
        }

        if (!ValidateString(biographyField.text)){
            Debug.Log("Validation failed for biography field {" + biographyField.text + "}");
            uploadStatusText.text = "Validation failed for biography field!";
            // isValid = false;
            return false;
        }

        return isValid;
    }

    public void CallPutTarget(){
        if (texture == null){
            Debug.Log("Empty image!");
            return;
        }

        if (VerifyUpload() == false){
            return;
        }

        // Only update target if camera quality is medium or high.
        if (userDefinedTargetHandler.m_FrameQuality != Vuforia.ImageTargetBuilder.FrameQuality.FRAME_QUALITY_MEDIUM && 
            userDefinedTargetHandler.m_FrameQuality != Vuforia.ImageTargetBuilder.FrameQuality.FRAME_QUALITY_HIGH){
            Debug.Log("Camera frame quality: " + userDefinedTargetHandler.m_FrameQuality);
            openMenuStatusText.text = "Camera frame must be in medium or high quality!";

            return;
        }

        deleteDeleteButton.interactable = false;
        putEditButton.interactable = false;
        uploadStatusText.text = "Saving...";

        targetName = nameField.text;

        // Generate list of ProfileData.
        ProfileDataList profileDatas = GenerateDataFromFields();

        // Convert to json to transfer data as metadata
        jsonData = JsonUtility.ToJson(profileDatas);

        // Update image too.
        StartCoroutine(PutUpdateTarget(true));
    }

    public void CallDeleteTarget(){
        deleteDeleteButton.interactable = false;
        putEditButton.interactable = false;
        uploadStatusText.text = "Deleting...";

        StartCoroutine(DeleteTarget());
    }

    private ProfileDataList GenerateDataFromFields(){
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
        phoneData.value = phoneField.text;
        profileDatas.profileDatasArray[2] = phoneData;

        ProfileData addressData = new ProfileData();
        addressData.name = "address";
        addressData.value = addressField.text;
        profileDatas.profileDatasArray[3] = addressData;

        ProfileData icData = new ProfileData();
        icData.name = "ic";
        icData.value = icField.text;
        profileDatas.profileDatasArray[4] = icData;
        
        ProfileData occupationData = new ProfileData();
        occupationData.name = "occupation";
        occupationData.value = occupationField.text;
        profileDatas.profileDatasArray[5] = occupationData;

        ProfileData biographyData = new ProfileData();
        biographyData.name = "biography";
        biographyData.value = biographyField.text;
        profileDatas.profileDatasArray[6] = biographyData;

        return profileDatas;
    }

    public void CallPostTarget(){
        if (texture == null){
            Debug.Log("Empty image!");
            return;
        }

        if (VerifyUpload() == false){
            return;
        }

        postUploadButton.interactable = false;
        uploadStatusText.text = "Uploading...";

        targetName = nameField.text;

        // Generate list of ProfileData.
        ProfileDataList profileDatas = GenerateDataFromFields();

        // Convert to json to transfer data as metadata
        jsonData = JsonUtility.ToJson(profileDatas);
        print(jsonData);

        StartCoroutine (PostNewTarget());
    }

    public void BackToMenu(){
        SceneManager.LoadScene("Main Menu");
    }

    protected override void OnTrackingFound(){
        base.OnTrackingFound();

        // If the upload menu is inactive, hide the open upload button.
        if (!uploadMenu.activeSelf){
            uploadButton.interactable = false;
        }

        if (!editMenu.activeSelf){
            editButton.interactable = true;
        }

        targetsInCamera += 1;
        Debug.Log(string.Format("Targets found: {0}", targetsInCamera));
    }

    protected override void OnTrackingLost(){
        base.OnTrackingLost();

        targetsInCamera -= 1;

        // If the edit menu is inactive.
        if (!editMenu.activeSelf){

            // If no targets are active, enable upload button.
            if (targetsInCamera == 0){
                uploadButton.interactable = true;
            }

            // If no targets are active, disable edit button.
            if (targetsInCamera == 0){
                editButton.interactable = false;
            }
        }

        Debug.Log(string.Format("Targets lost: {0}", targetsInCamera));
    }
    
    // https://library.vuforia.com/articles/Solution/How-To-Use-the-Vuforia-Web-Services-API.htm#How-To-Add-a-Target
    IEnumerator PostNewTarget()
    {
        Debug.Log("CustomMessage: PostNewTarget()");
    
        string requestPath = "/targets";
        string serviceURI = url + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());


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
        model.application_metadata = System.Convert.ToBase64String(metadata);
        
        string requestBody = JsonUtility.ToJson(model);
    
        WWWForm form = new WWWForm ();
    
        var headers = form.headers;
        byte[] rawData = form.data;
        headers["host"]= url;
        headers["date"] = date;
        headers["Content-Type"]= contentType;

        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }
    
        string contentMD5 = sb.ToString();
        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);
    
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
    
        WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(requestBody), headers);
        yield return request;
    
        if (request.error != null)
        {
            Debug.Log("request error: " + request.error);

            string result_code = JsonUtility.FromJson<VWSResponse>(request.text).result_code;
            uploadStatusText.text = "Error: " + result_code;
        }
        else
        {
            Debug.Log("request success");
            Debug.Log("returned data" + request.text);
            uploadStatusText.text = "Uploaded!";

            // Close upload menu.
            ToggleUploadMenu();
        }

        postUploadButton.interactable = true;
    }

    // https://library.vuforia.com/articles/Solution/How-To-Use-the-Vuforia-Web-Services-API.htm#How-To-Update-a-Target
    IEnumerator PutUpdateTarget(bool updateImage = false)
    {
        Debug.Log("CustomMessage: PutUpdateTarget()");
    
        // Setting up query.
        string requestPath = "/targets/" + currentImageData.UniqueTargetId;
        string serviceURI = url + requestPath;
        string httpAction = "PUT";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

        string metadataStr = jsonData;
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);

        // Create new model to prepare for sending.
        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = targetName;
        model.width = 64.0f;
        model.application_metadata = System.Convert.ToBase64String(metadata);

        if (updateImage){
            // Create texture and encode pixels to base64.
            Texture2D tex = new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
            tex.SetPixels(texture.GetPixels());
            tex.Apply();
            byte[] image = tex.EncodeToPNG();

            model.image = System.Convert.ToBase64String(image);
        }

        // Convert model to json.
        string requestBody = JsonUtility.ToJson(model);

        // Create ContentMD5.
        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }
    
        string contentMD5 = sb.ToString();
        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);
    
        // Build signature.
        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);
    
        Debug.Log("<color=green>Signature: "+signature+"</color>");        

        // Build Http Request.
        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(new Uri(serviceURI));

        request.MethodType = BestHTTP.HTTPMethods.Put;
		request.RawData = Encoding.UTF8.GetBytes(requestBody);
		request.AddHeader("Authorization", string.Format("VWS {0}:{1}", access_key, signature));
		request.AddHeader("Content-Type", contentType);
		request.AddHeader("Date", date);
        request.Send();
        
        yield return StartCoroutine(request);
    
        switch(request.State){

            case BestHTTP.HTTPRequestStates.Error:
            
                Debug.Log("request error: " + request.Exception.Message);

                string errorText = request.Exception.Message;
                uploadStatusText.text = "Exception: " + errorText;

                break;
            
            case BestHTTP.HTTPRequestStates.Finished:
            
                // There is an error
                if (request.Response.StatusCode != 200){
                    Debug.Log("request error: " + request.Response.Message);
                    
                    string result_code = JsonUtility.FromJson<VWSResponse>(request.Response.DataAsText).result_code;
                    uploadStatusText.text = "Error: " + result_code;
                } else {
                    Debug.Log("request success");
                    uploadStatusText.text = "Saved!";

                    // Since the image is saved to the cloud, we can change the local copy.
                    currentImageData.MetaData = metadataStr;
                    currentImageData.TargetName = targetName;

                    // Force update of target info.
                    cloudContentManager.HandleTargetFinderResult(currentImageData);

                    // The only issue with this method is we do not know the new tracking rating of the copy.
                    // However, since our version of profiler does not show tracking rating, it should work fine.

                    // Also, if the new image fails the processor on Vuforia's side, it wouldn't make sense to
                    // change the local copy's data. However, it would be more convenient for the user to see
                    // the new updated version. Therefore, when changing the local copy, we are assuming the
                    // new image to be processed successfully.
                    
                    // Close edit menu.
                    ToggleEditMenu();
                }

                break;
        }

        // Enable buttons.
        deleteDeleteButton.interactable = true;
        putEditButton.interactable = true;
    }

    IEnumerator DeleteTarget(){
        Debug.Log("CustomMessage: DeleteTarget()");
    
        // Setting up query.
        string requestPath = "/targets/" + currentImageData.UniqueTargetId;
        string serviceURI = url + requestPath;
        string httpAction = "DELETE";
        string contentType = "";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

        // Create ContentMD5.
        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(""));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }
    
        string contentMD5 = sb.ToString();
        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);
    
        // Build signature.
        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);
    
        Debug.Log("<color=green>Signature: "+signature+"</color>");        

        // Build Http Request.
        BestHTTP.HTTPRequest request = new BestHTTP.HTTPRequest(new Uri(serviceURI));

        request.MethodType = BestHTTP.HTTPMethods.Delete;
		request.RawData = Encoding.UTF8.GetBytes("");
		request.AddHeader("Authorization", string.Format("VWS {0}:{1}", access_key, signature));
		request.AddHeader("Content-Type", contentType);
		request.AddHeader("Date", date);
        request.Send();
        
        yield return StartCoroutine(request);
    
        switch(request.State){

            case BestHTTP.HTTPRequestStates.Error:
            
                Debug.Log("request error: " + request.Exception.Message);

                string errorText = request.Exception.Message;
                uploadStatusText.text = "Exception: " + errorText;

                break;
            
            case BestHTTP.HTTPRequestStates.Finished:
            
                // There is an error
                if (request.Response.StatusCode != 200){
                    Debug.Log("request error: " + request.Response.Message);
                    
                    string result_code = JsonUtility.FromJson<VWSResponse>(request.Response.DataAsText).result_code;
                    uploadStatusText.text = "Error: " + result_code;
                } else {
                    Debug.Log("request success");
                    uploadStatusText.text = "Deleted!";

                    // We disable cloud tracking for x seconds. The reason why we do this is because it takes time for
                    // Vuforia to actually delete the record. If we continue cloud tracking, it would retrack the record.
                    DisableCloudTracking(2f);

                    // To get all the tracked targets. For testing.
                    // IEnumerable<Vuforia.ObjectTarget> obj = Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().GetTargetFinder<Vuforia.ImageTargetFinder>().GetObjectTargets();
                    // IEnumerator myEnum = obj.GetEnumerator();

                    // while(myEnum.MoveNext()){
                    //     print(myEnum.Current);
                    // }

                    // Clear local copy.
                    // This only works for laptop, fails to work on my mobile.
                    Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().GetTargetFinder<Vuforia.ImageTargetFinder>().ClearTrackables(false);
                    
                    // Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().GetTargetFinder<Vuforia.ImageTargetFinder>().Stop();
                    // Vuforia.TrackerManager.Instance.GetTracker<Vuforia.ObjectTracker>().GetTargetFinder<Vuforia.ImageTargetFinder>().StartRecognition();

                    // Close edit menu.
                    ToggleEditMenu();
                }

                break;
        }

        // Enable buttons.
        deleteDeleteButton.interactable = true;
        putEditButton.interactable = true;
    }

    public void DisableCloudTracking(float seconds){
        IEnumerator myEnum = PauseTracking(seconds);
        StartCoroutine(myEnum);
    }

    IEnumerator PauseTracking(float seconds){
        forceTrackerDisabledSeconds = true;
        EnableCloudAndTracker(false);
        yield return new WaitForSecondsRealtime(seconds);
        forceTrackerDisabledSeconds = false;
        EnableCloudAndTracker(true);
    }

    // void PutUpdateTarget()
    // {
    //     Debug.Log("CustomMessage: PutUpdateTarget()");
    
    //     string requestPath = "/targets" + currentImageData.UniqueTargetId;
    //     string serviceURI = url + requestPath;
    //     string httpAction = "PUT";
    //     string contentType = "application/json";
    //     string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());
    
    //     // if your texture2d has RGb24 type, don't need to redraw new texture2d
    //     Texture2D tex = new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
    //     tex.SetPixels(texture.GetPixels());
    //     tex.Apply();
    //     byte[] image = tex.EncodeToPNG();

    //     string metadataStr = jsonData;
    //     byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
    //     PostNewTrackableRequest model = new PostNewTrackableRequest();
    //     model.name = targetName;
    //     model.width = 64.0f; // don't need same as width of texture
    //     model.image = System.Convert.ToBase64String(image);
        
    //     model.application_metadata = System.Convert.ToBase64String(metadata);
    //     string requestBody = JsonUtility.ToJson(model);

    //     VWS.Instance.UpdateTarget(currentImageData.UniqueTargetId, currentImageData.TargetName, 64, tex, true, metadataStr, resp => {
    //         Debug.Log(resp.result_code);
    //         if (resp.result_code == "Success"){
    //             uploadStatusText.text = "Saved!";
    //             ToggleEditMenu();
    //         } else {
    //             uploadStatusText.text = resp.result_code;
    //         }
    //     });

    //     postUploadButton.interactable = true;
    // }
}
