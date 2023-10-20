using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;
using System;
using Firebase.Database;
using TMPro;
using UniTools;

namespace SocialApp
{
    public class FeedViewController : MonoBehaviour
    {
        public static Feed postEliminato = new Feed()
        {
            BodyTXT = "Post not found...",
            Type = FeedType.Image,
            DateCreated = "12/2/2004 2:00 AM",
            Key = "",
            ImageURL = "https://firebasestorage.googleapis.com/v0/b/av-vault.appspot.com/o/Feeds%2FImage%2F41ca7d33-0b18-4841-a7e6-2f1fbf02aaaf.png?alt=media&token=1cd967c1-39ce-4507-9584-c0a8db07133c",
            ToUserID = "pnf",
            MediaWidth = 573,
            MeidaHeight = 499,
            OwnerID = "pnf"
        };
        public FeedsDataLoader feed;
        [SerializeField]
        private GameObject testoPan;
        [SerializeField]
        private GameObject immaginePan;
        [SerializeField]
        private GameObject videoPan;
        [SerializeField]
        private Text TextBody = default;
        [SerializeField]
        private Text TextImageBody;
        [SerializeField]
        private Text TextVideoBody;
        [SerializeField]
        public GameObject loading;
        [SerializeField]
        private Text LikesCountBody = default;
        [SerializeField]
        private Text DislikesCountBody;
        [SerializeField]
        private Text CommentsCountBody = default;
        [SerializeField]
        private Image ImageBody = default, CustomBackgroundImage;
        [SerializeField]
        private RawImage VideoBody = default;
        [SerializeField]
        private Text DateBody = default;

        [SerializeField]
        private RectTransform ProfileRect = default;
        [SerializeField]
        private RectTransform ImageRect = default;
        [SerializeField]
        private RectTransform VideoRect = default;
        [SerializeField]
        private RectTransform piùOpsioniBot = default;
        [SerializeField]
        public Text ProfileUseeNameLabel = default;
        [SerializeField]
        private VideoPlayer VPlayer = default;
        [SerializeField]
        private GameObject PlayBtn = default;
        [SerializeField]
        private AvatarViewController AvatarView = default;
        [SerializeField]
        private bool CacheAvatar = default;
        [SerializeField]
        private Color LikedPostColor = default;
        [SerializeField]
        private Color UnLikedPostColor = default;
        [SerializeField]
        private Image LikeImage = default;
        [SerializeField]
        private Image DislikeImage = default;
        public Image audioIc;

        private int doppioClick;
        private bool VideoFirstRun = true;
        private bool IsPostLiked = false;
        private bool IsPostDisliked = false;
        private bool CanLikePost = false;
        private bool CanDislikePost = false;
        public Feed LoadedFeed;

        private bool IsActiveListeners;

        private DatabaseReference DRPostLikesCount;
        private DatabaseReference DRPostDislikesCount;
        public static bool avvertito;

        void Awake()
        {
            Init();
        }

        void Start()
        {
            UpdateUIRect();
        }

        private void OnDisable()
        {
            RemoveListeners();
            ClearView();
        }

        private void Init()
        {
            //ScrollView = gameObject.GetComponentInParent<ScrollViewController>();
            HidePlayBtn();
        }

        private void AddListeners()
        {
            DRPostLikesCount = AppManager.FIREBASE_CONTROLLER.GetPostLikesCountReferense(LoadedFeed.Key);
            DRPostLikesCount.ValueChanged += OnLikesCountUpdated;
            DRPostDislikesCount = AppManager.FIREBASE_CONTROLLER.GetPostDislikesCountReferense(LoadedFeed.Key);
            DRPostDislikesCount.ValueChanged += OnDislikesCountUpdated;
            IsActiveListeners = true;
        }

        private void RemoveListeners()
        {
            StopAllCoroutines();
            if (IsActiveListeners)
            {
                if (AppManager.FIREBASE_CONTROLLER != null)
                {
                    DRPostLikesCount.ValueChanged -= OnLikesCountUpdated;
                    DRPostDislikesCount.ValueChanged -= OnDislikesCountUpdated;
                }
                IsActiveListeners = false;
            }
        }

