/*===============================================================================
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using System.Collections.Generic;
using UnityEngine;

public class CloudContentManager2 : MonoBehaviour
{

    #region PRIVATE_MEMBER_VARIABLES

    [SerializeField] Transform CloudTarget = null;
    [SerializeField] UnityEngine.UI.Text cloudTargetInfo = null;

    readonly string[] starRatings = { "☆☆☆☆☆", "★☆☆☆☆", "★★☆☆☆", "★★★☆☆", "★★★★☆", "★★★★★" };

    Transform contentManagerParent;

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    public void ShowTargetInfo(bool showInfo)
    {
        // Don't open the target info if upload menu is open.
        // if (CloudUploading.Instance.uploadMenu.activeSelf){
        //     return;
        // }

        Canvas canvas = cloudTargetInfo.GetComponentInParent<Canvas>();

        canvas.enabled = showInfo;
    }

    public void HandleTargetFinderResult(Vuforia.TargetFinder.CloudRecoSearchResult targetSearchResult)
    {
        Debug.Log("<color=blue>HandleTargetFinderResult(): " + targetSearchResult.TargetName + "</color>");
        CloudUploading.currentImageData = targetSearchResult;

        // cloudTargetInfo.text =
        //     "Name: " + targetSearchResult.TargetName +
        //     "\nRating: " + starRatings[targetSearchResult.TrackingRating] +
        //     "\nMetaData: " + ((targetSearchResult.MetaData.Length > 0) ? targetSearchResult.MetaData : "No") +
        //     "\nTarget Id: " + targetSearchResult.UniqueTargetId;

        if (targetSearchResult.MetaData.Length > 0){
            ProfileDataList profileDataList = JsonUtility.FromJson<ProfileDataList>(targetSearchResult.MetaData);

            cloudTargetInfo.text =
                "Name: " + profileDataList.profileDatasArray[0].value +
                "\nAge: " + profileDataList.profileDatasArray[1].value +
                "\nPhone: " + profileDataList.profileDatasArray[2].value +
                "\nAddress: " + profileDataList.profileDatasArray[3].value +
                "\nIC: " + profileDataList.profileDatasArray[4].value +
                "\nOccupation: " + profileDataList.profileDatasArray[5].value +
                "\nBiography: " + profileDataList.profileDatasArray[6].value;
            
        } else {
            Debug.Log("Target lacks MetaData!");
            
            cloudTargetInfo.text =
                "Name: " + targetSearchResult.TargetName +
                "\nRating: " + starRatings[targetSearchResult.TrackingRating] +
                "\nMetaData: " + ((targetSearchResult.MetaData.Length > 0) ? targetSearchResult.MetaData : "No") +
                "\nTarget Id: " + targetSearchResult.UniqueTargetId;
        }
        
    }

    #endregion // PUBLIC_METHODS


    #region // PRIVATE_METHODS

    GameObject GetValuefromDictionary(Dictionary<string, GameObject> dictionary, string key)
    {
        Debug.Log("<color=blue>GetValuefromDictionary() called.</color>");
        if (dictionary == null)
            Debug.Log("dictionary is null");

        if (dictionary.ContainsKey(key))
        {
            Debug.Log("key: " + key);
            GameObject value;
            dictionary.TryGetValue(key, out value);
            Debug.Log("value: " + value.name);
            return value;
        }

        return null;
        //return "Key not found.";
    }

    #endregion // PRIVATE_METHODS
}
