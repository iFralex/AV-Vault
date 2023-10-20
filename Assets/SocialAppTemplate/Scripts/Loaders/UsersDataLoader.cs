using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Firebase.Database;

namespace SocialApp
{

    public class UsersDataLoader : MonoBehaviour
    {
        [SerializeField]
        private ScrollViewController ScrollView = default;
        [SerializeField]
        private InputField SearchInput = default;
        [SerializeField]
        private int AutoLoadCount = 3;

        [SerializeField]
        private List<string> UsersKeys = new List<string>();

        private int UsersLoaded = 0;

        private int CurrentRequestID = 0;

        private DatabaseReference DRFriendsCount;
        private DatabaseReference DRRequestFriendsCount;
        private DatabaseReference DRPendingFriendsCount;

        private void OnEnable()
        {
            AddListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            DRFriendsCount = AppManager.FIREBASE_CONTROLLER.GetFriendCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DRRequestFriendsCount = AppManager.FIREBASE_CONTROLLER.GetRequestFriendCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DRPendingFriendsCount = AppManager.FIREBASE_CONTROLLER.GetPendingFriendCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DRFriendsCount.ValueChanged += OnFriendsCountUpdated;
            DRRequestFriendsCount.ValueChanged += OnRequestCountUpdated;
            DRPendingFriendsCount.ValueChanged += OnPendingCountUpdated;
        }

        private void RemoveListeners()
        {
            if (AppManager.FIREBASE_CONTROLLER != null)
            {
                DRFriendsCount.ValueChanged -= OnFriendsCountUpdated;
                DRRequestFriendsCount.ValueChanged -= OnRequestCountUpdated;
                DRPendingFriendsCount.ValueChanged -= OnPendingCountUpdated;
            }
        }

        private void OnFriendsCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdateFriendsCount(0);
                return;
            }
            try
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdateFriendsCount(int.Parse(args.Snapshot.Value.ToString()));
            }
            catch (Exception)
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdateFriendsCount(0);
            }
        }

        private void OnRequestCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdateRequestCount(0);
                return;
            }
            try
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdateRequestCount(int.Parse(args.Snapshot.Value.ToString()));
            }
            catch (Exception)
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdateRequestCount(0);
            }
        }

        private void OnPendingCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdatePendingCount(0);
                return;
            }
            try
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdatePendingCount(int.Parse(args.Snapshot.Value.ToString()));
            }
            catch (Exception)
            {
                AppManager.FRIEND_UI_CONTROLLER.UpdatePendingCount(0);
            }
        }

        public void ResetLoader()
        {
            UsersLoaded = 0;
            UsersKeys.Clear();
            UsersKeys.TrimExcess();
            ScrollView.ResetSroll();
            ScrollView.HideAllScrollItems();
        }


        public void AutoLoadContent(bool _forward)
        {
            if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Search)
                return;
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
            _usersQuery.callback = OnUsersLoaded;
            _usersQuery.forward = _forward;
            _usersQuery.ownerID = AppManager.USER_PROFILE.FIREBASE_USER.UserId;

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
                CurrentRequestID++;
                _usersQuery.RequestID = CurrentRequestID;
                if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Search)
                {
                    AppManager.FIREBASE_CONTROLLER.SearchUsers(_usersQuery, SearchInput.text);
                }
                else
                {
                    _usersQuery.Type = AppManager.FRIEND_UI_CONTROLLER.CurrentTabState;
                    AppManager.FIREBASE_CONTROLLER.GetFriendsAt(_usersQuery);
                }
                ScrollView.BlockScroll();
            }
        }

        public void OnUsersLoaded(UsersCallback _callback)
        {
            ScrollView.UnblockScroll();
            if (_callback.IsSuccess && CurrentRequestID == _callback.RequestID)
            {
                int _usersCount = _callback.users.Count;
                if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Search)
                {
                    _usersCount = Mathf.Clamp(_callback.users.Count, 0, ScrollView.GetContentListCount());
                }
                List<ScrollViewItem> _itemsList = ScrollView.PushItem(_usersCount, _callback.forward);
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

    public class UsersQuery
    {
        public int startIndex;
        public int endIndex;
        public Action<UsersCallback> callback;
        public bool forward;
        public string indexKey;
        public string ownerID;
        public FriendsTabState Type;
        public int RequestID;
    }

    public class UsersCallback
    {
        public List<User> users;
        public bool forward;
        public bool IsSuccess;
        public int RequestID;
    }
}