        private void UpdateUIRect()
        {
            // update text rect
            /*.sizeDelta = new Vector2(TextRect.rect.width, TextBody.preferredHeight + TextHeightOffset);
            // update feed rect
            float _height = TextRect.rect.height + ProfileRect.rect.height + ImageRect.rect.height + VideoRect.rect.height + BottomRect.rect.height;
            MainRect.sizeDelta = new Vector2(MainRect.rect.width, _height);*/
            if (LoadedFeed != null)
                if (AppManager.IsAdmin() && AppManager.USER_PROFILE.FIREBASE_USER != null)
                    piùOpsioniBot.gameObject.SetActive(AppManager.USER_PROFILE.FIREBASE_USER.UserId != LoadedFeed.OwnerID && !AppManager.IsAdmin());
        }

        public void LoadMedia(Feed _feed)
        {
            gameObject.SetActive(true);
            AvatarView.SetCacheTexture(CacheAvatar);
            ClearView();
            LoadedFeed = _feed;
            if (_feed.Type == FeedType.Image)
            {
                ImageBody.preserveAspect = true;
                float _bodyWidth = ImageRect.rect.width;
                float _imageWidth = (float)_feed.MediaWidth;
                float _imageHeight = (float)_feed.MeidaHeight;
                float _ratio = _imageWidth / _imageHeight;
                float _expectedHeight = _bodyWidth / _ratio;
                ImageRect.sizeDelta = new Vector2(_bodyWidth, _expectedHeight);
                immaginePan.GetComponent<RectTransform>().sizeDelta = new Vector2(_bodyWidth, Mathf.Min(_expectedHeight, 944));
            }
            else if (_feed.Type == FeedType.Video)
            {
                ImageBody.preserveAspect = true;
                float _bodyWidth = VideoRect.rect.width;
                float _imageWidth = (float)_feed.MediaWidth;
                float _imageHeight = (float)_feed.MeidaHeight;

                float _expectedHeight = _imageHeight * _bodyWidth / _imageWidth;
                VideoRect.sizeDelta = new Vector2(_bodyWidth, _expectedHeight);
                videoPan.GetComponent<RectTransform>().sizeDelta = new Vector2(_bodyWidth, Mathf.Min(_expectedHeight, 944));
            }
            // load view
            LoadView();
            UpdateUIRect();
            AddListeners();
        }

        private void ClearView()
        {
            StopAllCoroutines();
            HidePlayBtn();
            ImageRect.sizeDelta = new Vector2(ImageRect.rect.width, 1f);
            VideoRect.sizeDelta = new Vector2(VideoRect.rect.width, 1f);
            TextBody.text = string.Empty;
            Destroy(ImageBody.sprite);
            ImageBody.sprite = null;
            VPlayer.Stop();
            //VPlayer.url = string.Empty;
            StopAllCoroutines();
            VideoFirstRun = true;
            LoadedFeed = null;
            AvatarView.DisplayDefaultAvatar();
            LikeImage.color = DislikeImage.color = UnLikedPostColor;
            LikesCountBody.text = "0";
            DislikesCountBody.text = "0";
            testoPan.SetActive(false);
            immaginePan.SetActive(false);
            videoPan.SetActive(false);
            loading.SetActive(true);
            ProfileRect.gameObject.SetActive(false);
            LikesCountBody.transform.parent.parent.parent.gameObject.SetActive(false);
            CommentsCountBody.text = "0";
            CustomBackgroundImage.color = new Color(1, 1, 1, 0);
            ImageBody.GetComponent<Button>().enabled = true;
            doppioClick = 0;
        }

        private void LoadView()
        {
            if (LoadedFeed.ToUserID == "pnf")
            {
                LoadPostNotFount();
                return;
            }
            LoadDate();
            LoadUserData();
            LoadUserName();
            LoadLikes();
            LoadDislikes();
            LoadComments();
            LoadCustomBackground();
            if (LoadedFeed.Type == FeedType.Text)
                LoadText();
            else if (LoadedFeed.Type == FeedType.Image)
                LoadGraphic();
            else if (LoadedFeed.Type == FeedType.Video)
                LoadVideo();
        }

