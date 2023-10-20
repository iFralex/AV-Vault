using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SocialApp
{

    public class SelectFromFriendsLoader : MonoBehaviour
    {
        [SerializeField]
        private ScrollViewController ScrollView = default;
        [SerializeField]
        private RectTransform ScrollViewRect = default;
        [SerializeField]
        private GameObject InputGameObject = default;
        [SerializeField]
        private GameObject ApplyBtn = default;
        [SerializeField]
        private Text WindowTitle = default;
        [SerializeField]
        private int AutoLoadCount = 3;

        [SerializeField]
        private List<string> UsersKeys = new List<string>();

        private int UsersLoaded = 0;

        private string UserId;

        private AddNewChatType CurrerntType;

        public float HiddenScrollOfsetY;
        public float FullScrollOfsetY;

        private MessageGroupInfo CurrentMessageGroup;

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

        public void LoadWindow(AddNewChatType _type, MessageGroupInfo _group = null)
        {
            CurrentMessageGroup = _group;
            CurrerntType = _type;
            CheckUI();
            if (CurrerntType == AddNewChatType.SHOW_CHAT_MEMBERS)
            {
                LoadGroupUsers(_group.ChatID);
            }
            else
            {
                LoadUserFriends(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            }
        }

        private void CheckUI()
        {
            if (CurrerntType == AddNewChatType.ADD_NEW_CHAT)
            {
                ScrollViewRect.offsetMax = new Vector2(ScrollViewRect.offsetMax.x, -HiddenScrollOfsetY);
                InputGameObject.SetActive(true);
            }
            if (CurrerntType == AddNewChatType.ADD_NEW_MEMBERS || CurrerntType == AddNewChatType.SHOW_CHAT_MEMBERS)
            {
                ScrollViewRect.offsetMax = new Vector2(ScrollViewRect.offsetMax.x, -FullScrollOfsetY);
                InputGameObject.SetActive(false);
            }
            ApplyBtn.SetActive(CurrerntType != AddNewChatType.SHOW_CHAT_MEMBERS);

            if (CurrerntType == AddNewChatType.ADD_NEW_CHAT) WindowTitle.text = "Add new chat";
            if (CurrerntType == AddNewChatType.ADD_NEW_MEMBERS) WindowTitle.text = "Add new members";
            if (CurrerntType == AddNewChatType.SHOW_CHAT_MEMBERS) WindowTitle.text = "Members";
        }

        public void LoadUserFriends(string _userId)
        {
            UserId = _userId;
            AutoLoadContent(true);
        }

        public void LoadGroupUsers(string _groupId)
        {
            UserId = _groupId;
            AutoLoadContent(true);
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
                if (CurrerntType == AddNewChatType.SHOW_CHAT_MEMBERS)
                {
                    AppManager.FIREBASE_CONTROLLER.GetGroupUsersAt(_usersQuery);
                }
                else
                {
                    AppManager.FIREBASE_CONTROLLER.GetFriendsAt(_usersQuery);
                }
                
                ScrollView.BlockScroll();
            }
        }

        public void OnFriendsLoaded(UsersCallback _callback)
        {
            if (CurrerntType == AddNewChatType.ADD_NEW_MEMBERS)
            {
                for (int j=0;j< _callback.users.Count;j++)
                {
                    if (CurrentMessageGroup.Users.Contains(_callback.users[j].UserID))
                    {
                        _callback.users.RemoveAt(j);
                    }
                }
                _callback.users.TrimExcess();
            }
            ScrollView.UnblockScroll();
            if (_callback.IsSuccess)
            {
                List<ScrollViewItem> _itemsList = ScrollView.PushItem(_callback.users.Count, _callback.forward);
                for (int i = 0; i < _itemsList.Count; i++)
                {
                    _itemsList[i].gameObject.GetComponent<SelectUserViewController>().DisplayInfo(_callback.users[i], CurrerntType != AddNewChatType.SHOW_CHAT_MEMBERS);
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

        public AddNewChatType GetWindowType()
        {
            return CurrerntType;
        }

        public MessageGroupInfo GetCurrrentMessageGroup()
        {
            return CurrentMessageGroup;
        }
    }

    public enum AddNewChatType
    {
        ADD_NEW_CHAT,
        ADD_NEW_MEMBERS,
        SHOW_CHAT_MEMBERS
    }
}
