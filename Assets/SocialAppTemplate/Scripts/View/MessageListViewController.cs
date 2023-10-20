using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System;
using TMPro;

namespace SocialApp
{
    public class MessageListViewController : MonoBehaviour
    {
        [SerializeField]
        private Text FullNameLabel = default;
        [SerializeField]
        private TextMeshProUGUI MessageBodyLabel = default;
        [SerializeField]
        private AvatarViewController AvatarView = default;
        [SerializeField]
        private bool CacheAvatar = default;
        [SerializeField]
        private OnlineStatusController OnlineController = default;
        [SerializeField]
        private Image OnlineImage = default;
        [SerializeField]
        private Color OnlineColor = default;
        [SerializeField]
        private Color OfflineColor = default;
        [SerializeField]
        private GameObject UnreadObject = default;
        [SerializeField]
        private Text UnreadLabel = default;

        private string CurrentUserID;
        private MessageGroupInfo MessageInfo;


        private void ClearData()
        {
            AvatarView.DisplayDefaultAvatar();
            FullNameLabel.text = string.Empty;
            CurrentUserID = string.Empty;
            OnlineImage.color = OfflineColor;
            UnreadObject.SetActive(false);
            UnreadLabel.text = string.Empty;
            RemoveListeners();
            MessageInfo = null;
        }

        public void DisplayInfo(string _chatID)
        {
            AvatarView.SetCacheTexture(CacheAvatar);
            ClearData();

            AppManager.FIREBASE_CONTROLLER.GetGroupChatInfo(_chatID, _info =>
            {
                if (_info.IsSuccess)
                {
                    DisplayGroupMessage(_info.Info);
                }
                else
                {
                    // check legacy solution
                    MessageGroupInfo _newInfo = new MessageGroupInfo();
                    _newInfo.ChatID = AppManager.FIREBASE_CONTROLLER.GetUserMessageKey(AppManager.USER_PROFILE.FIREBASE_USER.UserId, _chatID);
                    _newInfo.Type = MessageType.Messanging;
                    _newInfo.Users = new List<string>();
                    _newInfo.Users.Add(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                    _newInfo.Users.Add(_chatID);

                    DisplayGroupMessage(_newInfo);
                }
            });
        }

        private void DisplayGroupMessage(MessageGroupInfo _info)
        {
            MessageInfo = _info;

            if (MessageInfo.Type == MessageType.Messanging)
            {
                CurrentUserID = MessagesDataLoader.GetTargetUserId(MessageInfo);
                AppManager.FIREBASE_CONTROLLER.GetUserFullName(CurrentUserID, _name =>
                {
                    FullNameLabel.text = _name;
                });
                GetProfileImage();
                OnlineController.SetUser(CurrentUserID);
                OnlineController.SetUpdateAction(OnOnlineStatusUpdated);
                OnlineController.StartCheck();

                AppManager.FIREBASE_CONTROLLER.GetLastMessageWithUser(CurrentUserID, _msg =>
                {
                    MessageBodyLabel.text = _msg;
                });
            }
            else if (MessageInfo.Type == MessageType.Group)
            {
                CurrentUserID = MessageInfo.ChatID;
                FullNameLabel.text = MessageInfo.ChatName;
                GetProfileImage();

                AppManager.FIREBASE_CONTROLLER.GetLastMessageAtGroup(MessageInfo.ChatID, _msg =>
                {
                    MessageBodyLabel.text = _msg;
                });
            }

            AddListeners();
        }

        DatabaseReference CountRef;
        public void AddListeners()
        {
            if (MessageInfo.Type == MessageType.Messanging)
            {
                CountRef = AppManager.FIREBASE_CONTROLLER.GetUnreadCountWithUserReferece(AppManager.FIREBASE_CONTROLLER.GetUserMessageKey(AppManager.USER_PROFILE.FIREBASE_USER.UserId, CurrentUserID));
            }
            else if (MessageInfo.Type == MessageType.Group)
            {
                CountRef = AppManager.FIREBASE_CONTROLLER.GetUnreadCountWithUserReferece(MessageInfo.ChatID);
            } 
            CountRef.ValueChanged += OnUnreadCountUpdated;
        }

        public void RemoveListeners()
        {
            if (CountRef != null)
            {
                CountRef.ValueChanged -= OnUnreadCountUpdated;
                CountRef = null;
            }
        }

        private void OnUnreadCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                UnreadLabel.text = "0";
                UnreadObject.SetActive(false);
                return;
            }
            try
            {
                if (args.Snapshot.Value.ToString() == "0")
                {
                    UnreadLabel.text = "0";
                    UnreadObject.SetActive(false);
                }
                else
                {
                    UnreadLabel.text = args.Snapshot.Value.ToString();
                    UnreadObject.SetActive(true);
                }
            }
            catch (Exception)
            {
                UnreadLabel.text = "0";
                UnreadObject.SetActive(false);
            }
        }

        private void OnOnlineStatusUpdated()
        {
            if (OnlineController.IsOnline())
            {
                OnlineImage.color = OnlineColor;
            }
            else
            {
                OnlineImage.color = OfflineColor;
            }
        }

        private void GetProfileImage()
        {
            if (MessageInfo.Type == MessageType.Group)
            {
                AvatarView.DisplayGroupChatAvatar();
            }
            else
            {
                AvatarView.LoadAvatar(CurrentUserID);
            }
        }
    }
}