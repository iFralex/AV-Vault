using System.Collections;
using System.Collections.Generic;
using SocialApp;
using UnityEngine;
using UnityEngine.UI;

public class eliminacommenti : MonoBehaviour
{
    public GameObject prefab;
    Transform lista;

    public void ApriCommenti(int i)
    {
        gameObject.SetActive(true);
        AppManager.FIREBASE_CONTROLLER.GetCommentsDaEliminare(i, OnMessagesLoaded);
    }

    public void OnMessagesLoaded(DeleteMessagesCallback _callback)
    {
        if (prefab == null)
        {
            prefab = lista.GetChild(0).gameObject;
            for (int o = 1; o < lista.childCount; o++)
                Destroy(lista.GetChild(o).gameObject);
            prefab.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        lista = prefab.transform.parent;
        int i = 0;
        for (; i < _callback.messages.Count;)
        {
            GameObject c = Instantiate(prefab, lista);
            c.GetComponent<MessageViewController>().LoadMedia(new Message() { BodyTXT = _callback.messages[i].BodyTXT, DateCreated = _callback.messages[i].DateCreated, UserID = _callback.messages[i].UserID, Type = ContentMessageType.TEXT, Key = _callback.messages[i].Key, FullName = _callback.messages[i].FullName });
            string key = _callback.messages[i].Key;
            string postId = _callback.messages[i].PostId;
            c.GetComponent<Button>().onClick.AddListener(() => EliminaCommento(c, key, postId));
            i += 1;
        }
        Destroy(prefab);
        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection);
    }

    public void EliminaCommento(GameObject c, string id, string postId)
    {
        AppManager.FIREBASE_CONTROLLER.DeleteComment(id, postId, s => {
            if (s)
                Destroy(c);
            else
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "An error was occurred" });
        });
    }

    public void Cerca(string s)
    {
        for (int i = 0; i < lista.childCount; i++)
            if (!lista.GetChild(i).GetChild(1).GetChild(0).GetComponent<Text>().text.ToUpper().Contains(s.ToUpper()))
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
        public string Key;
        public string BodyTXT;
        public string DateCreated;
        public string UserID;
        public string FullName;
        public string PostId;
    }
}