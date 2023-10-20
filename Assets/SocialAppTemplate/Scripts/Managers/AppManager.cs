using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialApp;
using Firebase.Database;
using Firebase.Auth;
//using GoogleMobileAds.Placement;
//using GoogleMobileAds.Api;
using Firebase;
using System;
using System.Linq;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using Lofelt.NiceVibrations;

namespace SocialApp
{

    public class AppManager : MonoBehaviour
    {
        //public static InterstitialAdGameObject intersAd;
        public FeedsDataLoader feedDataLoader;
        // instance
        private static AppManager instance;
        public static AppManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<AppManager>();
                }
                return instance;
            }

        }

        //tutorial
        public TutorialController TutorialController;
        public static TutorialController TUTORIAL_CONTROLLER
        {
            get
            {
                if (Instance == null)
                {
                    return null;
                }
                else
                {
                    return Instance.TutorialController;
                }
            }
        }

        // device
        public DeviceController Device;
        public static DeviceController DEVICE_CONTROLLER
        {
            get
            {
                return Instance.Device;
            }
        }

        // firebase
        public FirebaseController Firebase;
        public static FirebaseController FIREBASE_CONTROLLER
        {
            get
            {
                if (Instance == null)
                {
                    return null;
                }
                else
                {
                    return Instance.Firebase;
                }
            }
        }

        // registration
        public RegistrationController Registration;
        public static RegistrationController REGISTRATION_CONTROLLER
        {
            get
            {
                return Instance.Registration;
            }
        }

        // login
        public LoginController Login;
        public static LoginController LOGIN_CONTROLLER
        {
            get
            {
                return Instance.Login;
            }
        }

        public static List<string> moderatorsList = new List<string>();
        public static bool IsAdmin()
        {
            bool b = moderatorsList.Contains(FirebaseAuth.DefaultInstance.CurrentUser.Email) && !Application.isEditor;
            VIEW_CONTROLLER.moderatorBt.SetActive(b);
            return b;
        }

        // view
        public ViewController View;
        public static ViewController VIEW_CONTROLLER
        {
            get
            {
                return Instance.View;
            }
        }

        // app setings
        private AppSettings Settings;
        public static AppSettings APP_SETTINGS
        {
            get
            {
                if (Instance == null)
                    return null;
                if (Instance.Settings == null)
                {
                    Instance.Settings = Resources.Load<AppSettings>(AppSettings.AppSettingPath);
                }
                return Instance.Settings;
            }
        }

        // profile
        public ProfileController Profile;
        public static ProfileController USER_PROFILE
        {
            get
            {
                if (Instance == null)
                {
                    return null;
                }
                else
                {
                    return Instance.Profile;
                }

            }
        }

        public UserInfoViewController UserInfo;
        public static UserInfoViewController USER_INFO
        {
            get
            {
                return Instance.UserInfo;
            }
        }

        public DynamicsLinkController DynamicsLink;
        public static DynamicsLinkController DYNAMICS_CONTROLLER
        {
            get
            {
                if (Instance == null)
                {
                    return null;
                }
                else
                {
                    return Instance.DynamicsLink;
                }
            }
        }

        // navigation
        public NavigationController Navigation;
        public static NavigationController NAVIGATION
        {
            get
            {
                return Instance.Navigation;
            }
        }

        // settings
        public SettingsController UserSettings;
        public static SettingsController USER_SETTINGS
        {
            get
            {
                return Instance.UserSettings;
            }
        }

        // friends ui
        public FriendsUIController FriendsUI;
        public static FriendsUIController FRIEND_UI_CONTROLLER
        {
            get
            {
                return Instance.FriendsUI;
            }
        }

        // settings
        public AgoraController AgoraController;
        public static AgoraController AGORA_CONTROLLER
        {
            get
            {
                return Instance.AgoraController;
            }
        }

        private void Start()
        {
            Init();
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
            RemoveCallListener();
        }

        private void AddListeners()
        {
            LOGIN_CONTROLLER.OnLoginEvent += OnLoginSuccess;
            LOGIN_CONTROLLER.OnLogoutEvent += OnLogoutSuccess;
        }
        public void Debug(string s) => print(s);
        private void RemoveListeners()
        {
            LOGIN_CONTROLLER.OnLoginEvent -= OnLoginSuccess;
            LOGIN_CONTROLLER.OnLogoutEvent -= OnLogoutSuccess;
        }

        private void Init()
        {
#if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                ATTrackingStatusBinding.RequestAuthorizationTracking();
#endif
            if (PlayerPrefs.HasKey("accessi"))
            {
                int n = PlayerPrefs.GetInt("accessi");
                if (!(n == 3 || n == 5 || n == 20 || n == 50 || n == 70 || n == 100))
                    PlayerPrefs.SetInt("accessi", n + 1);
            }
            else
                PlayerPrefs.SetInt("accessi", 0);
            //PlayerPrefs.DeleteKey("accessi");
            //intersAd = MobileAds.Instance.GetAd<InterstitialAdGameObject>("interstitial");
            //MobileAds.Initialize((initStatus) => { });
            Application.targetFrameRate = 60;
            VIEW_CONTROLLER.HideAllScreen();
            //FIREBASE_CONTROLLER.InitPushNotificationEvents();
            FIREBASE_CONTROLLER.InitFirebase();
            if (APP_SETTINGS._EnableVideoAudioCalls)
            {
#if PLATFORM_ANDROID
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                }
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                }
#endif
                //AgoraController.Init();
            }
            StartCoroutine(WaitForFirebaseReady());
        }

        private IEnumerator WaitForFirebaseReady()
        {
            VIEW_CONTROLLER.ShowLoading();
            while (!FIREBASE_CONTROLLER.IsFirebaseInited())
            {
                yield return new WaitForFixedUpdate();
            }
            VIEW_CONTROLLER.HideLoading();
            FIREBASE_CONTROLLER.GetModerators();
            CheckLogin();
        }

        private void CheckLogin()
        {
            string savedEmail = PlayerPrefs.GetString(AppSettings.LoginSaveKey);
            if (string.IsNullOrEmpty(savedEmail))
            {
                VIEW_CONTROLLER.ShowPrima();
            }
            else
            {
                AutoLogin();
            }
        }

        public void AutoLogin()
        {
            string savedEmail = PlayerPrefs.GetString(AppSettings.LoginSaveKey);
            string savedPassword = PlayerPrefs.GetString(AppSettings.PasswordSaveKey);
            LOGIN_CONTROLLER.AutoLogin(savedEmail, savedPassword);
        }

        private void OnLoginSuccess()
        {
            AddCallListener();
        }

        private void OnLogoutSuccess()
        {
            RemoveCallListener();
        }

        public void MostraAnnuncio()
        {
            //intersAd.ShowIfLoaded();
        }
        public static void LoadAd()
        {
            //intersAd.LoadAd();
        }

        Query CallReference;

        private void AddCallListener()
        {
            if (APP_SETTINGS._EnableVideoAudioCalls)
            {
                CallReference = FIREBASE_CONTROLLER.GetCallReference().LimitToLast(1);
                CallReference.ChildAdded += HandleCallAdded;
            }
        }

        private void RemoveCallListener()
        {
            if (APP_SETTINGS._EnableVideoAudioCalls && CallReference != null)
            {
                CallReference.ChildAdded -= HandleCallAdded;
                CallReference = null;
            }
        }

        void HandleCallAdded(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                return;
            }
            else
            {
                CallObject _callMsg = JsonUtility.FromJson<CallObject>(args.Snapshot.GetRawJsonValue());

                // check is call valid
                AppManager.FIREBASE_CONTROLLER.GetServerTimestamp(_msg =>
                {
                    string _time = _msg.Data;
                    long timeStamp;
                    bool isInteger = long.TryParse(_time, out timeStamp);

                    if (isInteger)
                    {
                        long callTimeStamp;
                        bool IsSucces = long.TryParse(_callMsg.CreateTimeStamp, out callTimeStamp);
                        if (IsSucces)
                        {
                            long _timePassed = (long)Mathf.Abs(timeStamp - callTimeStamp);
                            int _timePassedSeconds = (int)_timePassed / 100;
                            if (_timePassedSeconds < (long)AppSettings.IncomingCallMaxTime)
                            {
                                if (_callMsg.IsActive)
                                {
                                    /*if (VIEW_CONTROLLER.IsCallWindowActive())
                                    {
                                        // send bisy
                                        FIREBASE_CONTROLLER.SetCallBisy(_callMsg);
                                    }
                                    else
                                    {
                                        VIEW_CONTROLLER.StartCall(IncommingType.ANSWERS, _callMsg);
                                    }*/
                                }
                            }
                        }
                    }
                });
            }
        }

        public void SendEmail()
        {
            string email = "altavelos.business@gmail.com";
            string subject = MyEscapeURL("(AV: Vault) Insert a post in the main feed");
            string body = MyEscapeURL("Hi.\nI am interested in promoting my post by inserting it in the main feed.\nCould you give me more information?\nHow much will it cost?\r\n");
            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        }

        string MyEscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }

        public void HapticSelect() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        public void HapticSuccess() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
        public void HapticFailure() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
        public void HapticWarning() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
        public void HapticSoftImpact() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
        public void HapticLightImpact() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        public void HapticMediumImpact() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
        public void HapticRigidImpact() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
        public void HapticHeavyImpact() => HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
    }
}
