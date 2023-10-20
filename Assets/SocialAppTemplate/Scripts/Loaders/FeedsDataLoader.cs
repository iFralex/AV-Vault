using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Lofelt.NiceVibrations;

namespace SocialApp
{

    public class FeedsDataLoader : MonoBehaviour
    {
        public static bool loading = false;
        static int postCaricati;
        [SerializeField]
        public List<FeedViewController> itemList;
        [SerializeField]
        public FeedDataType LoaderType = default;
        public enum OrderFeed { cronologico, casuale};
        public OrderFeed ordine;
        [SerializeField]
        public List<string> FeedsKeys = new List<string>();

        public int FeedsLoaded = -1;
        public bool ultimoPost;

        [HideInInspector]
        public string CurrentUserID;
        List<Feed> feeds = new List<Feed>();
        public GameObject vuotoT;

        private void Awake()
        {
            if (LoaderType == FeedDataType.World)
                ordine  = (OrderFeed)PlayerPrefs.GetInt("Order world feed");
        }

        public void OnEnable()
        {
            ultimoPost = false;
            FeedsLoaded = -1;
            loading = false;
            LoadContent(true);
        }

        public void OnDisable()
        {
            ultimoPost = false;
            FeedsLoaded = -2;
            FeedsKeys.Clear();
            FeedsKeys.TrimExcess();
        }

        public void AutoLoadContent(bool _forward)
        {
            if (_forward)
            {
                LoadContent(_forward);
                FeedViewController a = itemList[1];
                itemList[1] = itemList[0];
                itemList[0] = a;
                a = itemList[1];
                itemList[1] = itemList[2];
                itemList[2] = a;
            }
            else
            {
                LoadContent(_forward);
                FeedViewController a = itemList[0];
                itemList[0] = itemList[2];
                itemList[2] = a;
                a = itemList[1];
                itemList[1] = itemList[2];
                itemList[2] = a;
                ultimoPost = false;
            }
        }

        public void ResetLoader()
        {
            FeedsLoaded = 0;
            FeedsKeys.Clear();
            FeedsKeys.TrimExcess();
            LoadContent(true);
        }

        private void LoadContent(bool _forward)
        {
            if (FeedsLoaded == 1 && !_forward)
            {
                itemList[1].gameObject.SetActive(false);
                FeedsLoaded = 0;
                return;
            }
            FeedQuery _feedQuery = new FeedQuery();
            _feedQuery.callback = OnFeedsLoaded;
            _feedQuery.forward = _forward;
            if (LoaderType == FeedDataType.User)
                _feedQuery.ownerID = CurrentUserID;
            else if (AppManager.USER_PROFILE.FIREBASE_USER != null)
                _feedQuery.ownerID = AppManager.USER_PROFILE.FIREBASE_USER.UserId;

            _feedQuery.indexKey = _feedQuery.targetKey = string.Empty;
            if (_forward)
            {
                if (FeedsKeys.Count > 0)
                    _feedQuery.indexKey = FeedsKeys[FeedsLoaded + 1];
                if (FeedsKeys.Count - 3 > FeedsLoaded)
                    _feedQuery.targetKey = FeedsKeys[FeedsLoaded + 2];
            }
            else
            {
                _feedQuery.indexKey = FeedsKeys[FeedsLoaded - 1];
                if (FeedsLoaded > 1)
                    _feedQuery.targetKey = FeedsKeys[FeedsLoaded - 2];
            }

            if (LoaderType == FeedDataType.Profile || LoaderType == FeedDataType.User)
                AppManager.FIREBASE_CONTROLLER.GetFeedsAt(_feedQuery);
            else if (LoaderType == FeedDataType.World)
            {
                if (ordine == OrderFeed.cronologico)
                    AppManager.FIREBASE_CONTROLLER.GetWorldFeedsAt(_feedQuery);
                else
                    AppManager.FIREBASE_CONTROLLER.GetWorldFeedsAtRandom(_feedQuery);
            }
            else if (LoaderType == FeedDataType.Premium)
                AppManager.FIREBASE_CONTROLLER.GetMainFeedsAt(_feedQuery);
            else if (LoaderType == FeedDataType.Followed)
                AppManager.FIREBASE_CONTROLLER.GetFollowedFeedsAt(_feedQuery);
            else if (LoaderType == FeedDataType.Reports)
                AppManager.FIREBASE_CONTROLLER.GetReportsFeedsAt(_feedQuery);
            else if (LoaderType == FeedDataType.SinglePost)
            {
                AppManager.FIREBASE_CONTROLLER.GetPostAt(AppSettings.memeCondivisoKey, OnFeedsLoaded);
                //AppManager.FIREBASE_CONTROLLER.GetPostAt("-MtOdg38eSHFEnDN7ASr", OnFeedsLoaded);
            }
            loading = true;
        }

