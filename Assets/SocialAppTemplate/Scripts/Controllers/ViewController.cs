using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Video;
using Lofelt.NiceVibrations;
namespace SocialApp
{
    public class ViewController : MonoBehaviour
    {
        public GameObject reportsBt;
        public List<GameObject> postWorld, postMain, postReported, postProfile, postUserFeed, postFollowedFeed;
        public Texture2D watermarkTexture;
        [SerializeField]
        private Camera MainCamera = default;
        [SerializeField]
        public GameObject condividiObject, VaultPLUSFeaturesObject;
        [SerializeField]
        public GameObject errorObject;
        [SerializeField]
        public GameObject InteractionsObject, vaultPlusObject, moderatorBt;
        [SerializeField]
        public GameObject UserInfoObject;
        [SerializeField]
        private GameObject LoadingScreen = default, EliminaCommentiObject, SostituisciNomeAccountObject;
        [SerializeField]
        private GameObject PopupObject = default;
        [SerializeField]
        private GameObject ExitGuestObject;
        [SerializeField]
        private GameObject RegistrationObject = default;
        public GameObject UserProfileObject = default;
        [SerializeField]
        public GameObject WorldNewsObject = default;
        [SerializeField]
        private GameObject FollowedFeedObject;
        [SerializeField]
        private GameObject UserFeedObject;
        [SerializeField]
        private GameObject reportsFeedObject;
        [SerializeField]
        private GameObject MainNewsObject = default;
        [SerializeField]
        public GameObject SinglePostObject;
        [SerializeField]
        private GameObject ProfileFeedObject;
        [SerializeField]
        private GameObject FriendsNewsObject = default;
        [SerializeField]
        public GameObject primaPaginaObject = default;
        [SerializeField]
        private GameObject LoginObject = default;
        [SerializeField]
        private GameObject FeedPreviewImageObject = default;
        [SerializeField]
        private GameObject FeedPreviewVideoObject = default;
        [SerializeField]
        private GameObject FriendListObject = default;
        [SerializeField]
        private GameObject NavigationPanelObject = default;
        [SerializeField]
        private GameObject CommentsObject = default;
        [SerializeField]
        private GameObject FeedPopupObject = default, FeedPopupMoteratoreObject;
        [SerializeField]
        private GameObject DeletedAccountObject = default;
        public Sprite audioIc, audioMutedIc;

        // camera
        public Camera GetMainCamera()
        {
            return MainCamera;
        }

        // popup
        public void ShowPopupMessage(PopupMessage _msg)
        {
            PopupObject.SetActive(true);
            PopupObject.GetComponent<PopupController>().ShowMessage(_msg);
            PopupObject.GetComponent<Animator>().SetBool("avvia", true);
        }

        public void HidePopupMessage()
        {
            StartCoroutine(DisattivaOggetto(PopupObject));
            PopupObject.GetComponent<Animator>().SetBool("avvia", false);
        }

        IEnumerator DisattivaOggetto(GameObject a)
        {
            yield return new WaitForSeconds(1);
            a.SetActive(false);
        }
        // loading
        public void ShowLoading()
        {
            LoadingScreen.SetActive(true);
        }

        public void HideLoading()
        {
            LoadingScreen.SetActive(false);
        }

        public void ShowErrorScreen()
        {
            errorObject.SetActive(true);
        }

        public void HideErrorScreen()
        {
            errorObject.SetActive(false);
        }

        // registration
        public void ShowRegistration()
        {
            RegistrationObject.SetActive(true);
        }

        public void HideRegistration()
        {
            RegistrationObject.SetActive(false);
        }

        // login
        public void ShowLogin()
        {
            LoginObject.SetActive(true);
        }

        public void HideLogin()
        {
            LoginObject.SetActive(false);
        }

        public void ShowPrima()
        {
            primaPaginaObject.SetActive(true);
        }

        public void HidePrima()
        {
            primaPaginaObject.SetActive(false);
        }

        public void ShowExitByGuest()
        {
            ExitGuestObject.SetActive(true);
        }

        public void HideExitByGuest()
        {
            ExitGuestObject.SetActive(false);
        }

        // user profile
        public void ShowUserProfile()
        {
            if (AppManager.IsAdmin())
                reportsBt.SetActive(true);
            if (!AppSettings.guest)
                UserProfileObject.SetActive(true);
            else
            {
                ShowExitByGuest();
                UserProfileObject.SetActive(false);
            }
        }

        public void HideUserProfile()
        {
            UserProfileObject.SetActive(false);
        }

        public void ShowUserPosts(string id)
        {
            UserFeedObject.GetComponent<FeedsDataLoader>().CurrentUserID = id;
            UserFeedObject.SetActive(true);
        }

        public void HideUserPosts(string id)
        {
            UserFeedObject.SetActive(false);
        }

        public void ShowDeletedAccount()
        {
            Debug.Log("chiamato");
            DeletedAccountObject.SetActive(true);
            Debug.Log("funziona");
        }

        // feed preview
        public void ShowFeedPreview(FeedPreviewRequest _request, FeedType _type)
        {
            if (_type == FeedType.Image)
            {
                FeedPreviewImageObject.SetActive(true);
                FeedPreviewImageObject.GetComponent<FeedPreviewController>().DisplayPreview(_request);
            }
            else
            {
                FeedPreviewVideoObject.SetActive(true);
                FeedPreviewVideoObject.GetComponent<FeedPreviewController>().DisplayPreview(_request);
            }
        }

        public void HideFeedPreview()
        {
            FeedPreviewImageObject.SetActive(false);
            FeedPreviewVideoObject.SetActive(false);
        }

        // navigation
        public void ShowNavigationPanel()
        {
            NavigationPanelObject.SetActive(true);
            AppManager.NAVIGATION.AddListeners();
        }

