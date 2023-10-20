using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Firebase.Database;
using SocialApp;

namespace SocialApp
{

    public class MessageListLoader : MonoBehaviour
    {
        [SerializeField]
        private ScrollViewController ScrollView = default;
        [SerializeField]
        private int AutoLoadCount = 3;

        [SerializeField]
        private List<string> MessagesKeys = new List<string>();

        private int MessagesLoaded = 0;

        private bool MessageWasLoaded = false;

        private Query DRMessageList;


        private void OnEnable()
        {
            MessageWasLoaded = false;
            AddListeners();
            ResetLoader();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            if (AppManager.FIREBASE_CONTROLLER != null)
            {
                DRMessageList = AppManager.FIREBASE_CONTROLLER.GetMessageListReferece().LimitToLast(1);
                DRMessageList.ChildAdded += HandleMessageAdded;
            }
        }

        private void RemoveListeners()
        {
            if (AppManager.FIREBASE_CONTROLLER != null)
            {
                DRMessageList.ChildAdded -= HandleMessageAdded;
            }
        }

        void HandleMessageAdded(object sender, ChildChangedEventArgs args)
        {
            ResetLoader();
            if (MessageWasLoaded == false)
            {
                MessageWasLoaded = true;
                AutoLoadContent(true);
            }
        }

        public void ResetLoader()
        {
            MessagesLoaded = 0;
            MessagesKeys.Clear();
            MessagesKeys.TrimExcess();
            ScrollView.ResetSroll();
            ScrollView.HideAllScrollItems();
        }


        public void AutoLoadContent(bool _forward)
        {
            if (_forward)
            {
                int loadCount = MessagesLoaded + AutoLoadCount;
                if (MessagesLoaded <= 0)
                {
                    loadCount = ScrollView.GetContentListCount();
                }
                LoadContent(MessagesLoaded, loadCount, _forward);
            }
            else
            {
                LoadContent(MessagesLoaded - ScrollView.GetContentListCount() - AutoLoadCount, MessagesLoaded - ScrollView.GetContentListCount() - 1, _forward);
            }
        }

        private void LoadContent(int _startIndex, int _endIndex, bool _forward)
        {
            MessageListQuery _messageListQuery = new MessageListQuery();
            _messageListQuery.startIndex = _startIndex;
            _messageListQuery.endIndex = _endIndex;
            _messageListQuery.callback = OnListLoaded;
            _messageListQuery.forward = _forward;
            _messageListQuery.ownerID = AppManager.USER_PROFILE.FIREBASE_USER.UserId;

            string indexKey = string.Empty;
            if (_forward)
            {
                if (MessagesKeys.Count > 0)
                {
                    indexKey = MessagesKeys[MessagesLoaded - 1];
                }
            }
            else
            {
                if (_startIndex < 0)
                {
                    _startIndex = 0;
                    _messageListQuery.startIndex = _startIndex;
                }
                indexKey = MessagesKeys[_startIndex];
            }

            _messageListQuery.indexKey = indexKey;
            if (_endIndex >= 0)
            {
                AppManager.FIREBASE_CONTROLLER.GetMessageListAt(_messageListQuery);
                ScrollView.BlockScroll();
            }
        }

        public void OnListLoaded(MessageListCallback _callback)
        {
            MessageWasLoaded = false;
            ScrollView.UnblockScroll();
            if (_callback.IsSuccess)
            {
                ResetLoader();
                List<ScrollViewItem> _itemsList = ScrollView.PushItem(_callback.usersIds.Count, _callback.forward);
                for (int i = 0; i < _itemsList.Count; i++)
                {
                    _itemsList[i].gameObject.GetComponent<MessageListViewController>().DisplayInfo(_callback.usersIds[i]);
                    if (_callback.forward)
                    {
                        MessagesLoaded++;
                        AddMessageKey(_callback.usersIds[i]);
                    }
                    else
                    {
                        MessagesLoaded--;
                    }
                }
                if (!_callback.forward)
                    ScrollView.UpdateScrollViewPosition(_itemsList, _callback.forward);
            }
        }


        private void AddMessageKey(string _key)
        {
            if (!MessagesKeys.Contains(_key))
            {
                MessagesKeys.Add(_key);
            }
        }

        public void ResetListeners()
        {
            //StartCoroutine(OnReload());
            //gameObject.SetActive(false);
            //gameObject.SetActive(true);
            OnDisable();
            OnEnable();
        }

        private IEnumerator OnReload()
        {
            RemoveListeners();
            yield return new WaitForSeconds(1f);
            AddListeners();
        }
    }

    public class MessageListQuery
    {
        public int startIndex;
        public int endIndex;
        public Action<MessageListCallback> callback;
        public bool forward;
        public string indexKey;
        public string ownerID;
    }

    public class MessageListCallback
    {
        public List<string> usersIds;
        public bool forward;
        public bool IsSuccess;
    }
}

