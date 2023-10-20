using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocialApp;

namespace SocialApp
{
    public class FriendsUIController : MonoBehaviour
    {
        [SerializeField]
        private Image FriendBack = default;
        [SerializeField]
        private Image RequestBack = default;
        [SerializeField]
        private Image PendingBack = default;
        [SerializeField]
        private Image SearchBack = default;
        [SerializeField]
        private Image SearchIcon = default;
        [SerializeField]
        private InputField SearchInput = default;
        [SerializeField]
        private GameObject SearchPanel = default;
        [SerializeField]
        private RectTransform ScrollViewRect = default;
        [SerializeField]
        private float DefaultScrollHeight = default;
        [SerializeField]
        private float SearchScrollHeight = default;
        [SerializeField]
        private Color ActiveColor = default;
        [SerializeField]
        private Color DefaultColor = default;
        [SerializeField]
        private UsersDataLoader UserLoader = default;
        [SerializeField]
        private Text FriendsCountLabel = default;
        [SerializeField]
        private Text RequestCountLabel = default;
        [SerializeField]
        private Text PendingCountLabel = default;

        public FriendsTabState CurrentTabState;



        public void OnFriends()
        {
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            CurrentTabState = FriendsTabState.Friend;
            ToDefaultState();
            FriendBack.color = ActiveColor;
            UserLoader.ResetLoader();
            UserLoader.AutoLoadContent(true);
            ScrollViewRect.offsetMax = new Vector2(ScrollViewRect.offsetMax.x, -DefaultScrollHeight);
        }

        public void OnRequest()
        {
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            CurrentTabState = FriendsTabState.Requested;
            ToDefaultState();
            RequestBack.color = ActiveColor;
            UserLoader.ResetLoader();
            UserLoader.AutoLoadContent(true);
            ScrollViewRect.offsetMax = new Vector2(ScrollViewRect.offsetMax.x, -DefaultScrollHeight);
        }

        public void OnPending()
        {
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            CurrentTabState = FriendsTabState.Pending;
            ToDefaultState();
            PendingBack.color = ActiveColor;
            UserLoader.ResetLoader();
            UserLoader.AutoLoadContent(true);
            ScrollViewRect.offsetMax = new Vector2(ScrollViewRect.offsetMax.x, -DefaultScrollHeight);
        }

        public void OnSearch()
        {
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            CurrentTabState = FriendsTabState.Search;
            ToDefaultState();
            SearchInput.text = string.Empty;
            SearchBack.color = ActiveColor;
            SearchIcon.color = DefaultColor;
            SearchPanel.SetActive(true);
            UserLoader.ResetLoader();
            ScrollViewRect.offsetMax = new Vector2(ScrollViewRect.offsetMax.x, -SearchScrollHeight);
        }

        private void ToDefaultState()
        {
            FriendBack.color = DefaultColor;
            RequestBack.color = DefaultColor;
            PendingBack.color = DefaultColor;
            SearchBack.color = DefaultColor;
            SearchIcon.color = ActiveColor;
            SearchPanel.SetActive(false);
        }

        public UsersDataLoader GetUsersDataLoader()
        {
            return UserLoader;
        }

        public void UpdateFriendsCount(int _count)
        {
            FriendsCountLabel.text = _count.ToString();
        }

        public void UpdateRequestCount(int _count)
        {
            RequestCountLabel.text = _count.ToString();
        }

        public void UpdatePendingCount(int _count)
        {
            PendingCountLabel.text = _count.ToString();
        }
    }

    public enum FriendsTabState
    {
        Friend,
        Requested,
        Pending,
        Search
    }
}
