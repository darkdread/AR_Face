using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject creditsScreen;
    public GameObject instructionsScreen;

    public void StartApp(){
        SceneManager.LoadScene("Analyzer");
    }

    public void ShowMainMenu(){
        creditsScreen.SetActive(false);
        instructionsScreen.SetActive(false);
    }

    public void ShowCredits(){
        creditsScreen.SetActive(true);
    }
    
    public void ShowInstructions(){
        instructionsScreen.SetActive(true);
    }

    public void QuitApp(){
        Application.Quit();
    }
}
