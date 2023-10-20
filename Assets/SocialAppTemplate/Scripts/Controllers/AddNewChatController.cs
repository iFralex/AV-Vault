using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SocialApp
{
    public class AddNewChatController : MonoBehaviour
    {
        [SerializeField]
        private InputField ChatNameInput = default;

        private static List<string> SelectedUsersIDs = new List<string>();

        public static event Action<MessageGroupInfo> OnNewMembersAdded;

        private void OnEnable()
        {
            ClearWindows();
            SelectUserViewController.OnUserSelected += AddUserToList;
            SelectUserViewController.OnUserDeselected += RemoveUserFromList;
        }

        private void OnDisable()
        {
            SelectUserViewController.OnUserSelected -= AddUserToList;
            SelectUserViewController.OnUserDeselected -= RemoveUserFromList;
        }

        public static bool ContainUser(User _user)
        {
            return SelectedUsersIDs.Contains(_user.UserID);
        }

        private void ClearWindows()
        {
            SelectedUsersIDs.Clear();
            SelectedUsersIDs.TrimExcess();
        }

        public void ApplyChanges()
        {
            AddNewChatType windowType = GetComponentInChildren<SelectFromFriendsLoader>().GetWindowType();
            if (windowType == AddNewChatType.ADD_NEW_CHAT)
            {
                AddNewGroupChat();
            }
            else if (windowType == AddNewChatType.ADD_NEW_MEMBERS)
            {
                AddNewMembers();
            }
        }

        public void AddNewMembers()
        {
            MessageGroupInfo messageGroup = GetComponentInChildren<SelectFromFriendsLoader>().GetCurrrentMessageGroup();
            if (SelectedUsersIDs.Count <= 0)
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = "Not all fields are filled";
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
                return;
            }

            AppManager.VIEW_CONTROLLER.ShowLoading();

            for (int i=0;i< SelectedUsersIDs.Count;i++)
            {
                messageGroup.Users.Add(SelectedUsersIDs[i]);
            }

            AppManager.FIREBASE_CONTROLLER.AddOrUpdateChatInfo(messageGroup, NewMembersAdded);
        }

        private void NewMembersAdded(ChatInfoMessage _callback)
        {
            if (_callback.IsSuccess)
            {
                AppManager.FIREBASE_CONTROLLER.UpdateMessagesList(_callback.Info);
            }
            OnNewMembersAdded?.Invoke(_callback.Info);
            AppManager.VIEW_CONTROLLER.HideLoading();
            //AppManager.VIEW_CONTROLLER.HideAddNEwChat();
        }

        public void AddNewGroupChat()
        {
            string _chatName = ChatNameInput.text;
            if (string.IsNullOrEmpty(_chatName) || SelectedUsersIDs.Count <= 0)
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = "Not all fields are filled";
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
                return;
            }

            AppManager.VIEW_CONTROLLER.ShowLoading();

            MessageGroupInfo _groupInfo = new MessageGroupInfo();
            _groupInfo.ChatID = Guid.NewGuid().ToString();
            _groupInfo.Users = SelectedUsersIDs;
            _groupInfo.Users.Add(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            _groupInfo.Type = MessageType.Group;
            _groupInfo.ChatName = _chatName;

            AppManager.FIREBASE_CONTROLLER.AddOrUpdateChatInfo(_groupInfo, OnNewChatAdded);
        }

        public void OnNewChatAdded(ChatInfoMessage _callback)
        {
            if (_callback.IsSuccess)
            {
                AppManager.FIREBASE_CONTROLLER.UpdateMessagesList(_callback.Info);
                //AppManager.VIEW_CONTROLLER.HideAddNEwChat();
            }
            else
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = "Failed to add new chat";
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
            }
            AppManager.VIEW_CONTROLLER.HideLoading();
        }

        public void AddUserToList(User _user)
        {
            if (!SelectedUsersIDs.Contains(_user.UserID))
            {
                SelectedUsersIDs.Add(_user.UserID);
            }
        }

        public void RemoveUserFromList(User _user)
        {
            if (SelectedUsersIDs.Contains(_user.UserID))
            {
                SelectedUsersIDs.Remove(_user.UserID);
                SelectedUsersIDs.TrimExcess();
            }
        }
    }
}