        public void HideNavigationPanel()
        {
            NavigationPanelObject.SetActive(false);
            AppManager.NAVIGATION.RemoveListeners();
        }

        // world news
        public void ShowWorldNews()
        {
            WorldNewsObject.SetActive(true);
        }

        public void HideWorldNews()
        {
            WorldNewsObject.SetActive(false);
        }

        public void SwitchOrderFeedWorldNew()
        {
            FeedsDataLoader.OrderFeed ordine = WorldNewsObject.GetComponent<FeedsDataLoader>().ordine;
            ordine = ordine == FeedsDataLoader.OrderFeed.cronologico ? FeedsDataLoader.OrderFeed.casuale : FeedsDataLoader.OrderFeed.cronologico;
            WorldNewsObject.GetComponent<FeedsDataLoader>().ordine = ordine;
            PlayerPrefs.SetInt("Order world feed", (int)ordine);
            ShowWorldNews();
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
        }

        public void HideMainFeed()
        {
            MainNewsObject.SetActive(false);
        }

        public void HideUserInfo()
        {
            UserInfoObject.SetActive(false);
        }

        // main feed
        public void ShowMainNews()
        {
            MainNewsObject.SetActive(true);
        }

        public void ShowFollowedFeed()
        {
            FollowedFeedObject.SetActive(true);
        }

        public void HideFollowedFeed()
        {
            FollowedFeedObject.SetActive(false);
        }

        public void ShowReportFeed()
        {
            reportsFeedObject.SetActive(true);
            HideUserProfile();
        }

        public void HideReportFeed()
        {
            reportsFeedObject.SetActive(false);
            ShowUserProfile();
        }

