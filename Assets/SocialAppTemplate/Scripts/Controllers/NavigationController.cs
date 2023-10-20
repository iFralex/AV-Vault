using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using System;
using SocialApp;

namespace SocialApp
{
    public class NavigationController : MonoBehaviour
    {

        [SerializeField]
        private Image[] Icons = default;
        [SerializeField]
        private Image[] Backgounds = default;

        private Color ActiveColor = default;
        private Color ActiveBackColor = default;
        private GameObject UnreadCountObject = default;
        private Text UnreadCountLabel = default;
        private GameObject FriendFeedsCountObject = default;
        private Text FriendFeedCountLabel = default;
        private GameObject FriendCountObject = default;
        private Text FriendCountLabel = default;

        private DatabaseReference DRRequestFriendsCount;
        private DatabaseReference DRFeedFriendsCount;
        private DatabaseReference DRUnreadAllCount;

        private NavigationTab CurrentTab;

        public void ShowUserProfile()
        {
            CurrentTab = NavigationTab.Profile;
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            AppManager.VIEW_CONTROLLER.ShowUserProfile();

            ToDefault();
            //Icons[0].color = ActiveColor;
            Backgounds[0].gameObject.SetActive(true);
        }

        public void ShowFriends()
        {
            CurrentTab = NavigationTab.Friends;
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            AppManager.FRIEND_UI_CONTROLLER.OnFriends();

            ToDefault();
            Icons[3].color = ActiveColor;
            Backgounds[3].color = ActiveBackColor;
        }

        public void ShowFriendsNews()
        {
            CurrentTab = NavigationTab.FriendsNews;
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            AppManager.VIEW_CONTROLLER.ShowFriendsNews();
            ToDefault();
            Icons[2].color = ActiveColor;
            Backgounds[2].color = ActiveBackColor;
            AppManager.FIREBASE_CONTROLLER.ClearUnreadFriendsFeed();
        }

        float tempoAllNews;
        public void ShowAllNewsDown()
        {
            tempoAllNews = Time.time;
        }

        public void ShowAllNewsUp()
        {
            void Attiva()
            {
                CurrentTab = NavigationTab.WorldNews;
                AppManager.DEVICE_CONTROLLER.UnloadAssets();
                AppManager.VIEW_CONTROLLER.HideNavigationGroup();
                AppManager.VIEW_CONTROLLER.ShowWorldNews();
                ToDefault();
                //Icons[1].color = ActiveColor;
                Backgounds[1].gameObject.SetActive(true);
                AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.MediumImpact);
            }
            if (tempoAllNews + .5f > Time.time)
                Attiva();
            else
            {
                if (!Backgounds[1].gameObject.activeInHierarchy)
                    Attiva();
                else
                {
                    AppManager.VIEW_CONTROLLER.SwitchOrderFeedWorldNew();
                    AppManager.VIEW_CONTROLLER.WorldNewsObject.GetComponent<FeedsDataLoader>().OnDisable();
                    AppManager.VIEW_CONTROLLER.WorldNewsObject.GetComponent<FeedsDataLoader>().OnEnable();
                }
            }
        }

        public void ShowMainNews()
        {
            CurrentTab = NavigationTab.MainNews;
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            AppManager.VIEW_CONTROLLER.ShowMainNews();
            ToDefault();
            //Icons[2].color = ActiveColor;
            Backgounds[2].gameObject.SetActive(true);
            AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
        }

        public void ShowFollowedFeed()
        {
            CurrentTab = NavigationTab.MainNews;
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            AppManager.VIEW_CONTROLLER.ShowFollowedFeed();
            ToDefault();
            //Icons[2].color = ActiveColor;
            Backgounds[3].gameObject.SetActive(true);
            AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
        }

        public void ShowMessanging()
        {
            CurrentTab = NavigationTab.Messanging;
            AppManager.DEVICE_CONTROLLER.UnloadAssets();
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            
            ToDefault();
            Icons[4].color = ActiveColor;
            Backgounds[4].color = ActiveBackColor;
        }

