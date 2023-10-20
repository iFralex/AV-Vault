using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostControll : MonoBehaviour
{
    public RectTransform finestraPost, finestra;
    public Button postBt;
    public List<RectTransform> seziioniPost;
    public List<float> altezze;
    public SocialApp.FeedDadaUoloader dataLoader;
    int selez = -1;

    public void ApriSezione(int n)
    {
        postBt.onClick.RemoveAllListeners();
        if (selez == n)
            n = 100;
        else if (n == 0)
            postBt.onClick.AddListener(() => dataLoader.PostSimpleText());
        else
            postBt.onClick.AddListener(() => seziioniPost[n].transform.GetChild(5).GetComponent<SocialApp.FeedPreviewController>().StartPost());
        postBt.onClick.AddListener(() => Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection));

        float alt = 130.75f;
        for (int i = 0; i < 3; i++)
            if (n == i)
            {
                alt += altezze[n + 1] + 25;
                StartCoroutine(Transizione(seziioniPost[n].sizeDelta.y, altezze[n + 1], seziioniPost[n]));
                seziioniPost[i].transform.GetChild(2).GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, 180));
            }
            else if (Mathf.Round(seziioniPost[i].position.y) != Mathf.Round(altezze[0]))
            {
                alt += altezze[0] + 25;
                StartCoroutine(Transizione(seziioniPost[i].sizeDelta.y, altezze[0], seziioniPost[i]));
                seziioniPost[i].transform.GetChild(2).GetComponent<RectTransform>().rotation = Quaternion.Euler(Vector3.zero);
                seziioniPost[i].GetComponentInChildren<InputField>().text = "";
                if (seziioniPost[i].transform.childCount > 4)
                    seziioniPost[i].transform.GetChild(5).gameObject.SetActive(false);
            }
        StartCoroutine(Transizione(finestraPost.sizeDelta.y, alt, finestraPost));
        selez = n;
        postBt.interactable = false;
    }

    IEnumerator Transizione(float a, float b, RectTransform ob)
    {
        for (float i = 0; i < 1.1f; i += .1f)
        {
            ob.sizeDelta = new Vector3(ob.sizeDelta.x, Mathf.Lerp(a, b, i), 0);
            yield return new WaitForSeconds(.005f);
        }
    }

    public void Messaggio(string s) => postBt.interactable = s != System.String.Empty;
    
}