        void LoadPostNotFount()
        {
            ProfileUseeNameLabel.color = Color.red;
            ProfileUseeNameLabel.text = "ERROR!";
            LoadGraphic();
            ImageBody.GetComponent<Button>().enabled = false;
            DateBody.text = "";
        }

        // load user
        private void LoadUserData()
        {
            GetProfileImage();
        }

        // load Likes
        private void LoadLikes()
        {
            CanLikePost = false;

            if (AppSettings.guest || AppManager.USER_PROFILE.FIREBASE_USER == null)
            {
                LikeImage.color = UnLikedPostColor;
                IsPostLiked = false;
                return;
            }

            AppManager.FIREBASE_CONTROLLER.IsLikedPost(LoadedFeed.Key, _isLike =>
            {
                CanLikePost = true;
                IsPostLiked = _isLike;
                if (IsPostLiked)
                {
                    LikeImage.color = LikedPostColor;
                }
                else
                {
                    LikeImage.color = UnLikedPostColor;
                }
            });
        }

        private void LoadDislikes()
        {
            CanDislikePost = false;

            if (AppSettings.guest || AppManager.USER_PROFILE.FIREBASE_USER == null)
            {
                DislikeImage.color = UnLikedPostColor;
                IsPostDisliked = false;
                return;
            }

            AppManager.FIREBASE_CONTROLLER.IsDislikedPost(LoadedFeed.Key, _isDislike =>
            {
                CanDislikePost = true;
                IsPostDisliked = _isDislike;
                if (IsPostDisliked)
                {
                    DislikeImage.color = LikedPostColor;
                }
                else
                {
                    DislikeImage.color = UnLikedPostColor;
                }
            });
        }

        // load Likes
        private void LoadComments()
        {
            AppManager.FIREBASE_CONTROLLER.GetPostCommentsCount(LoadedFeed.Key, _count =>
            {
                CommentsCountBody.text = _count.ToString();
            });
        }

        void LoadCustomBackground()
        {
            AppManager.FIREBASE_CONTROLLER.GetCustomBackgroundUrl(new GetProfileImageRequest() { Id = LoadedFeed.OwnerID, Size = ImageSize.Size_512 }, _callback =>
            {
                if (_callback.IsSuccess)
                {
                    if (_callback.DownloadUrl != null)
                    {
                        CoroutineExecuter _ce = new CoroutineExecuter();
                        ImageService _is = new ImageService(_ce);
                        _is.DownloadOrLoadTexture(_callback.DownloadUrl, _texture =>
                        {
                            if (_texture != null)
                            {
                                CustomBackgroundImage.sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                                CustomBackgroundImage.color = new Color(1, 1, 1, 1);
                                CustomBackgroundImage.preserveAspect = true;
                            }
                            else
                            {
                                CustomBackgroundImage.color = new Color(1, 1, 1, 0);
                            }
                        });
                    }
                    else
                    {
                        CustomBackgroundImage.color = new Color(1, 1, 1, 0);
                    }
                }
                else
                {
                    CustomBackgroundImage.color = new Color(1, 1, 1, 0);
                }
            });
        }

        // load date
        private void LoadDate()
        {
            DateTime data;
            DateBody.text = DateTime.TryParse(LoadedFeed.DateCreated, out data) ? (data + (DateTime.Now - DateTime.UtcNow)).ToString(AppManager.APP_SETTINGS.SystemDateFormat) : LoadedFeed.DateCreated;
        }

        // load text
        private void LoadText()
        {
            testoPan.SetActive(true);
            TextBody.text = LoadedFeed.BodyTXT;
            StartCoroutine(DimensioniTesto());
            loading.SetActive(false);
            ProfileRect.gameObject.SetActive(true);
            LikesCountBody.transform.parent.parent.parent.gameObject.SetActive(true);
            //LinksChecker.CheckLinks();
        }

        IEnumerator DimensioniTesto()
        {
            yield return null;
            yield return null;
            TextBody.transform.parent.parent.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(TextBody.transform.parent.GetComponent<RectTransform>().sizeDelta.x, Mathf.Min(TextBody.transform.parent.GetComponent<RectTransform>().sizeDelta.y, 944f));
        }

        // load image
        private void LoadGraphic()
        {
            TextImageBody.text = LoadedFeed.BodyTXT;
            StartCoroutine(OnLoadGraphic());
        }