        public void AddListeners()
        {/*
            if (AppManager.FIREBASE_CONTROLLER.IsFirebaseInited())
            {
                DRRequestFriendsCount = AppManager.FIREBASE_CONTROLLER.GetRequestFriendCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                DRFeedFriendsCount = AppManager.FIREBASE_CONTROLLER.GetFriendFeedCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                DRUnreadAllCount = AppManager.FIREBASE_CONTROLLER.GetAllUnreadCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                DRRequestFriendsCount.ValueChanged += OnRequestCountUpdated;
                DRFeedFriendsCount.ValueChanged += OnFriendsCountUpdated;
                DRUnreadAllCount.ValueChanged += OnUnreadCountUpdated;
            }
            UnreadCountObject.SetActive(false);
            FriendFeedsCountObject.SetActive(false);
            FriendCountObject.SetActive(false);*/
        }

        public void RemoveListeners()
        {/*
            if (AppManager.FIREBASE_CONTROLLER.IsFirebaseInited())
            {
                DRRequestFriendsCount.ValueChanged -= OnRequestCountUpdated;
                DRFeedFriendsCount.ValueChanged -= OnFriendsCountUpdated;
                DRUnreadAllCount.ValueChanged -= OnUnreadCountUpdated;
            }*/
        }

        private void ToDefault()
        {
            foreach (Image _background in Backgounds)
            {
                _background.gameObject.SetActive(false);
            }/*
            foreach (Image _icon in Icons)
            {
                _icon.color = DefaultColor;
            }*/
        }

        private void OnRequestCountUpdated(object sender, ValueChangedEventArgs args)
        {/*
            if (args.DatabaseError != null)
            {
                FriendCountLabel.text = "0";
                FriendCountObject.SetActive(false);
                return;
            }
            try
            {
                if (args.Snapshot.Value.ToString() == "0")
                {
                    FriendCountLabel.text = "0";
                    FriendCountObject.SetActive(false);
                }
                else
                {
                    FriendCountLabel.text = args.Snapshot.Value.ToString();
                    FriendCountObject.SetActive(true);
                }
            }
            catch (Exception)
            {
                FriendCountLabel.text = "0";
                FriendCountObject.SetActive(false);
            }*/
        }

        private void OnFriendsCountUpdated(object sender, ValueChangedEventArgs args)
        {
            /*if (args.DatabaseError != null)
            {
                FriendFeedCountLabel.text = "0";
                FriendFeedsCountObject.SetActive(false);
                return;
            }
            try
            {
                if (args.Snapshot.Value.ToString() == "0")
                {
                    FriendFeedCountLabel.text = "0";
                    FriendFeedsCountObject.SetActive(false);
                }
                else
                {
                    FriendFeedCountLabel.text = args.Snapshot.Value.ToString();
                    FriendFeedsCountObject.SetActive(true);
                }
            }
            catch (Exception)
            {
                FriendFeedCountLabel.text = "0";
                FriendFeedsCountObject.SetActive(false);
            }*/
        }

        private void OnUnreadCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                UnreadCountLabel.text = "0";
                UnreadCountObject.SetActive(false);
                return;
            }
            try
            {
                if (args.Snapshot.Value.ToString() == "0")
                {
                    UnreadCountLabel.text = "0";
                    UnreadCountObject.SetActive(false);
                }
                else
                {
                    UnreadCountLabel.text = args.Snapshot.Value.ToString();
                    UnreadCountObject.SetActive(true);
                }
            }
            catch (Exception)
            {
                UnreadCountLabel.text = "0";
                UnreadCountObject.SetActive(false);
            }
        }

        public void ShowLastTab()
        {
            switch (CurrentTab)
            {
                case NavigationTab.Profile:
                    ShowUserProfile();
                    break;
                case NavigationTab.WorldNews:
                    tempoAllNews = 1000000;
                    ShowAllNewsUp();
                    break;
                case NavigationTab.FriendsNews:
                    ShowFriendsNews();
                    break;
                case NavigationTab.Friends:
                    ShowFriends();
                    break;
                case NavigationTab.Messanging:
                    ShowMessanging();
                    break;
                default:
                    Debug.Log("NOTHING");
                    break;
            }


        }

        public enum NavigationTab
        {
            Profile,
            WorldNews,
            FriendsNews,
            Friends,
            Messanging,
            Setting,
            MainNews
        }
    }
}
