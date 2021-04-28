using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Slider interSpeed;
    public TMPro.TMP_Dropdown interMode;
    public TMPro.TMP_Dropdown interTheme;

    private void Start()    //Load PlayerPref settings
    {
        
    }

    public void PlayGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void updateInterSettings()
    {
        float speed = interSpeed.value;
        string mode = interMode.options[interMode.value].text;
        string theme = interTheme.options[interTheme.value].text;

        Debug.Log("Speed: " + speed + ", Mode: " + mode + ", Theme: " + theme);

        UnityEngine.PlayerPrefs.SetFloat(PlayerPrefKeys.interSpeed, speed);     //returns 0-1, so add one and range is 1-2
        UnityEngine.PlayerPrefs.SetString(PlayerPrefKeys.interMode, mode);      //Get the mode value, and convert to string
        UnityEngine.PlayerPrefs.SetString(PlayerPrefKeys.interTheme, theme);    //Get the theme value, and convert to string
    }
}