        public void PurchaseSuccess(UnityEngine.Purchasing.Product product)
        {
            string giorni = "";
            int n = 0;
            List<string> l = new List<string>() { "7", "1", "3" };
            for (int i = 0; i < l.Count; i++)
            {
                if (product.metadata.localizedTitle.Contains(l[i]))
                    switch (l[i])
                    {
                        case "7":
                            giorni = "7 days";
                            n = 7;
                            break;
                        case "1":
                            giorni = "1 month";
                            n = 30;
                            break;
                        case "3":
                            giorni = "3 months";
                            n = 90;
                            break;
                    }
            }
            if (giorni == "")
            {
                giorni = "infinity time";
                n = -1;
            }
            AppManager.FIREBASE_CONTROLLER.SetSubscriptionVaultPLUS(n, () =>
            {
                ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "Thank you for purchasing Vault PLUS for " + giorni + "." });
                ShowInfoVaultPLUS();
                AppManager.VIEW_CONTROLLER.HideLoading();
                UserProfileObject.GetComponent<UserProfileLoader>().OnEnable();
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
            });
        }

        public void PurchaseFailled(UnityEngine.Purchasing.Product product, UnityEngine.Purchasing.PurchaseFailureReason reason)
        {
            ShowPopupMessage(new PopupMessage() { Title = "Error!", Message = reason.ToString() });
            AppManager.VIEW_CONTROLLER.HideLoading();
        }

        public void ShowPurcaseVaultPLUS()
        {
            vaultPlusObject.SetActive(true);
            vaultPlusObject.transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
            vaultPlusObject.transform.GetChild(1).GetChild(3).gameObject.SetActive(true);
            vaultPlusObject.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(false);
            vaultPlusObject.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(true);
            UserProfileObject.transform.GetChild(10).gameObject.SetActive(false);
            HideNavigationPanel();
        }

        public void ShowInfoVaultPLUS()
        {
            UserProfileObject.transform.GetChild(10).gameObject.SetActive(false);
            HideNavigationPanel();
            vaultPlusObject.SetActive(true);
            vaultPlusObject.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
            vaultPlusObject.transform.GetChild(1).GetChild(3).gameObject.SetActive(false);
            vaultPlusObject.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(0).gameObject.SetActive(true);
            vaultPlusObject.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(false);
            vaultPlusObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(AppSettings.VaultPLUS);
            vaultPlusObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(3).gameObject.SetActive(!AppSettings.VaultPLUS);
            if (AppSettings.GiorniRimastiVaultPLUS != -1)
            {
                int n = 0;
                string t = "";
                if (AppSettings.GiorniRimastiVaultPLUS > 30)
                {
                    n = (int)Mathf.Round(AppSettings.GiorniRimastiVaultPLUS / 30);
                    if (n > 1)
                        t = "Months";
                    else
                        t = "Month";
                }
                else
                {
                    if (AppSettings.GiorniRimastiVaultPLUS >= 7)
                    {
                        n = (int)Mathf.Round(AppSettings.GiorniRimastiVaultPLUS / 7);
                        if (n > 1)
                            t = "Weeks";
                        else
                            t = "Week";
                    }
                    else
                    {
                        n = AppSettings.GiorniRimastiVaultPLUS;
                        if (n > 1)
                            t = "Days";
                        else
                            t = "Day";
                    }
                }
                vaultPlusObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = n + "<i><size=5>" + t + "</size></i>";
                Color color = new Color();
                if (AppSettings.GiorniRimastiVaultPLUS <= 2)
                    color = new Color(1, .5f, 0, 1);
                else
                    color = Color.yellow;
                if (AppSettings.GiorniRimastiVaultPLUS == 0)
                    color = Color.red;
                vaultPlusObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().color = color;
            }
            else
            {
                vaultPlusObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().text = "FOR EVER";
                vaultPlusObject.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<Text>().color = Color.green;
            }
        }

        public void HidePanelVaultPLUS()
        {
            vaultPlusObject.SetActive(false);
        }

        public void ShowVautPLUSFeatures()
        {
            if (!AppSettings.VaultPLUS)
                ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "You must be a subscriber to use the Vault PLUS features.", Callback = () => ShowPurcaseVaultPLUS() });
            else
            {
                VaultPLUSFeaturesObject.SetActive(true);
                UserProfileObject.transform.GetChild(10).gameObject.SetActive(false);
                HideNavigationPanel();
            }
        }

        // friends news
        public void ShowFriendsNews()
        {
            FriendsNewsObject.SetActive(true);
        }

        public void HideFriendsNews()
        {
            //FriendsNewsObject.SetActive(false);
        }

        public void ShowInteractions()
        {
            InteractionsObject.SetActive(true);
            UpdateInteractions(0);
        }

        public void HideInteractions()
        {
            InteractionsObject.SetActive(false);
        }

        public void UpdateInteractions(int count)
        {
            AppManager.FIREBASE_CONTROLLER.SetInteractionsCount(count);
        }

        public void ShowInteractionsLoading()
        {
            InteractionsObject.transform.GetChild(1).GetChild(0).GetChild(3).gameObject.SetActive(true);
        }

        public void HideInteractionsLoading()
        {
            InteractionsObject.transform.GetChild(1).GetChild(0).GetChild(3).gameObject.SetActive(false);
        }

        public void ShowPostCommentsLoading()
        {
            CommentsObject.transform.GetChild(1).GetChild(0).GetChild(4).gameObject.SetActive(true);
        }

        public void HidePostCommentsLoading()
        {
            CommentsObject.transform.GetChild(1).GetChild(0).GetChild(4).gameObject.SetActive(false);
        }

        // show post comments
        public void ShowPostComments(string _id, string _ownerId)
        {
            HideNavigationPanel();
            CommentsObject.SetActive(true);
            CommentsObject.GetComponent<MessagesDataLoader>().LoadPostComments(_id, _ownerId, true);
        }

        public void HidePostComments()
        {
            CommentsObject.SetActive(false);
        }

        public void OpenLink(string link) => Application.OpenURL(link);

        public void ShowSinglePost()
        {
            SinglePostObject.SetActive(true);
        }

        public void HideSinglePost()
        {
            SinglePostObject.SetActive(false);
        }

        public void ShowShareApp()
        {
            condividiObject.SetActive(true);
        }

        public void Share()
        {
            new NativeShare().SetText("I recently found this new app for looking at memes.\nIt's really funny.\nDownload it and post memes with me!\n https://avvaultsocial.page.link/Download").SetCallback((risultato, a) => { if (risultato != NativeShare.ShareResult.Unknown) HideShareApp(); }).Share();
        }

        public void HideShareApp()
        {
            condividiObject.SetActive(false);
        }

        public void ShowFeedPopup(Action<FeedPopupAction> _action)
        {
            FeedPopupObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(!(AppSettings.guest || AppManager.USER_PROFILE.FIREBASE_USER == null));
            FeedPopupObject.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(!(AppSettings.guest || AppManager.USER_PROFILE.FIREBASE_USER == null));
            GameObject a = FeedPopupObject;
            if (AppManager.IsAdmin())
                a = FeedPopupMoteratoreObject;
            a.SetActive(true);
            if (!AppManager.IsAdmin())
                FeedPopupObject.GetComponent<FeedPopupViewController>().SetupWindows(_action);
            else
                FeedPopupMoteratoreObject.GetComponent<FeedPopupViewController>().SetupWindows(_action);
            FeedPopupObject.GetComponentInChildren<Animator>().SetBool("e", true);
        }

        public void HideFeedPopup()
        {
            FeedPopupObject.GetComponentInChildren<Animator>().SetBool("e", false);
            FeedPopupMoteratoreObject.SetActive(false);
            StartCoroutine(Disattiva());
        }

        IEnumerator Disattiva()
        {
            yield return new WaitForSeconds(.5f);
            FeedPopupObject.SetActive(false);
        }

        // all
        public void HideAllScreen()
        {
            HidePrima();
            HideLogin();
            HidePopupMessage();
            HideRegistration();
            HideLoading();
            HideFeedPreview();
            HideUserProfile();
            HideNavigationPanel();
            HideWorldNews();
            HideFriendsNews();
            HidePostComments();
            HideFeedPopup();
            HideExitByGuest();
            HideMainFeed();
            HideErrorScreen();
            HideSinglePost();
            HideUserInfo();
            HideFollowedFeed();
            reportsFeedObject.SetActive(false);
        }

        // hide navigation group objects
        public void HideNavigationGroup()
        {
            HideUserInfo();
            HideUserProfile();
            HideWorldNews();
            HideMainFeed();
            HideFriendsNews();
            HidePostComments();
            HideExitByGuest();
            HideErrorScreen();
            HideFollowedFeed();
        }

        public void ShowPopupMSG(MessageCode _code, Action _callback = null)
        {
            PopupMessage msg = new PopupMessage();
            msg.Callback = _callback;
            switch (_code)
            {
                case MessageCode.EmptyEmail:
                    msg.Title = "Error";
                    msg.Message = "Email is empty";
                    break;
                case MessageCode.EmptyFirstName:
                    msg.Title = "Error";
                    msg.Message = "First Name is empty";
                    break;
                case MessageCode.EmptyLastName:
                    msg.Title = "Error";
                    msg.Message = "Last Name is empty";
                    break;
                case MessageCode.EmptyPassword:
                    msg.Title = "Error";
                    msg.Message = "Password is empty";
                    break;
                case MessageCode.PasswordNotMatch:
                    msg.Title = "Error";
                    msg.Message = "Passwords do not match";
                    break;
                case MessageCode.EmailNotValid:
                    msg.Title = "Error";
                    msg.Message = "Email is not valid";
                    break;
                case MessageCode.SmallPassword:
                    msg.Title = "Error";
                    msg.Message = "Password is too small. Min value is " + AppManager.APP_SETTINGS.MinAllowPasswordCharacters.ToString();
                    break;
                case MessageCode.RegistrationSuccess:
                    msg.Title = "Success";
                    msg.Message = "Registration Success!";
                    break;
                case MessageCode.RegistrationSuccessWithConfirm:
                    msg.Title = "Success";
                    msg.Message = "Registration Success! Please confirm your email address";
                    break;
                case MessageCode.VideoProcessing:
                    msg.Title = "Error";
                    msg.Message = "Video processing ...";
                    break;
                case MessageCode.MaxVideoSize:
                    msg.Title = "Error";
                    msg.Message = "Max allowed size is " + AppManager.APP_SETTINGS.MaxUploadVideoSizeMB.ToString() + " mb";
                    break;
                case MessageCode.FailedUploadFeed:
                    msg.Title = "Error";
                    msg.Message = "Fail to upload feed. Try again";
                    break;
                case MessageCode.EmailConfirm:
                    msg.Title = "Error";
                    msg.Message = "An email has been sent to your address to verify it. Click the link and come back here to log in.";
                    break;
                case MessageCode.FailedUploadImage:
                    msg.Title = "Error";
                    msg.Message = "Fail to upload image. Try again";
                    break;
                case MessageCode.SuccessPost:
                    msg.Title = "Success";
                    msg.Message = "Post add success";
                    break;
                case MessageCode.DeleteFeedOwnerError:
                    msg.Title = "Error";
                    msg.Message = "You are not the owner of this post";
                    break;
                case MessageCode.CallIsBisy:
                    msg.Title = "Line is bisy";
                    msg.Message = "User cannot speak now";
                    break;
                case MessageCode.MainFeed:
                    msg.Title = "This is the main feed";
                    msg.Message = "contact altavelos.business@gmail.com to have your post posted here";
                    break;
                default:
                    Debug.Log("NOTHING");
                    break;
                case MessageCode.PostFailed:
                    msg.Title = "You used forbidden words";
                    msg.Message = "Your message contained potentially offensive and derogatory words.  Try rewriting the post without these words.";
                    break;
                case MessageCode.UpdateEmailFailed:
                    msg.Title = "Error";
                    msg.Message = "There was a problem updating the email.\nMake sure this new email is not already used for other accounts.";
                    break;
                case MessageCode.CurrentPasswordIsIncorrect:
                    msg.Title = "Error";
                    msg.Message = "The password is wrong.\nEnter the current password in the field provided.";
                    break;
            }
            ShowPopupMessage(msg);
        }

        public void CopyText(string s, string s1)
        {
            TextEditor te = new TextEditor();
            te.text = s + s1;
            te.SelectAll();
            te.Copy();
            ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "The text was copied successfully!" });
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
        }

        public void SavePost(Sprite sprite)
        {
            ShowLoading();
            var texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
            texture.SetPixels(pixels);
            
            Texture2D _watermarkTexture = new Texture2D(watermarkTexture.width, watermarkTexture.height);
            _watermarkTexture.SetPixels(watermarkTexture.GetPixels());
            int larg = (int)(texture.width / 1.5f);
            //TextureScale.Bilinear(_watermarkTexture, larg, larg * _watermarkTexture.height / _watermarkTexture.width);
            TextureScaler.scale(_watermarkTexture, larg, larg * _watermarkTexture.height / _watermarkTexture.width);
            
            int watermarkWidth = _watermarkTexture.width;
            int watermarkHeight = _watermarkTexture.height;
            int startx = texture.width - watermarkWidth - 10;
            int starty = texture.height - watermarkHeight - 10;

            Color[] watermarkPixels = _watermarkTexture.GetPixels();
            Color[] originalPixels = texture.GetPixels(startx, starty, watermarkWidth, watermarkHeight);
            Color c = watermarkPixels[0];
            for (int i = 0; i < watermarkPixels.Length; i++)
            {
                if (watermarkPixels[i] != c)
                    originalPixels[i] = watermarkPixels[i];
                else
                    originalPixels[i] += new Color(.25f, .25f, .25f, 1);
            }
            texture.SetPixels(startx, starty, watermarkWidth, watermarkHeight, originalPixels);
            texture.Apply();
            
            NativeGallery.SaveImageToGallery(texture, "AV: Vault", "meme_of_avvault.png", (success, path) =>
            {
                if (success)
                {
                    ShowPopupMessage(new PopupMessage() { Title = "Meme saved!", Message = "The saving operation was successful." });
                    HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
                }
                else
                {
                    ShowPopupMessage(new PopupMessage() { Title = "Error!", Message = "There was an error saving the image.\nTry again. " + success });
                    HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
                }
                HideLoading();
            });
            /*
            textureEsempio.transform.parent.gameObject.SetActive(true);
            textureEsempio.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            textureEsempio.preserveAspect = true;*/
        }
        public VideoPlayer videoPreview;
        /*
        public void SaveVideo()
        {
#if UNITY_ANDROID
            SmileSoftScreenRecordController.instance.SetGalleryAddingCapabilities(true);
            SmileSoftScreenRecordController.instance.StartRecording();
            videoPreview.Play();
            StartCoroutine(AspettaFinoAQuando(() => videoPreview.isPlaying, () =>
            {
                SmileSoftScreenRecordController.instance.StopRecording();
                videoPreview.transform.parent.parent.gameObject.SetActive(false);
                /*ShowLoading();
                StartCoroutine(AspettaFinoAQuando(() => !UnityEngine.Apple.ReplayKit.ReplayKit.recordingAvailable, () =>
                {
                    HideLoading();
                    if (!UnityEngine.Apple.ReplayKit.ReplayKit.Preview())
                    {
                        ShowPopupMessage(new PopupMessage() { Title = "Error", Message = UnityEngine.Apple.ReplayKit.ReplayKit.lastError });
                        return;
                    }
                    else
                        ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "The video was successfully saved!" });
                }));
            }));

            /*
            AppManager.ANDROID_UTILS_CONTROLLER.onErrorRecord = () =>
            {
                videoPreview.transform.parent.parent.gameObject.SetActive(false);
                ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "There was a problem capturing the video." });
            };
            AppManager.ANDROID_UTILS_CONTROLLER.onStopRecord = () =>
            {
                videoPreview.transform.parent.parent.gameObject.SetActive(false);
                ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "The video was successfully saved!" });
            };
            AppManager.ANDROID_UTILS_CONTROLLER.onStartRecord = () =>
            {
                StartCoroutine(AspettaFinoAQuando(() => videoPreview.isPlaying, () => AppManager.ANDROID_UTILS_CONTROLLER.StopRecording()));
            };
            AppManager.ANDROID_UTILS_CONTROLLER.StartRecording();
            print("aa");
        if (!AndroidUtils.IsPermitted(AndroidPermission.RECORD_AUDIO))//RECORD_AUDIO is declared inside plugin manifest but we need to request it manualy
        {
            AndroidUtils.RequestPermission(AndroidPermission.RECORD_AUDIO);
            onAllowCallback = () =>
            {
                androidRecorder.Call("startRecording");
                StartCoroutine(AspettaFinoAQuando(() => videoPreview.isPlaying, () =>
                {
                    androidRecorder.Call("stopRecording");
                    videoPreview.transform.parent.parent.gameObject.SetActive(false);
                }));
            };
            onDenyCallback = () => { ShowToast("Need RECORD_AUDIO permission to record voice");};
            onDenyAndNeverAskAgainCallback = () => { ShowToast("Need RECORD_AUDIO permission to record voice");};
        }
        else
        {
            androidRecorder.Call("startRecording");
            StartCoroutine(AspettaFinoAQuando(() => videoPreview.isPlaying, () =>
            {
                androidRecorder.Call("stopRecording");
                videoPreview.transform.parent.parent.gameObject.SetActive(false);
                ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "The video was successfully saved!" });
            }));
        }
#else
            videoPreview.Play();
            if (UnityEngine.Apple.ReplayKit.ReplayKit.APIAvailable)
            {
                if (!UnityEngine.Apple.ReplayKit.ReplayKit.StartRecording(false, false))
                {
                    videoPreview.Stop();
                    videoPreview.transform.parent.parent.gameObject.SetActive(false);
                    ShowPopupMessage(new PopupMessage() { Title = "Error", Message = UnityEngine.Apple.ReplayKit.ReplayKit.lastError });
                    return;
                }
                StartCoroutine(AspettaFinoAQuando(() => videoPreview.isPlaying, () =>
                {
                    UnityEngine.Apple.ReplayKit.ReplayKit.StopRecording();
                    videoPreview.transform.parent.parent.gameObject.SetActive(false);
                    ShowLoading();
                    StartCoroutine(AspettaFinoAQuando(() => !UnityEngine.Apple.ReplayKit.ReplayKit.recordingAvailable, () =>
                    {
                        HideLoading();
                        if (!UnityEngine.Apple.ReplayKit.ReplayKit.Preview())
                        {
                            ShowPopupMessage(new PopupMessage() { Title = "Error", Message = UnityEngine.Apple.ReplayKit.ReplayKit.lastError });
                            return;
                        }
                        else
                            ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "The video was successfully saved!" });
                    }));
                }));
            }
            else
                ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "It's not possible to save the video on your device." });
#endif
            /*else
            {
                ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "It's not possible save the video." });
                videoPreview.transform.parent.parent.gameObject.SetActive(false);
            }
        }
*/
        IEnumerator AspettaFinoAQuando(Func<bool> func, Action action)
        {
            yield return new WaitForSeconds(1);
            yield return new WaitWhile(() => func());
            action.Invoke();
        }

        IEnumerator Transizione(GameObject a, GameObject b, GameObject c, float inizio = 0, int verso = 1, bool feed = false, bool attivaUltimo = true)
        {
            /*b.SetActive(true);
            if (!attivaUltimo)
            {
                if (verso == 1)
                    b.SetActive(false);
                else
                    a.SetActive(false);
            }
            //print(inizio);
            if (!feed)
                a.transform.GetChild(a.transform.childCount - 1).gameObject.SetActive(true);*/
            for (float i = inizio; verso == 1 ? i <= 1.1f : i >= -0.1f; i += .03f * verso)
            {
                StatoTransizione(i, a, b, c, feed);
                yield return new WaitForSeconds(.005f);
            }/*
            if (!attivaUltimo)
            {
                if (verso == 1)
                    b.SetActive(false);
                else
                    a.SetActive(false);
            }
            if (!feed)
            {
                if (verso == 1)
                {
                    aOg = a;
                    bOg = b;
                }
                else
                {
                    aOg = b;
                    bOg = a;
                }
                aOg.transform.GetChild(aOg.transform.childCount - 1).gameObject.SetActive(false);
                bOg.transform.GetChild(bOg.transform.childCount - 1).gameObject.SetActive(false);
                aOg.SetActive(false);
                bOg.SetActive(true);
            }*/
        }

        IEnumerator TransizioneVersoAlto(GameObject a, GameObject b, float inizio, int verso, bool aper, bool feed = false)
        {
            for (float i = inizio; verso == 1 ? i <= 1.1f : i >= -0.1f; i += .03f * verso)
            {
                if (feed)
                {
                    if (!apertoMioFeed)
                        StatoTransizione(i, a, b);
                    else
                        StatoTransizioneScendi(i, a, b);
                }
                else
                {
                    if (!aperto)
                        StatoTransizione(i, a, b);
                    else
                        StatoTransizioneScendi(i, a, b);
                }
                yield return new WaitForSeconds(.005f);
            }
            if (!feed)
            {
                if (aperto && !aper)
                    CommentsObject.GetComponent<MessagesDataLoader>().primo = false;
                if (aperto == !aper)
                    AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
                aperto = aper;
                CommentsObject.SetActive(aper);
            }
            else
            {
                if (apertoMioFeed == !aper)
                    AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
                apertoMioFeed = aper;
                ProfileFeedObject.SetActive(aper);
            }
        }

        void StatoTransizione(float i, GameObject a, GameObject b)
        {
            b.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, (i - .5f) * 2));
            a.GetComponent<RectTransform>().anchoredPosition = new Vector2(a.GetComponent<RectTransform>().anchoredPosition.x, Mathf.Lerp(-1356, 1356, i) - 1356);
        }

        void StatoTransizioneScendi(float i, GameObject a, GameObject b)
        {
            b.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, i));
            a.GetComponent<RectTransform>().anchoredPosition = new Vector2(a.GetComponent<RectTransform>().anchoredPosition.x, Mathf.Lerp(-1356, 0, i));
        }

        void StatoTransizione(float i, GameObject a, GameObject b, GameObject c, bool feed)
        {
            if (feed)
            {
                c.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(-763, 763, i) + 763, c.GetComponent<RectTransform>().anchoredPosition.y);
                b.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(-763, 763, i), b.GetComponent<RectTransform>().anchoredPosition.y);
                a.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(-763, 763, i) - 763, a.GetComponent<RectTransform>().anchoredPosition.y);
            }
            else
            {
                a.transform.GetChild(a.transform.childCount - 1).GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0, .7f, i));
                a.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(Mathf.Lerp(0, -382, i), 0, 0);
                b.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(Mathf.Lerp(0, 763, 1f - i), 0, 0);
            }
        }
        bool cambiaPost, aperto, apertoMioFeed, barraPremuta;
        float tempo = 0;
        Vector2 direz, posIniz;

        public void BarraPremuta(bool b) => barraPremuta = b;

        public void AttivaOggetto(RectTransform pan) => pan.anchoredPosition = Vector2.zero;

        void Update()
        {
            if (barraPremuta)
                return;
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0))//Input.touchCount > 0)
            {
                if ((WorldNewsObject.activeInHierarchy || MainNewsObject.activeInHierarchy || FollowedFeedObject.activeInHierarchy || reportsFeedObject.activeInHierarchy || ProfileFeedObject.activeInHierarchy || UserProfileObject.activeInHierarchy || SinglePostObject.activeInHierarchy) && !FeedPopupObject.activeInHierarchy && !FeedPopupMoteratoreObject.activeInHierarchy && !EliminaCommentiObject.activeInHierarchy && !SostituisciNomeAccountObject.activeInHierarchy)
                {
                    GameObject oggetto = null;
                    List<GameObject> posts = new List<GameObject>();
                    if (UserFeedObject.activeInHierarchy)
                        oggetto = UserFeedObject;
                    else if (WorldNewsObject.activeInHierarchy)
                        oggetto = WorldNewsObject;
                    else if (MainNewsObject.activeInHierarchy)
                        oggetto = MainNewsObject;
                    else if (FollowedFeedObject.activeInHierarchy)
                        oggetto = FollowedFeedObject;
                    else if (reportsFeedObject.activeInHierarchy)
                        oggetto = reportsFeedObject;
                    else if (ProfileFeedObject.activeInHierarchy || UserProfileObject.activeInHierarchy)
                        oggetto = ProfileFeedObject;
                    else if (SinglePostObject.activeInHierarchy)
                        oggetto = SinglePostObject;
                        
                    posts.Add(oggetto.GetComponent<FeedsDataLoader>().itemList[0].gameObject);
                    posts.Add(oggetto.GetComponent<FeedsDataLoader>().itemList[2].gameObject);
                    posts.Add(oggetto.GetComponent<FeedsDataLoader>().itemList[1].gameObject);

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        tempo = Time.time;
                        cambiaPost = true;
                        posIniz = Input.mousePosition;
                        direz = Vector2.zero;
                    }

                    if (cambiaPost)
                    {
                        if (Input.GetKey(KeyCode.Mouse0))
                        {
                            if (Vector2.Distance(Input.mousePosition, posIniz) > 100)
                            {
                                if (direz == Vector2.zero)
                                {
                                    Vector2 vet = (Vector2)Input.mousePosition - posIniz;
                                    if (Mathf.Abs(vet.x) > Mathf.Abs(vet.y))
                                        direz = new Vector2(vet.x > 0 ? 1 : -1, 0);
                                    else if (posts[0].GetComponent<FeedViewController>().LoadedFeed == null)
                                        direz = new Vector2(0, vet.y > 0 ? 1 : -1);
                                    else
                                    {
                                        if (vet.y < 0)
                                            direz = new Vector2(0, -1);
                                        else if (posts[0].GetComponent<FeedViewController>().LoadedFeed.ToUserID != "pnf")
                                            direz = new Vector2(0, 1);
                                    }
                                    if (!aperto && direz.y > 0 && FeedsDataLoader.loading && ((apertoMioFeed && UserProfileObject.activeInHierarchy) || (!apertoMioFeed)))
                                    {
                                        direz = Vector2.zero;
                                        return;
                                    }
                                }

                                if (!aperto)
                                {
                                    if (SinglePostObject.activeInHierarchy)
                                    {
                                        if (direz.y != 0)
                                            CommentsObject.SetActive(!SinglePostObject.GetComponentInChildren<FeedsDataLoader>().itemList[0].loading.activeInHierarchy);
                                        if (direz.x != 0)
                                        {
                                            cambiaPost = false;
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (InteractionsObject.activeInHierarchy || vaultPlusObject.activeInHierarchy || UserProfileObject.transform.GetChild(8).gameObject.activeInHierarchy || VaultPLUSFeaturesObject.activeInHierarchy)
                                            return;
                                        if (!apertoMioFeed && UserProfileObject.activeInHierarchy)
                                            ProfileFeedObject.SetActive(direz.y != 0);
                                        else if (apertoMioFeed && UserProfileObject.activeInHierarchy)
                                            CommentsObject.SetActive(direz.y > 0 && !FeedsDataLoader.loading);
                                        else if (!aperto)
                                            CommentsObject.SetActive(direz.y != 0 && !FeedsDataLoader.loading);
                                    }
                                }
                            }
                            else
                                return;

                            if ((InteractionsObject.activeInHierarchy && !SinglePostObject.activeInHierarchy) && direz.x != 0)
                                return;
                            if (direz.x != 0 && !aperto)
                                StatoTransizione(((Input.mousePosition.x - posIniz.x) / Screen.width) / 2 + .5f, posts[1], posts[0], posts[2], true);
                            else
                            {
                                if (SinglePostObject.activeInHierarchy)
                                {
                                    if (!aperto)
                                        StatoTransizione(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) / 2 + .5f, CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject);
                                    else
                                        StatoTransizioneScendi(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) + 1f, CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject);
                                    return;
                                }
                                if (vaultPlusObject.activeInHierarchy || UserProfileObject.transform.GetChild(8).gameObject.activeInHierarchy || VaultPLUSFeaturesObject.activeInHierarchy)
                                    return;
                                if (ProfileFeedObject.activeInHierarchy && !apertoMioFeed && !aperto)
                                    StatoTransizione(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f)) / 2 + .5f, ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject);
                                else if (ProfileFeedObject.activeInHierarchy && apertoMioFeed && !aperto)
                                {
                                    if (direz.y < 0)
                                        StatoTransizioneScendi(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) + 1f, ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject);
                                    else
                                        StatoTransizione(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) / 2 + .5f, CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject);
                                }
                                else if (!aperto)
                                    StatoTransizione(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) / 2 + .5f, CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject);
                                else
                                    StatoTransizioneScendi(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) + 1f, CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject);
                            }
                        }
                        else if (Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            if (direz.x != 0)
                            {
                                float vel = (Input.mousePosition.x - posIniz.x) / (Time.time - tempo);
                                void Resta()
                                {
                                    if (vel > 0)
                                        StartCoroutine(Transizione(posts[2], posts[1], posts[0], (Input.mousePosition.x - posIniz.x) / Screen.width / 2, -1, true, false));
                                    else
                                        StartCoroutine(Transizione(posts[0], posts[2], posts[1], 1 - Mathf.Abs((Input.mousePosition.x - posIniz.x) / Screen.width / 2), 1, true, false));
                                }

                                if (aperto)
                                {
                                    CommentsObject.GetComponent<MessagesDataLoader>().Avanti(vel < 0);
                                    return;
                                }

                                if (InteractionsObject.activeInHierarchy)
                                {
                                    InteractionsObject.GetComponentInChildren<InteractionsLoader>().Avanti(vel < 0);
                                    return;
                                }

                                if ((Mathf.Abs(posts[0].GetComponent<RectTransform>().anchoredPosition.x) > 382 || Mathf.Abs(vel) > 1500) && Mathf.Abs(Input.mousePosition.x - posIniz.x) > Screen.width / 15f)
                                {
                                    if (vel > 0)
                                    {
                                        if (oggetto.GetComponent<FeedsDataLoader>().FeedsLoaded == 0)
                                        {
                                            Resta();
                                            return;
                                        }
                                        StartCoroutine(Transizione(posts[1], posts[0], posts[2], ((Input.mousePosition.x - posIniz.x) / Screen.width) / 2 + .5f, 1, true, false));
                                        oggetto.GetComponentInChildren<FeedsDataLoader>().AutoLoadContent(false);
                                    }
                                    else
                                    {
                                        if (oggetto.GetComponent<FeedsDataLoader>().ultimoPost)
                                        {
                                            Resta();
                                            return;
                                        }
                                        StartCoroutine(Transizione(posts[1], posts[0], posts[2], ((Input.mousePosition.x - posIniz.x) / Screen.width) / 2 + .5f, -1, true, false));
                                        oggetto.GetComponent<FeedsDataLoader>().AutoLoadContent(true);
                                    }
                                }
                                else
                                    Resta();
                            }
                            else if (direz.y != 0)
                            {
                                float vel = (Input.mousePosition.y - posIniz.y) / (Time.time - tempo);
                                void Resta()
                                {
                                    StartCoroutine(TransizioneVersoAlto(CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject, (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f) / 2 + .5f, -1, false));
                                }
                                void ApriCom(bool a)
                                {
                                    if (a)
                                    {
                                        if ((CommentsObject.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y > -678 || Mathf.Abs(vel) > 1500) && Mathf.Abs(Input.mousePosition.y - posIniz.y) > Screen.width * 16 / 9f / 15f)
                                        {
                                            if (vel > 0)
                                            {
                                                if (SinglePostObject.activeInHierarchy && SinglePostObject.GetComponentInChildren<FeedViewController>().LoadedFeed != null)
                                                    CommentsObject.GetComponent<MessagesDataLoader>().LoadPostComments(SinglePostObject.GetComponentInChildren<FeedViewController>().LoadedFeed.Key, SinglePostObject.GetComponentInChildren<FeedViewController>().LoadedFeed.OwnerID, true);
                                                else if (posts[0].GetComponent<FeedViewController>().LoadedFeed != null)
                                                    CommentsObject.GetComponent<MessagesDataLoader>().LoadPostComments(posts[0].GetComponent<FeedViewController>().LoadedFeed.Key, posts[0].GetComponent<FeedViewController>().LoadedFeed.OwnerID, true);
                                                CommentsObject.GetComponent<MessagesDataLoader>().primo = true;
                                                CommentsObject.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(AppSettings.guest || AppManager.USER_PROFILE.FIREBASE_USER == null ? 1 : 0).gameObject.SetActive(true);
                                                CommentsObject.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(AppSettings.guest || AppManager.USER_PROFILE.FIREBASE_USER == null ? 0 : 1).gameObject.SetActive(false);
                                                StartCoroutine(TransizioneVersoAlto(CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject, (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f) / 2 + .5f, 1, true));
                                            }
                                            else
                                                Resta();
                                        }
                                        else
                                            Resta();
                                    }
                                    else
                                    {
                                        if ((CommentsObject.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y < -678 || Mathf.Abs(vel) > 1500) && Mathf.Abs(Input.mousePosition.y - posIniz.y) > Screen.width * 16 / 9f / 15f)
                                        {
                                            if (vel > 0)
                                                StartCoroutine(TransizioneVersoAlto(CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), 1, true));
                                            else
                                                StartCoroutine(TransizioneVersoAlto(CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), -1, false));
                                        }
                                        else
                                            StartCoroutine(TransizioneVersoAlto(CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), 1, true));
                                    }
                                }
                                void ApriFeed(bool a)
                                {
                                    if (InteractionsObject.activeInHierarchy || vaultPlusObject.activeInHierarchy || UserProfileObject.transform.GetChild(8).gameObject.activeInHierarchy || VaultPLUSFeaturesObject.activeInHierarchy)
                                        return;
                                    if (a)
                                    {
                                        if ((ProfileFeedObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y > -678 || Mathf.Abs(vel) > 1500) && Mathf.Abs(Input.mousePosition.y - posIniz.y) > Screen.width * 16 / 9f / 15f)
                                        {
                                            if (vel > 0)
                                                StartCoroutine(TransizioneVersoAlto(ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject, (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f) / 2 + .5f, 1, true, true));
                                            else
                                                StartCoroutine(TransizioneVersoAlto(ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject, (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f) / 2 + .5f, -1, false, true));
                                        }
                                        else
                                            StartCoroutine(TransizioneVersoAlto(ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject, (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f) / 2 + .5f, -1, false, true));
                                    }
                                    else
                                    {
                                        if ((ProfileFeedObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y < -678 || Mathf.Abs(vel) > 1500) && Mathf.Abs(Input.mousePosition.y - posIniz.y) > Screen.width * 16 / 9f / 15f)
                                        {
                                            if (vel > 0)
                                                StartCoroutine(TransizioneVersoAlto(ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), 1, true, true));
                                            else
                                                StartCoroutine(TransizioneVersoAlto(ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), -1, false, true));
                                        }
                                        else
                                            StartCoroutine(TransizioneVersoAlto(ProfileFeedObject.transform.GetChild(0).gameObject, UserProfileObject.transform.GetChild(17).gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), 1, true, true));
                                    }
                                }
                                if (SinglePostObject.activeInHierarchy && aperto && vel > 0)
                                    return;
                                if (SinglePostObject.activeInHierarchy)
                                {
                                    ApriCom(vel > 0);
                                    return;
                                }
                                if (apertoMioFeed && !aperto)
                                {
                                    if (vel > 0)
                                        ApriCom(true);
                                    else
                                        ApriFeed(false);
                                }
                                else if (apertoMioFeed && aperto)
                                    ApriCom(false);
                                else if (!apertoMioFeed && !aperto && UserProfileObject.activeInHierarchy)
                                    ApriFeed(true);
                                else if (!apertoMioFeed && !aperto)
                                    ApriCom(true);
                                else
                                    ApriCom(false);
                            }
                            cambiaPost = false;
                        }
                    }
                }
            }
        }

        public void ApriCommentoBt(FeedViewController post)
        {
            aperto = false;
            CommentsObject.SetActive(true);
            if (SinglePostObject.activeInHierarchy && SinglePostObject.GetComponentInChildren<FeedViewController>().LoadedFeed != null)
                CommentsObject.GetComponent<MessagesDataLoader>().LoadPostComments(SinglePostObject.GetComponentInChildren<FeedViewController>().LoadedFeed.Key, SinglePostObject.GetComponentInChildren<FeedViewController>().LoadedFeed.OwnerID, true);
            else if (post.LoadedFeed != null)
                CommentsObject.GetComponent<MessagesDataLoader>().LoadPostComments(post.LoadedFeed.Key, post.LoadedFeed.OwnerID, true);
            CommentsObject.GetComponent<MessagesDataLoader>().primo = true;
            CommentsObject.transform.GetChild(0).GetChild(2).GetChild(AppSettings.guest ? 1 : 0).gameObject.SetActive(true);
            CommentsObject.transform.GetChild(0).GetChild(2).GetChild(AppSettings.guest ? 0 : 1).gameObject.SetActive(false);
            StartCoroutine(TransizioneVersoAlto(CommentsObject.transform.GetChild(1).gameObject, CommentsObject.transform.GetChild(0).gameObject, .5f, 1, true));
        }
    }

    public enum MessageCode
    {
        EmptyEmail,
        EmptyFirstName,
        EmptyLastName,
        EmptyPassword,
        PasswordNotMatch,
        EmailNotValid,
        SmallPassword,
        RegistrationSuccess,
        RegistrationSuccessWithConfirm,
        VideoProcessing,
        MaxVideoSize,
        FailedUploadFeed,
        FailedUploadImage,
        SuccessPost,
        EmailConfirm,
        DeleteFeedOwnerError,
        CallIsBisy,
        MainFeed,
        PostFailed,
        UpdateEmailFailed,
        CurrentPasswordIsIncorrect
    }
}