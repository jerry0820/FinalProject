using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void SetQuaility(){
        SetScreenRes();
    }
    void SetScreenRes(){
        string index = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        switch (index){
            case "0":
                Screen.SetResolution(1152,648,true);
                break;
            case "1":
                Screen.SetResolution(1280,768,true);
                break;
            case "2":
                Screen.SetResolution(1360,796,true);
                break;
            case "3":
                Screen.SetResolution(1980,1080,true);
                break;
        }
    }
}