        // load user name
        private void LoadUserName()
        {
            ProfileUseeNameLabel.color = Color.white;
            if (AppManager.USER_PROFILE.IsMine(LoadedFeed.OwnerID))
            {
                AppManager.USER_PROFILE.GetUserFullName(_userName =>
                {
                    ProfileUseeNameLabel.text = _userName;
                });
                ProfileUseeNameLabel.color = AppSettings.VaultPLUS ? Color.yellow : Color.white;
            }
            else
            {
                AppManager.FIREBASE_CONTROLLER.GetUserFullName(LoadedFeed.OwnerID, _userName =>
                {
                    ProfileUseeNameLabel.text = _userName;
                });
                AppManager.FIREBASE_CONTROLLER.CheckUserIsVaultPlus(LoadedFeed.OwnerID, b =>
                {
                    ProfileUseeNameLabel.color = b ? Color.yellow : Color.white;
                });
            }
        }

        private IEnumerator OnLoadGraphic()
        {
            string _url = LoadedFeed.ImageURL;
            if (!string.IsNullOrEmpty(_url))
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(_url);
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Texture2D _texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (_texture != null)
                    {
                        immaginePan.SetActive(true);
                        ImageBody.sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                        loading.SetActive(false);
                        ProfileRect.gameObject.SetActive(true);
                        if (LoadedFeed.ToUserID != "pnf")
                            LikesCountBody.transform.parent.parent.parent.gameObject.SetActive(true);
                        TextImageBody.transform.parent.gameObject.SetActive(TextImageBody.text != string.Empty);
                    }
                }
            }
        }

        public void SetAudio()
        {
            if (AppSettings.audioMuted)
            {
                audioIc.sprite = AppManager.VIEW_CONTROLLER.audioMutedIc;
                VPlayer.SetDirectAudioMute(0, true);
            }
            else
            {
                audioIc.sprite = AppManager.VIEW_CONTROLLER.audioIc;
                VPlayer.SetDirectAudioMute(0, false);
            }
        }

        public void MuteODismuteAudio()
        {
            AppSettings.audioMuted = !AppSettings.audioMuted;
            foreach (FeedViewController f in feed.itemList)
                f.SetAudio();
            if (AppSettings.audioMuted)
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.LightImpact);
            else
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.RigidImpact);
        }

        // load video
        private void LoadVideo()
        {
            TextVideoBody.text = LoadedFeed.BodyTXT;
            SetAudio();
            StartCoroutine(OnLoadVideo());
        }

        private IEnumerator OnLoadVideo()
        {
            string _url = LoadedFeed.VideoURL;
            print(LoadedFeed.VideoURL);
            //print(LoadedFeed.MediaWidth + "  " + LoadedFeed.MeidaHeight + "  " + _url);
            if (!string.IsNullOrEmpty(LoadedFeed.ImageURL))
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(LoadedFeed.ImageURL);
                yield return www.SendWebRequest();

                if (!(www.isNetworkError || www.isHttpError))
                {
                    videoPan.SetActive(true);
                    Texture2D _texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    VideoBody.texture = _texture;
                    loading.SetActive(false);
                    ProfileRect.gameObject.SetActive(true);
                    LikesCountBody.transform.parent.parent.parent.gameObject.SetActive(true);
                    //VideoBody.GetComponent<Button>().enabled = ;
                    TextVideoBody.transform.parent.gameObject.SetActive(TextVideoBody.text != string.Empty);
                    print("gg");
                }
            }

            if (!AppManager.APP_SETTINGS.UseOriginVideoFile)
            {
                print("lll");
                _url = string.Empty;
                bool _isGettedUrl = false;
                string _tempUrl = string.Empty;
                print("bb");
                AppManager.FIREBASE_CONTROLLER.GetFeedVideoFileUrl(LoadedFeed.VideoFileName, (_gettedUrl =>
                {
                    _isGettedUrl = true;
                    _tempUrl = _gettedUrl;
                }));
                print("d");
                while (!_isGettedUrl)
                {
                    yield return null;
                }
                _url = _tempUrl;
                print(_url);
            }

            if (!string.IsNullOrEmpty(_url))
            {
                VPlayer.url = _url;
                VPlayer.Prepare();
                while (!VPlayer.isPrepared)
                {
                    yield return null;
                }
                //ShowPlayBtn();
                StartCoroutine(AudioPlay());
            }
        }

        IEnumerator AudioPlay()
        {
            float t = 0;
            for (; ; )
            {
                if (Input.touchCount == 0 && GetComponent<RectTransform>().anchoredPosition.x == 0 && !VPlayer.isPlaying && VPlayer.isPrepared)
                {
                    if (VideoFirstRun)
                    {
                        VideoFirstRun = false;
                        VPlayer.frame = 0;
                    }
                    VideoBody.texture = VPlayer.texture;
                    VPlayer.Play();
                }
                if (Input.touchCount != 0 && VPlayer.isPlaying && VPlayer.isPrepared)
                {
                    t += Time.deltaTime;
                    if (Input.GetTouch(0).phase != TouchPhase.Began || !(t < .5f && Input.GetTouch(0).phase == TouchPhase.Stationary))
                        VPlayer.Pause();
                    if (Input.GetTouch(0).phase == TouchPhase.Ended)
                        t = 0;
                }
                if (Input.touchCount == 0 && Mathf.Abs(GetComponent<RectTransform>().anchoredPosition.x) == 762)
                    VPlayer.Stop();
                yield return new WaitForSeconds(.5f);
            }
        }

        private void GetProfileImage()
        {
            AvatarView.LoadAvatar(LoadedFeed.OwnerID);
        }



        public void OnClickVideo()
        {
            if (string.IsNullOrEmpty(VPlayer.url))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.VideoProcessing);
            }

            if (!VPlayer.isPrepared)
            {
                VPlayer.Prepare();
                return;
            }
            //StopAllCoroutines();
            StartCoroutine(CheckFeedVisibility());
            if (VPlayer.isPlaying)
            {
                VPlayer.Pause();
                ShowPlayBtn();
            }
            else
            {
                if (VideoFirstRun)
                {
                    VideoFirstRun = false;
                    VPlayer.frame = 0;
                }
                VideoBody.texture = VPlayer.texture;
                VPlayer.Play();
                HidePlayBtn();
            }
        }
        Coroutine doppioClickCor;
        public void DoppioClickLike()
        {
            if (doppioClick != -1)
                doppioClick++;
            if (doppioClick >= 2)
            {
                doppioClick = -1;
                ClickLike();
                StopCoroutine(doppioClickCor);
                print(name + gameObject.activeInHierarchy);
                StartCoroutine(AspettaDoppioClick(() => doppioClick = 0));
            }
            else
                doppioClickCor = StartCoroutine(AspettaDoppioClick(() =>
                {
                    doppioClick = 0;
                    if (LoadedFeed.Type == FeedType.Image && TextImageBody.text != string.Empty)
                        ImageBody.transform.GetChild(0).gameObject.SetActive(!ImageBody.transform.GetChild(0).gameObject.activeInHierarchy);
                    if (LoadedFeed.Type == FeedType.Video && TextVideoBody.text != string.Empty)
                        VideoBody.transform.GetChild(0).gameObject.SetActive(!VideoBody.transform.GetChild(0).gameObject.activeInHierarchy);
                    if (TextVideoBody.text != string.Empty || TextImageBody.text != string.Empty)
                        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.MediumImpact);
                }));
        }

        IEnumerator AspettaDoppioClick(Action _action)
        {
            yield return new WaitForSeconds(.3f);
            _action.Invoke();
        }

        public void ClickLike()
        {
            if (CanLikePost && CanDislikePost)
            {
                if (IsPostLiked)
                {
                    AppManager.FIREBASE_CONTROLLER.UnLikPost(LoadedFeed.Key, success =>
                    {
                        if (success)
                            AppManager.FIREBASE_CONTROLLER.RemovePostInteraction(LoadedFeed.Key, LoadedFeed.ToUserID, InteractionType.Like, success =>
                            {
                                if (success)
                                {
                                    LoadLikes();
                                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection);
                                }
                            });
                    });
                }
                else
                {
                    if (IsPostDisliked)
                        ClickDislike();
                    AppManager.FIREBASE_CONTROLLER.LikePost(LoadedFeed.Key, success =>
                    {
                        if (success)
                            AppManager.FIREBASE_CONTROLLER.AddPostInteraction(LoadedFeed.Key, LoadedFeed.ToUserID, 0, success =>
                            {
                                if (success)
                                {
                                    LoadLikes();
                                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
                                }
                            });
                    });
                }
            }
        }

        public void ClickDislike()
        {
            if (CanDislikePost && CanLikePost)
            {
                if (IsPostDisliked)
                {
                    AppManager.FIREBASE_CONTROLLER.UnDislikPost(LoadedFeed.Key, success =>
                    {
                        if (success)
                            AppManager.FIREBASE_CONTROLLER.RemovePostInteraction(LoadedFeed.Key, LoadedFeed.ToUserID, InteractionType.Dislike, success =>
                            {
                                if (success)
                                {
                                    LoadDislikes();
                                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Selection);
                                }
                            });
                    });
                }
                else
                {
                    if (IsPostLiked)
                        ClickLike();
                    AppManager.FIREBASE_CONTROLLER.DislikePost(LoadedFeed.Key, success =>
                    {
                        if (success)
                            AppManager.FIREBASE_CONTROLLER.AddPostInteraction(LoadedFeed.Key, LoadedFeed.ToUserID, InteractionType.Dislike, success =>
                            {
                                if (success)
                                {
                                    LoadDislikes();
                                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                                }
                            });
                    });
                }
            }
        }

        private void OnLikesCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                LikesCountBody.text = "0";
                return;
            }
            try
            {
                if (args.Snapshot.Value.ToString() == "0")
                {
                    LikesCountBody.text = "0";
                }
                else
                {
                    LikesCountBody.text = args.Snapshot.Value.ToString();
                }
            }
            catch (Exception)
            {
                LikesCountBody.text = "0";
            }
        }

        private void OnDislikesCountUpdated(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                DislikesCountBody.text = "0";
                return;
            }
            try
            {
                if (args.Snapshot.Value.ToString() == "0")
                {
                    DislikesCountBody.text = "0";
                }
                else
                {
                    DislikesCountBody.text = args.Snapshot.Value.ToString();
                }
            }
            catch (Exception)
            {
                DislikesCountBody.text = "0";
            }
        }

        public void ClickComments()
        {
            AppManager.VIEW_CONTROLLER.ShowPostComments(LoadedFeed.Key, LoadedFeed.OwnerID);
        }

        public void ShowUserProfile()
        {
            string _userId = LoadedFeed.OwnerID;
            if (AppManager.USER_PROFILE.IsMine(_userId))
            {
                AppManager.NAVIGATION.ShowUserProfile();
            }
            else
            {
                AppManager.VIEW_CONTROLLER.HideNavigationGroup();
                //AppManager.VIEW_CONTROLLER.ShowAnotherUserProfile(_userId);
            }
        }

        public bool IsMine()
        {
            return LoadedFeed.OwnerID == AppManager.USER_PROFILE.FIREBASE_USER.UserId;
        }

        public string GetFeedKey()
        {
            return LoadedFeed.Key;
        }

        private void ShowPlayBtn()
        {
            PlayBtn.SetActive(true);
        }

        private void HidePlayBtn()
        {
            PlayBtn.SetActive(false);
        }

        private IEnumerator CheckFeedVisibility()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (!VPlayer.isPlaying)
                {
                    ShowPlayBtn();
                }
            }
        }

        public void ShowAdditionalMenu()
        {
            AppManager.VIEW_CONTROLLER.ShowFeedPopup((actionType) =>
            {
                if (actionType == FeedPopupAction.DELETE)
                {
                    AppManager.FIREBASE_CONTROLLER.RemovePost(LoadedFeed.Key, LoadedFeed.OwnerID, () =>
                    {
                        feed.ResetLoader();
                    });
                }
                else if (actionType == FeedPopupAction.DELETE_USER)
                    AppManager.FIREBASE_CONTROLLER.DeleteUserDada(LoadedFeed.OwnerID);
                else if (actionType == FeedPopupAction.PREMIUM)
                    AppManager.FIREBASE_CONTROLLER.PostInMainFeed(LoadedFeed.Key);
                else if (actionType == FeedPopupAction.BLOCK_USER)
                    AppManager.FIREBASE_CONTROLLER.BlockUser(LoadedFeed.ToUserID);
                else if (actionType == FeedPopupAction.REPORT_POST)
                    AppManager.FIREBASE_CONTROLLER.ReportPost(LoadedFeed.Key);
                else if (actionType == FeedPopupAction.SAVE_POST)
                {
                    if (LoadedFeed.Type == FeedType.Image)
                        AppManager.VIEW_CONTROLLER.SavePost(ImageBody.sprite);
                    else if (LoadedFeed.Type == FeedType.Video)
                    {/*
#if UNITY_IOS && !UNITY_EDITOR
                        if (!UnityEngine.Apple.ReplayKit.ReplayKit.APIAvailable)
                        {
                            AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "It's not possible save the video." });
                            return;
                        }
#endif
                        VideoPlayer vid = AppManager.VIEW_CONTROLLER.videoPreview;
                        vid.transform.parent.parent.gameObject.SetActive(true);
                        vid.transform.parent.parent.GetChild(1).gameObject.SetActive(true);
                        vid.transform.parent.parent.GetChild(1).GetChild(0).GetChild(2).gameObject.SetActive(Application.platform == RuntimePlatform.Android);
                        vid.GetComponent<RawImage>().texture = VideoBody.texture;
                        float alt = 1356f;
                        float larg = VideoBody.texture.width * 1356f / VideoBody.texture.height;
                        if (larg > 760f)
                        {
                            alt = VideoBody.texture.height * 760f / VideoBody.texture.width;
                            larg = 760f;
                        }
                        vid.transform.parent.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(larg / 2, alt / 2);
                        vid.GetComponent<RectTransform>().sizeDelta = new Vector2(larg, alt);
                        vid.url = VPlayer.url;
                        vid.Prepare();
                        StartCoroutine(ScattaScreenShot(() => !vid.isPrepared, () =>
                        {
                            StartCoroutine(ScattaScreenShot(() => !avvertito, () =>
                            {
                                avvertito = false;
                                AppManager.VIEW_CONTROLLER.SaveVideo();
                                vid.GetComponent<RawImage>().texture = vid.texture;
                            }));
                        }));*/
                        AppManager.VIEW_CONTROLLER.ShowLoading();
                        AppManager.FIREBASE_CONTROLLER.AddWatermarkToVideo(LoadedFeed, bytes =>
                        {
                            if (bytes == null)
                            {
                                AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error!", Message = "There was an error saving the video.\nTry again. " });
                                AppManager.VIEW_CONTROLLER.HideLoading();
                            }
                            else
                                NativeGallery.SaveVideoToGallery(bytes, "AV: Vault", "meme_of_avvault.mp4", (success, path) =>
                                {
                                    if (success)
                                        AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Meme saved!", Message = "The saving operation was successful." });
                                    else
                                        AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error!", Message = "There was an error saving the video.\nTry again. " + path });
                                    AppManager.VIEW_CONTROLLER.HideLoading();
                                });
                        });
                    }
                    else
                        AppManager.VIEW_CONTROLLER.CopyText(LoadedFeed.BodyTXT, "\n\nFrom AV: Vault, the best app for memes");
                }
            });
        }

        IEnumerator ScattaScreenShot(Func<bool> func, Action _action)
        {
            yield return new WaitWhile(func);
            _action.Invoke();
        }

        public void Avvertito() => avvertito = true;
        public void IngrandisciFoto(Image img)
        {
            img.sprite = ImageBody.sprite;
        }

        public void ApriCommentoBt() => AppManager.VIEW_CONTROLLER.ApriCommentoBt(this);

        /*public void DeleteUserAsync(string _uid)
        {
            Firebase.Auth.FirebaseUser user = auth.CurrentUser;
            if (user != null)
            {
                user.DeleteAsync().ContinueWith(task => {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("DeleteAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
                        return;
                    }

                    Debug.Log("User deleted successfully.");
                });
            }
        }*/

        public void ShowUserInfo()
        {
            if (LoadedFeed.OwnerID != "pnf")
                AppManager.USER_INFO.ShowUserInfo(LoadedFeed.OwnerID);
        }
    }
}