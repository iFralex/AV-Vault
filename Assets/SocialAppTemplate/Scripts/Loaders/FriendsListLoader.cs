using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Firebase.Database;
using SocialApp;

namespace SocialApp
{

    public class FriendsListLoader : MonoBehaviour
    {
        [SerializeField]
        private ScrollViewController ScrollView = default;
        [SerializeField]
        private int AutoLoadCount = 3;
        [SerializeField]
        private Text UsernameLabel = default;

        [SerializeField]
        private List<string> UsersKeys = new List<string>();

        private int UsersLoaded = 0;

        private string UserId;

        private void OnEnable()
        {
            ResetLoader();
        }

        public void ResetLoader()
        {
            UsersLoaded = 0;
            UsersKeys.Clear();
            UsersKeys.TrimExcess();
            ScrollView.ResetSroll();
            ScrollView.HideAllScrollItems();
        }

        public void LoadUserFriends(string _userId)
        {
            UserId = _userId;
            AutoLoadContent(true);
            AppManager.FIREBASE_CONTROLLER.GetUserFullName(UserId, _name =>
            {
                UsernameLabel.text = _name;
            });
        }

        public void AutoLoadContent(bool _forward)
        {
            if (_forward)
            {
                int loadCount = UsersLoaded + AutoLoadCount;
                if (UsersLoaded <= 0)
                {
                    loadCount = ScrollView.GetContentListCount();
                }
                LoadContent(UsersLoaded, loadCount, _forward);
            }
            else
            {
                LoadContent(UsersLoaded - ScrollView.GetContentListCount() - AutoLoadCount, UsersLoaded - ScrollView.GetContentListCount() - 1, _forward);
            }
        }

        private void LoadContent(int _startIndex, int _endIndex, bool _forward)
        {
            UsersQuery _usersQuery = new UsersQuery();
            _usersQuery.startIndex = _startIndex;
            _usersQuery.endIndex = _endIndex;
            _usersQuery.callback = OnFriendsLoaded;
            _usersQuery.forward = _forward;
            _usersQuery.ownerID = UserId;

            string indexKey = string.Empty;
            if (_forward)
            {
                if (UsersKeys.Count > 0)
                {
                    indexKey = UsersKeys[UsersLoaded - 1];
                }
            }
            else
            {
                if (_startIndex < 0)
                {
                    _startIndex = 0;
                    _usersQuery.startIndex = _startIndex;
                }
                indexKey = UsersKeys[_startIndex];
            }

            _usersQuery.indexKey = indexKey;
            if (_endIndex >= 0)
            {
                _usersQuery.Type = FriendsTabState.Friend;
                AppManager.FIREBASE_CONTROLLER.GetFriendsAt(_usersQuery);
                ScrollView.BlockScroll();
            }
        }

        public void OnFriendsLoaded(UsersCallback _callback)
        {
            ScrollView.UnblockScroll();
            if (_callback.IsSuccess)
            {
                List<ScrollViewItem> _itemsList = ScrollView.PushItem(_callback.users.Count, _callback.forward);
                for (int i = 0; i < _itemsList.Count; i++)
                {
                    _itemsList[i].gameObject.GetComponent<UserViewController>().DisplayInfo(_callback.users[i]);
                    if (_callback.forward)
                    {
                        UsersLoaded++;
                        AddUserKey(_callback.users[i].UserID);
                    }
                    else
                    {
                        UsersLoaded--;
                    }
                }
                if (!_callback.forward)
                    ScrollView.UpdateScrollViewPosition(_itemsList, _callback.forward);
            }
        }

        public void OnSearch()
        {
            ResetLoader();
            LoadContent(UsersLoaded, UsersLoaded + AutoLoadCount, true);
        }

        private void AddUserKey(string _key)
        {
            if (!UsersKeys.Contains(_key))
            {
                UsersKeys.Add(_key);
            }
        }
    }
}
