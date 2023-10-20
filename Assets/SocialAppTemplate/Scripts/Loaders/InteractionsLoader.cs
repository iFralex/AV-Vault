using System;
using System.Collections;
using System.Collections.Generic;
using SocialApp;
using UnityEngine;

public class InteractionsLoader : MonoBehaviour
{
    int numero;
    public bool primo;
    [SerializeField]
    private ScrollViewController ScrollView = default;
    [SerializeField]
    private int InteractionsLoaded = 0;
    [SerializeField]
    private List<string> InteractionsKeys = new List<string>();

    public void OnEnable()
    {
        InteractionsKeys.Clear();
        InteractionsLoaded = 0;
        primo = false;
        Avanti(true);
    }

    public void Avanti(bool b) => LoadInteractions(b);

    public void LoadInteractions(bool _forward)
    {
        ScrollView.HideAllScrollItems();
        AppManager.VIEW_CONTROLLER.ShowInteractionsLoading();
        LoadContent(InteractionsLoaded, ScrollView.GetContentListCount(), _forward);
    }

    private void LoadContent(int _startIndex, int _endIndex, bool _forward)
    {
        InteractionsQuery _interactionQuery = new InteractionsQuery();
        _interactionQuery.startIndex = _startIndex;
        _interactionQuery.endIndex = _endIndex;
        _interactionQuery.callback = OnMessagesLoaded;
        _interactionQuery.forward = _forward;
        _interactionQuery.UserId = AppManager.USER_PROFILE.FIREBASE_USER.UserId;
        string indexKey = string.Empty;
        if (_forward)
        {
            if (InteractionsKeys.Count > 0)
                indexKey = InteractionsKeys[InteractionsKeys.Count - 1];
        }
        else
            indexKey = InteractionsKeys[0];
        _interactionQuery.indexKey = indexKey;
        if (_endIndex >= 0)
            AppManager.FIREBASE_CONTROLLER.GetInteractionsAt(_interactionQuery);
    }

    public void OnMessagesLoaded(InteractionsCallback _callback)
    {
        if (_callback.IsSuccess)
        {
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection);
            print(_callback.interactions.Count + "  " + primo);
            if (_callback.interactions.Count == 0 && !primo)
            {
                foreach (ScrollViewItem a in ScrollView.PushItem(numero, true))
                    a.gameObject.GetComponent<InteractionViewController>().LoadMedia(a.GetComponent<InteractionViewController>().CurrentInteraction);
                AppManager.VIEW_CONTROLLER.HideInteractionsLoading();
                return;
            }
            if (_callback.interactions.Count == 0 && primo)
            {
                AppManager.VIEW_CONTROLLER.HideInteractionsLoading();
                return;
            }
            numero = _callback.interactions.Count;
            primo = false;
            AppManager.VIEW_CONTROLLER.HideInteractionsLoading();
            InteractionsKeys.Clear();
            bool isScrollViewFull = ScrollView.IsListFull();
            List<ScrollViewItem> _itemsList = ScrollView.PushItem(_callback.interactions.Count, true);
            _callback.interactions.Reverse();
            for (int i = 0; i < _itemsList.Count; i++)
            {
                _itemsList[i].gameObject.GetComponent<InteractionViewController>().LoadMedia(_callback.interactions[i]);
                InteractionsLoaded++;
                if (!InteractionsKeys.Contains(_callback.interactions[i].Key))
                    InteractionsKeys.Add(_callback.interactions[i].Key);
            }
        }
        else
        {
            AppManager.VIEW_CONTROLLER.HideInteractionsLoading();
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
        }
    }
}

public class Interaction
{
    public string UserID;
    public string PostID;
    public string Date;
    public int Type;
    public string Key;
}

public class InteractionsCallback
{
    public List<Interaction> interactions;
    public bool forward;
    public bool IsSuccess;
}

public class InteractionsQuery
{
    public int startIndex;
    public int endIndex;
    public Action<InteractionsCallback> callback;
    public bool forward;
    public string indexKey;
    public string UserId;
}