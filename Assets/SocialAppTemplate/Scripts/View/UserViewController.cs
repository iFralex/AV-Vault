using UnityEngine;
using UnityEngine.UI;


namespace SocialApp
{
    public class UserViewController : MonoBehaviour
    {
        [SerializeField]
        private Text FullNameLabel = default;
        [SerializeField]
        private GameObject AddToFriendBtn = default;
        [SerializeField]
        private GameObject AcceptFriendBtn = default;
        [SerializeField]
        private GameObject DeclineFriendBtn = default;
        [SerializeField]
        private GameObject RemoveFriendBtn = default;
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


        private void ClearData()
        {
            AvatarView.DisplayDefaultAvatar();
            FullNameLabel.text = string.Empty;
            CurrentUser = null;
            HideAllBtns();
            OnlineImage.color = OfflineColor;
        }

        public void DisplayInfo(User _user)
        {
            ClearData();
            AvatarView.SetCacheTexture(CacheAvatar);
            CurrentUser = _user;
            FullNameLabel.text = CurrentUser.FullName;
            GetProfileImage();
            DisplayButtons();
            OnlineController.SetUser(CurrentUser.UserID);
            OnlineController.SetUpdateAction(OnOnlineStatusUpdated);
            OnlineController.StartCheck();
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

        private void DisplayButtons()
        {
            if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Search)
                AppManager.FIREBASE_CONTROLLER.CanAddToFriend(CurrentUser.UserID, OnCanAddFriend);
            if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Friend)
                RemoveFriendBtn.SetActive(true);
            if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Pending)
                DeclineFriendBtn.SetActive(true);
            if (AppManager.FRIEND_UI_CONTROLLER.CurrentTabState == FriendsTabState.Requested)
                AcceptFriendBtn.SetActive(true);
        }

        private void OnCanAddFriend(bool _canAdd)
        {
            AddToFriendBtn.SetActive(_canAdd);
        }

        private void HideAllBtns()
        {
            AddToFriendBtn.SetActive(false);
            AcceptFriendBtn.SetActive(false);
            DeclineFriendBtn.SetActive(false);
            RemoveFriendBtn.SetActive(false);
        }

        public void AddToFriend()
        {
            if (CurrentUser == null)
                return;
            AppManager.FIREBASE_CONTROLLER.AddToFriends(CurrentUser.UserID, OnAddedToFriend);
        }

        private void OnAddedToFriend()
        {
            AddToFriendBtn.SetActive(false);
        }

        public void AcceptFriend()
        {
            if (CurrentUser == null)
                return;
            AppManager.FIREBASE_CONTROLLER.AcceptFriend(CurrentUser.UserID, OnFriendAccepted);
        }

        private void OnFriendAccepted()
        {
            AppManager.FRIEND_UI_CONTROLLER.OnRequest();
        }

        public void CancelPending()
        {
            if (CurrentUser == null)
                return;
            AppManager.FIREBASE_CONTROLLER.CancelPendingFromFriend(CurrentUser.UserID, OnPendingCanceled);
        }

        public void RemoveFriend()
        {
            if (CurrentUser == null)
                return;
            AppManager.FIREBASE_CONTROLLER.RemoveFromFriend(CurrentUser.UserID, OnFriendRemoved);
        }

        private void OnFriendRemoved()
        {
            AppManager.FRIEND_UI_CONTROLLER.OnFriends();
        }

        private void OnPendingCanceled()
        {
            AppManager.FRIEND_UI_CONTROLLER.OnPending();
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
    }
}
