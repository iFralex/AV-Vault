using UnityEngine;
using UnityEngine.UI;
using System;

namespace SocialApp
{
    public class SelectUserViewController : MonoBehaviour
    {
        [SerializeField]
        private Text FullNameLabel = default;
        [SerializeField]
        private Toggle SelectToggle = default;
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

        private User CurrentUser;

        // events
        public static Action<User> OnUserSelected;
        public static Action<User> OnUserDeselected;


        private void ClearData()
        {
            AvatarView.DisplayDefaultAvatar();
            FullNameLabel.text = string.Empty;
            CurrentUser = null;
            OnlineImage.color = OfflineColor;
        }

        public void DisplayInfo(User _user, bool _Selectable = true)
        {
            ClearData();
            AvatarView.SetCacheTexture(CacheAvatar);
            CurrentUser = _user;
            FullNameLabel.text = CurrentUser.FullName;
            GetProfileImage();
            OnlineController.SetUser(CurrentUser.UserID);
            OnlineController.SetUpdateAction(OnOnlineStatusUpdated);
            OnlineController.StartCheck();

            if (_Selectable)
            {
                SelectToggle.gameObject.SetActive(true);
                SelectToggle.isOn = AddNewChatController.ContainUser(_user);
            }
            else
            {
                SelectToggle.gameObject.SetActive(false);
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
            AvatarView.LoadAvatar(CurrentUser.UserID);
        }

        public void ShowUserProfile()
        {
            if (AppManager.USER_PROFILE.IsMine(CurrentUser.UserID))
            {
                AppManager.NAVIGATION.ShowUserProfile();
            }
            else
            {
                AppManager.VIEW_CONTROLLER.HideNavigationGroup();
                //AppManager.VIEW_CONTROLLER.ShowAnotherUserProfile(CurrentUser.UserID);
            }
        }

        public void OnToggleValueChange()
        {
            if (SelectToggle.isOn)
            {
                OnUserSelected?.Invoke(CurrentUser);
            }
            else
            {
                OnUserDeselected?.Invoke(CurrentUser);
            }
        }
    }
}
