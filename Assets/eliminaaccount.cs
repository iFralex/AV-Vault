using System.Collections;
using System.Collections.Generic;
using SocialApp;
using UnityEngine;
using UnityEngine.UI;

public class eliminaaccount : MonoBehaviour
{
    public GameObject prefab;
    public Button bt;
    public InputField inFi;
    Transform lista;

    void OnEnable()
    {
        AppManager.FIREBASE_CONTROLLER.GetAccountNamesDaEliminare(OnMessagesLoaded);
    }

    public void OnMessagesLoaded(List<string> s, List<Firebase.Database.DatabaseReference> re)
    {
        if (prefab == null)
        {
            prefab = lista.GetChild(0).gameObject;
            for (int o = 1; o < lista.childCount; o++)
                Destroy(lista.GetChild(o).gameObject);
            prefab.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        lista = prefab.transform.parent;
        print("Count: " + s.Count);
        int i = 0;
        for (; i < s.Count;)
        {
            GameObject c = Instantiate(prefab, prefab.transform.parent);
            c.transform.GetChild(0).GetComponent<Text>().text = s[i];
            Firebase.Database.DatabaseReference r = re[i];
            c.GetComponent<Button>().onClick.AddListener(() =>
            {
                bt.onClick.RemoveAllListeners();
                bt.onClick.AddListener(() => EliminaCommento(c.transform.GetChild(0).GetComponent<Text>(), r, inFi.text));
                bt.transform.parent.parent.gameObject.SetActive(true);
            });
            i += 1;
        }
        Destroy(prefab);
        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection);
    }

    public void EliminaCommento(Text t, Firebase.Database.DatabaseReference r, string n)
    {
        AppManager.FIREBASE_CONTROLLER.ReplaceAccountName(r, n, s => {
            if (!s)
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "An error was occurred" });
            else
            {
                t.text = n;
                bt.transform.parent.parent.gameObject.SetActive(false);
            }
        });
    }

    public void Cerca(string s)
    {
        for (int i = 0; i < lista.childCount; i++)
            if (!lista.GetChild(i).GetChild(0).GetComponent<Text>().text.ToUpper().Contains(s.ToUpper()))
                lista.GetChild(i).gameObject.SetActive(false);
            else
                lista.GetChild(i).gameObject.SetActive(true);
    }

    public class DeleteMessagesCallback
    {
        public List<CommentoDaEl> messages;
    }

    public class CommentoDaEl
    {
        public Firebase.Database.DatabaseReference reference;
        public string Key;
        public string BodyTXT;
        public string DateCreated;
        public string UserID;
        public string FullName;
    }
}