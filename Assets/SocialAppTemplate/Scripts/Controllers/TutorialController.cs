using System.Collections;
using System.Collections.Generic;
using SocialApp;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public GameObject pannello;
    
    public void SkipTutorial()
    {
        AppSettings.TutorialMode = FasiTutorial.Nulla;
        AzioneCompletata();
    }

    public void AzioneCompletata()
    {
        if (AppSettings.TutorialMode == FasiTutorial.Nulla)
        {
            PlayerPrefs.SetInt("tutorial", 0);
            pannello.SetActive(false);
            return;
        }
        pannello.SetActive(true);
        pannello.transform.GetChild((int)AppSettings.TutorialMode).gameObject.SetActive(true);
        if (AppSettings.TutorialMode != 0)
            pannello.transform.GetChild((int)AppSettings.TutorialMode - 1).gameObject.SetActive(false);
        AppSettings.TutorialMode = (FasiTutorial)((int)AppSettings.TutorialMode + 1);
    }
}