        public void OnFeedsLoaded(FeedCallback _callback)
        {
            if (_callback.IsSuccess)
            {
                loading = false;
                if ((((int)AppSettings.TutorialMode == (int)FasiTutorial.ScorriADestraPost && _callback.forward) || ((int)AppSettings.TutorialMode == (int)FasiTutorial.ScorriASinistraPost && _callback.forward) || ((int)AppSettings.TutorialMode == (int)FasiTutorial.ApriCommenti && !_callback.forward)) && LoaderType == FeedDataType.World)
                    AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();

                if (AppSettings.mostraCondividi && postCaricati > 4)
                {
                    AppManager.VIEW_CONTROLLER.ShowShareApp();
                    AppSettings.mostraCondividi = false;
                    PlayerPrefs.SetInt("accessi", PlayerPrefs.GetInt("accessi") + 1);
                }
#if UNITY_IOS
                if (PlayerPrefs.GetInt("accessi") == 5 && postCaricati > 3)
                {
                    UnityEngine.iOS.Device.RequestStoreReview();
                    PlayerPrefs.SetInt("accessi", PlayerPrefs.GetInt("accessi") + 1);
                }
#endif
                if (_callback.feeds.Count != 0)
                    HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
                else
                    HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
                ultimoPost = false;
                if (FeedsLoaded == -1)
                {
                    for (int i = 0; i < itemList.Count; i++)
                        itemList[i].gameObject.SetActive(false);
                    if (_callback.feeds.Count == 0)
                    {
                        vuotoT.gameObject.SetActive(true);
                        ultimoPost = true;
                    }
                    else if (_callback.feeds.Count == 1)
                        ultimoPost = true;
                    for (int i = 0; i < _callback.feeds.Count; i++)
                    {
                        itemList[i].gameObject.SetActive(true);
                        itemList[i].LoadMedia(_callback.feeds[i]);
                        FeedsKeys.Add(_callback.feeds[i].Key);
                    }
                    FeedsLoaded++;
                }
                else if (_callback.forward)
                {
                    if (_callback.feeds.Count == 0)
                    {
                        ultimoPost = true;
                        itemList[1].gameObject.SetActive(false);
                        FeedsLoaded++;
                        return;
                    }
                    itemList[2].gameObject.SetActive(true);
                    itemList[1].LoadMedia(_callback.feeds[0]);
                    if (FeedsKeys.Count - 2 == FeedsLoaded)
                        FeedsKeys.Add(_callback.feeds[0].Key);
                    FeedsLoaded++;
                }
                else
                {
                    itemList[1].gameObject.SetActive(true);
                    itemList[2].LoadMedia(_callback.feeds[0]);
                    FeedsLoaded--;
                }
            }
            else
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
        }

        public string GetUserID()
        {
            if (LoaderType == FeedDataType.User)
                return CurrentUserID;
            else
                return AppManager.USER_PROFILE.FIREBASE_USER.UserId;
        }

        public enum FeedDataType
        {
            Followed,
            Profile,
            World,
            User,
            Premium,
            Reports,
            SinglePost
        }
    }

    public class FeedQuery
    {
        public string targetKey = string.Empty;
        public string indexKey = string.Empty;
        public Action<FeedCallback> callback;
        public bool forward;
        public string ownerID;
    }

    public class FeedCallback
    {
        public List<Feed> feeds;
        public bool forward;
        public bool IsSuccess;
    }

    [System.Serializable]
    public class Feed
    {
        public string BodyTXT;
        public string ImageURL;
        public string VideoURL;
        public string VideoFileName;
        public int MediaWidth;
        public int MeidaHeight;
        public string DateCreated;
        public FeedType Type;
        public string Key;
        public string OwnerID;
        public string ToUserID;
        public bool premium;
        public List<string> Report;
    }

    public enum FeedType
    {
        Text,
        Image,
        Video
    }
}