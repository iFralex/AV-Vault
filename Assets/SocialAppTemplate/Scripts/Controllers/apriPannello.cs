using System.Collections;
using System.Collections.Generic;
using SocialApp;
using UnityEngine;

public class apriPannello : MonoBehaviour
{/*
    public bool premuto, chiudi;
    public RectTransform pannello1;
    public enum Tipi { feedProfilo, commenti }
    public Tipi tipo;

    public void Premi(bool b) => premuto = b;

    void Trascina(float a, float b, float t)
    {
        pannello1.anchoredPosition = new Vector2(pannello1.anchoredPosition.x, Mathf.Lerp(a, b, t));
    }

    public void TrascinaVersoLalto()
    {
        float n = ((Screen.height * Mathf.Abs((9f / 16f) - (Screen.width / (float)Screen.height)) / (Screen.width / (float)Screen.height)) / 2);
        //Trascina(-Screen.height - 2 * n, 0, ((Input.touchCount > 0 ? Input.GetTouch(0).position.y : Input.mousePosition.y) + n) / Screen.height);
        Trascina(-1356, 0, ((Input.touchCount > 0 ? Input.GetTouch(0).position.y : Input.mousePosition.y) ) / Screen.height);
    }

    public void LasciaTrascinaVersoLalto()
    {
        float n = ((Screen.height * Mathf.Abs((9f / 16f) - (Screen.width / (float)Screen.height)) / (Screen.width / (float)Screen.height)) / 2);
        if ((Input.touchCount > 0 ? Input.GetTouch(0).position.y : Input.mousePosition.y) > Screen.width * 16 / 9 / 2)
        {
            StartCoroutine(FinisciTrascinamento(-Screen.height - 2 * n, 0, ((Input.touchCount > 0 ? Input.GetTouch(0).position.y : Input.mousePosition.y) + n) / Screen.height));
            if ((int)FasiTutorial.ApriCommenti + 1 == (int)AppSettings.TutorialMode || (int)FasiTutorial.ApriMieiPosts + 1 == (int)AppSettings.TutorialMode)
                AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
        }
        else
        {
            StartCoroutine(FinisciTrascinamento(-Screen.height - 2 * n, (Input.touchCount > 0 ? Input.GetTouch(0).position.y : Input.mousePosition.y) - (Screen.width * 16 / 9 / 2), ((Input.touchCount > 0 ? Input.GetTouch(0).position.y : Input.mousePosition.y) + n) / Screen.height, -1, true));
            if ((int)FasiTutorial.ChiudiCommenti + 1 == (int)AppSettings.TutorialMode || (int)FasiTutorial.ChiudiMieiPosts + 1 == (int)AppSettings.TutorialMode)
                AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
        }
    }

    IEnumerator FinisciTrascinamento(float a, float b, float t, int verso = 1, bool chiudi = false)
    {
        for (float i = t; verso == 1 ? i < 1 : i > 0;)
        {
            i += verso == 1 ? .05f : -.05f;
            Trascina(a, b, i);
            yield return new WaitForSeconds(.01f);
        }
        if (chiudi)
        {
            pannello1.parent.gameObject.SetActive(false);
            AppManager.VIEW_CONTROLLER.ShowNavigationPanel();
        }
        else
        {
            AppManager.VIEW_CONTROLLER.HideNavigationPanel();
            pannello1.parent.gameObject.SetActive(true);
            if (tipo == Tipi.commenti)
            {
                pannello1.GetChild(0).GetChild(2).GetChild(AppSettings.guest ? 1 : 0).gameObject.SetActive(true);
                pannello1.GetChild(0).GetChild(2).GetChild(!AppSettings.guest ? 1 : 0).gameObject.SetActive(false);
            }
        }
    }*/
}