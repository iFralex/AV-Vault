using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity;
using Firebase.Storage;
using Firebase;
//using Firebase.Unity.Editor;
using System;
using System.Threading.Tasks;
using System.Linq;
using Firebase.Functions;
using Firebase.Extensions;

namespace SocialApp
{

    public class FirebaseController : MonoBehaviour
    {

        private FirebaseAuth Auth;
        public FirebaseDatabase Database;
        private FirebaseStorage Storage;
        private FirebaseFunctions Functions;
        private string CurrentDeviceToken;

        private bool FirebaseIsInited = false;

        public void InitFirebase()
        {
            OnFirebaseInit();
        }

        // init
        private void OnFirebaseInit()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    Auth = FirebaseAuth.DefaultInstance;
                    Database = FirebaseDatabase.DefaultInstance;
                    Storage = FirebaseStorage.DefaultInstance;
                    Functions = FirebaseFunctions.DefaultInstance;
                    FirebaseIsInited = true;
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void DeleteComment(string key, string postId, Action<bool> p)
        {
            Database.RootReference.Child("PostComments").Child(postId).Child("List").Child(key).RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    Database.RootReference.Child("ReportedComments").Child(key).RemoveValueAsync();
                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(true));
                    CleanTask(task);
                }
            });
        }

        public void DeleteImageProfile(string n, Action<bool> p)
        {
            print(-1);
            Storage.RootReference.Child("UsersData").Child(n).Child("Avatar").Child("Image_Origin.jpg").DeleteAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    print(0);
                    Storage.RootReference.Child("UsersData").Child(n).Child("Avatar").Child("Image_Size_1024.jpg").DeleteAsync().ContinueWith(task1 =>
                    {
                        if (task1.IsFaulted || task1.IsCanceled)
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                            CleanTask(task1);
                        }
                        else if (task1.IsCompleted)
                        {
                            print(1);
                            Storage.RootReference.Child("UsersData").Child(n).Child("Avatar").Child("Image_Size_512.jpg").DeleteAsync().ContinueWith(task2 =>
                            {
                                if (task2.IsFaulted || task2.IsCanceled)
                                {
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                                    CleanTask(task2);
                                }
                                else if (task2.IsCompleted)
                                {
                                    print(2);
                                    Storage.RootReference.Child("UsersData").Child(n).Child("Avatar").Child("Image_Size_256.jpg").DeleteAsync().ContinueWith(task3 =>
                                    {
                                        if (task3.IsFaulted || task3.IsCanceled)
                                        {
                                            UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                                            CleanTask(task3);
                                        }
                                        else if (task3.IsCompleted)
                                        {
                                            print(3);
                                            Storage.RootReference.Child("UsersData").Child(n).Child("Avatar").Child("Image_Size_128.jpg").DeleteAsync().ContinueWith(task4 =>
                                            {
                                                if (task4.IsFaulted || task4.IsCanceled)
                                                {
                                                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                                                    CleanTask(task4);
                                                }
                                                else if (task4.IsCompleted)
                                                {
                                                    print(4);
                                                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(true));
                                                    CleanTask(task4);
                                                }
                                            });
                                            CleanTask(task3);
                                        }
                                    });
                                    CleanTask(task2);
                                }
                            });
                            CleanTask(task1);
                        }
                    });
                    CleanTask(task);
                }
            });
        }

        public void ReplaceAccountName(DatabaseReference r, string n, Action<bool> p)
        {
            r.SetValueAsync(n).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => p.Invoke(true));
                    CleanTask(task);
                }
            });
        }

        public void InitPushNotificationEvents()
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            //Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        }

        public void CheckUserDeleted(string userId, Action<bool> _action)
        {
            Database.RootReference.Child("DeletedUsers").Child(userId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(task.Result.Exists));
                    CleanTask(task);
                }
            });
        }

        public void RemovePushNotificationEvents()
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
            //Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
        }

        public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            CurrentDeviceToken = token.Token;
            if (AppManager.USER_PROFILE.IsLogined() && !AppSettings.guest && !Application.isEditor)
            {
                print(CurrentDeviceToken);
                RegisterDeviceTokens();
            }
        }

        public void RegisterDeviceTokens()
        {
            if (string.IsNullOrEmpty(CurrentDeviceToken))
                return;
            Database.RootReference.Child(AppSettings.DeviceTokensKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(CurrentDeviceToken).SetValueAsync(0);
        }

        public void RemoveDeviceTokens()
        {
            if (string.IsNullOrEmpty(CurrentDeviceToken))
                return;
            Database.RootReference.Child(AppSettings.DeviceTokensKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(CurrentDeviceToken).RemoveValueAsync();
        }

        public void ClearDeviceToken()
        {
            CurrentDeviceToken = string.Empty;
        }

        public void GetUserDeviceTokes(string _userId, Action<List<string>> _callback)
        {
            DatabaseReference _tokenRef = Database.RootReference.Child(AppSettings.DeviceTokensKey).Child(_userId);
            _tokenRef.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {

                }
                else if (task.IsCompleted && task.Result.Exists)
                {
                    List<string> _ids = new List<string>();
                    DataSnapshot snapshot = task.Result;
                    foreach (DataSnapshot _data in snapshot.Children)
                    {
                        _ids.Add(_data.Key);
                    }

                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_ids));
                }
            });
        }

        public void SendPushNotification(NotificationMessage _message)
        {
            string _userID = _message.UserId;
            var data = new Dictionary<string, object>();
            data["_userId"] = _userID;
            data["_title"] = _message.Title;
            data["_body"] = _message.Body;

            // Call the function and extract the operation from the result.
            HttpsCallableReference _function = Functions.GetHttpsCallable("SendFCM");
            _function.CallAsync(data);

        }
        public List<string> lista;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Database.RootReference.Child("AllPosts").GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        for (int i = 0; i < task.Result.ChildrenCount; i++)
                            lista.Add(task.Result.Children.ElementAt(i).Key);
                    }
                });
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    string a = lista[i];
                    int index = UnityEngine.Random.Range(i, lista.Count - 1);
                    lista[i] = lista[index];
                    lista[index] = a;
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                for (int i = 0; i < lista.Count / 10 + 1; i++)
                    for (int o = 0; o < 10 && i * 10 + o < lista.Count; o++)
                        Database.RootReference.Child("AllRandomPosts").Child(i.ToString()).Child(lista[i * 10 + o]).SetValueAsync(0);
            }
        }
        // register with google
        public void RegisterWithGoogle(string googleIdToken, string googleAccessToken, Action<RegistrationMessage> _callback)
        {
            Credential credential =
            GoogleAuthProvider.GetCredential(googleIdToken, googleAccessToken);

            RegistrationMessage _regMsg = new RegistrationMessage();
            Auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    _regMsg.ErrorMessage = "CreateUserGoogle was canceled";
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_regMsg));
                    CleanTask(task);
                    return;
                }
                if (task.IsFaulted)
                {
                    _regMsg.ErrorMessage = "CreateUserGoogle encountered an error: " + task.Exception;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_regMsg));
                    CleanTask(task);
                    return;
                }
                Firebase.Auth.FirebaseUser newUser = task.Result;
                _regMsg.UserID = newUser.UserId;
                _regMsg.IsComplete = true;
                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_regMsg));
                CleanTask(task);
            });
        }

        // add new user
        public void AddNewUser(string _email, string _password, Action<RegistrationMessage> _callback)
        {
            RegistrationMessage _regMsg = new RegistrationMessage();
            Auth.CreateUserWithEmailAndPasswordAsync(_email, _password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    _regMsg.ErrorMessage = "CreateUserWithEmailAndPasswordAsync was canceled";
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_regMsg));
                    CleanTask(task);
                    return;
                }
                if (task.IsFaulted)
                {
                    _regMsg.ErrorMessage = "CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_regMsg));
                    CleanTask(task);
                    return;
                }
                Firebase.Auth.FirebaseUser newUser = task.Result;
                _regMsg.UserID = newUser.UserId;
                _regMsg.IsComplete = true;
                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_regMsg));
                CleanTask(task);
            });
        }

        // get user data
        public void GetUserData(string _userID, Action<User> _callback)
        {
            Query databaseQuery = Database.RootReference.Child(AppSettings.RootUserKey).Child(_userID);

            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(null));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    if (!task.Result.Exists)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(null));
                        CleanTask(task);
                        return;
                    }
                    User _user = JsonUtility.FromJson<User>(task.Result.GetRawJsonValue().ToString());

                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_user));
                    CleanTask(task);
                }
            });
        }

        public void UpdateUserName(string _firstName)
        {
            AppManager.VIEW_CONTROLLER.ShowLoading();
            /*Database.RootReference.Child(AppSettings.RootUserKey).Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("FirstName").SetValueAsync(_firstName).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {*/
            Database.RootReference.Child(AppSettings.RootUserKey).Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("FullName").SetValueAsync(_firstName).ContinueWithOnMainThread(task1 =>
            {
                if (task1.IsCompleted)
                {
                    /*Database.RootReference.Child(AppSettings.RootUserKey).Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("LastName").SetValueAsync(_lastName).ContinueWithOnMainThread(task2 =>
                    {
                        if (task2.IsCompleted)
                        {*/
                    AppManager.USER_PROFILE.UserFullName = "";
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    AppManager.VIEW_CONTROLLER.HideUserProfile();
                    AppManager.VIEW_CONTROLLER.ShowUserProfile();
                    //CleanTask(task1);
                    //}
                    //});
                    CleanTask(task1);
                }
                /*});
                CleanTask(task);
            */
            });
        }

        // set user data
        public void SetUserData(User _user, Action<SetUserDataMessage> _callback)
        {
            string json = JsonUtility.ToJson(_user);
            SetUserDataMessage _logMsg = new SetUserDataMessage();
            Database.RootReference.Child(AppSettings.RootUserKey).Child(_user.UserID).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    _logMsg.ErrorMessage = "Set user data was canceled";
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                if (task.IsFaulted)
                {
                    _logMsg.ErrorMessage = "Set User Data encountered an error: " + task.Exception;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                _logMsg.IsSuccess = true;
                _logMsg.UserID = _user.UserID;
                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                CleanTask(task);
            });
        }

        // get users who like post
        public void GetUsersWhoLikePost(string _postID, Action<List<User>> _callback)
        {
            Query databaseQuery;
            string rootKey = AppSettings.PostLikesKey;
            databaseQuery = Database.RootReference.Child(rootKey).Child(_postID).Child(AppSettings.ContainerListKey);


            List<User> _users = new List<User>();

            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result.Exists)
                    {
                        DataSnapshot snapshot = task.Result;


                        int _userCount = (int)snapshot.ChildrenCount;

                        List<Task> TaskList = new List<Task>();

                        for (int i = 0; i < _userCount; i++)
                        {
                            DataSnapshot userSnapshot = snapshot.Children.ElementAt(i);
                            string _userID = userSnapshot.Key;
                            DatabaseReference _friendRef = Database.RootReference.Child(AppSettings.RootUserKey).Child(_userID);
                            Task<DataSnapshot> _task = _friendRef.GetValueAsync();
                            TaskList.Add(_task);

                        }
                        Task.WhenAll(TaskList).ContinueWith(task2 =>
                        {
                            if (task2.IsCompleted)
                            {
                                foreach (Task<DataSnapshot> t in TaskList)
                                {
                                    if (t.IsCompleted && t.Result.Exists)
                                    {
                                        string _userId = t.Result.Key;
                                        string jsonUser = t.Result.GetRawJsonValue();
                                        if (!string.IsNullOrEmpty(jsonUser))
                                        {
                                            User _dataUser = JsonUtility.FromJson<User>(jsonUser);
                                            _users.Add(_dataUser);

                                        }
                                        CleanTask(t);
                                    }
                                }
                            }

                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_users));
                            CleanTask(task2);
                        });
                        CleanTask(task);
                    }
                });
        }

        public void LogInGoogle(string googleIdToken, string googleAccessToken, Action<LoginMessage> _callback, bool _silentMode = false)
        {
            Credential credential =
            GoogleAuthProvider.GetCredential(googleIdToken, googleAccessToken);

            LoginMessage _logMsg = new LoginMessage();
            Auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    _logMsg.IsSuccess = false;
                    _logMsg.ErrorMessage = "SignInWithEmailAndPasswordAsync was canceled.";
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                if (task.IsFaulted)
                {
                    _logMsg.IsSuccess = false;
                    _logMsg.ErrorMessage = task.Exception.Message;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    FirebaseUser newUser = task.Result;
                    if (AppManager.APP_SETTINGS.UseEmailConfirm && !_silentMode)
                    {
                        if (newUser.IsEmailVerified)
                        {
                            _logMsg.IsSuccess = true;
                            _logMsg.UserID = newUser.UserId;
                            _logMsg.FUser = newUser;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                        }
                        else
                        {
                            AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmailConfirm, AppManager.VIEW_CONTROLLER.ShowLogin);
                            AppManager.VIEW_CONTROLLER.HideLoading();
                            LogOut();
                        }
                        CleanTask(task);
                    }
                    else
                    {
                        _logMsg.IsSuccess = true;
                        _logMsg.UserID = newUser.UserId;
                        _logMsg.FUser = newUser;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                        CleanTask(task);
                    }
                });
            });
        }

        public void LogInAsGuest(Action<LoginMessage> _callback)
        {
            LoginMessage _logMsg = new LoginMessage();
            Auth.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    _logMsg.IsSuccess = false;
                    _logMsg.ErrorMessage = "SignInWithEmailAndPasswordAsync was canceled.";
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                if (task.IsFaulted)
                {
                    _logMsg.IsSuccess = false;
                    _logMsg.ErrorMessage = task.Exception.Message;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    FirebaseUser newUser = task.Result;
                    _logMsg.IsSuccess = true;
                    _logMsg.isGuest = true;
                    _logMsg.UserID = newUser.UserId;
                    _logMsg.FUser = newUser;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                });
            });
        }

        // login
        public void LogIn(string _login, string _password, Action<LoginMessage> _callback, bool _silentMode = false)
        {
            LoginMessage _logMsg = new LoginMessage();
            //Auth.SignInWithEmailAndPasswordAsync(_login, _password).ContinueWith(task =>
            Auth.SignInWithEmailAndPasswordAsync(_login, _password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    _logMsg.IsSuccess = false;
                    _logMsg.ErrorMessage = "SignInWithEmailAndPasswordAsync was canceled.";
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                if (task.IsFaulted)
                {
                    _logMsg.IsSuccess = false;
                    _logMsg.ErrorMessage = task.Exception.Message;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                    CleanTask(task);
                    return;
                }
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    FirebaseUser newUser = task.Result;
                    if (AppManager.APP_SETTINGS.UseEmailConfirm && !_silentMode)
                    {
                        if (newUser.IsEmailVerified)
                        {
                            _logMsg.IsSuccess = true;
                            _logMsg.UserID = newUser.UserId;
                            _logMsg.FUser = newUser;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                        }
                        else
                        {
                            AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmailConfirm, AppManager.VIEW_CONTROLLER.ShowLogin);
                            AppManager.VIEW_CONTROLLER.HideLoading();
                            FirebaseAuth.DefaultInstance.CurrentUser.SendEmailVerificationAsync();
                            LogOut();
                        }
                        CleanTask(task);
                    }
                    else
                    {
                        _logMsg.IsSuccess = true;
                        _logMsg.UserID = newUser.UserId;
                        _logMsg.FUser = newUser;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_logMsg));
                        CleanTask(task);
                    }
                });
            });
        }

        public void SetSubscriptionVaultPLUS(int duration, Action action)
        {
            Database.RootReference.Child(AppSettings.SubscriptionsVaultPLUSKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).SetRawJsonValueAsync(JsonUtility.ToJson(new SubscriptionVaultPLUS() { StartDate = DateTime.UtcNow.ToString(), Duration = duration })).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    AppSettings.VaultPLUS = false;
                else if (task.IsCompleted)
                    AppSettings.VaultPLUS = true;
                AppSettings.GiorniRimastiVaultPLUS = duration;
                UnityMainThreadDispatcher.Instance().Enqueue(() => action.Invoke());
            });
        }

        public void UserCountFollowers(string _id, Action<int> _action)
        {
            Database.RootReference.Child("FollowerUsers").Child(_id).Child("Count").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(0));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(Convert.ToInt32(task.Result.Value)));
                    CleanTask(task);
                }
            });
        }

        public void CheckUserIsVaultPlus(string id, Action<bool> a)
        {
            Database.RootReference.Child(AppSettings.SubscriptionsVaultPLUSKey).Child(id).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => a.Invoke(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => a.Invoke(task.Result.Exists));
                    CleanTask(task);
                }
            });
        }

        public void GetUserPostsCount(string _id, Action<int> _action)
        {
            Database.RootReference.Child("PostCounts").Child(_id).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(0));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(task.Result.Exists ? Convert.ToInt32(task.Result.Value) : 0));
                    CleanTask(task);
                }
            });
        }

        public void GetSubscriptionVaultPLUS()
        {
            Database.RootReference.Child(AppSettings.SubscriptionsVaultPLUSKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    AppSettings.VaultPLUS = false;
                else if (task.IsCompleted)
                {
                    if (!task.Result.Exists)
                    {
                        AppSettings.VaultPLUS = false;
                        return;
                    }
                    DateTime data = DateTime.Parse(Convert.ToString(task.Result.Child("StartDate").Value), System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    if (Convert.ToInt32(task.Result.Child("Duration").Value) != -1)
                        AppSettings.GiorniRimastiVaultPLUS = (data.AddDays(Convert.ToInt32(task.Result.Child("Duration").Value)) - DateTime.UtcNow).Days;
                    else
                    {
                        AppSettings.GiorniRimastiVaultPLUS = -1;
                        AppSettings.VaultPLUS = true;
                        if (AppManager.VIEW_CONTROLLER.UserProfileObject.activeInHierarchy)
                            AppManager.VIEW_CONTROLLER.UserProfileObject.GetComponent<UserProfileLoader>().OnEnable();
                        return;
                    }
                    AppSettings.VaultPLUS = AppSettings.GiorniRimastiVaultPLUS >= 0;
                    if (AppSettings.GiorniRimastiVaultPLUS < 0)
                    {
                        print("a");
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Warning", Message = "Your <color=yellow><b>Vault PLUS</b></color> membership has expired.\nClick \"Ok\" to renew it!", Callback = () => AppManager.VIEW_CONTROLLER.ShowPurcaseVaultPLUS() }));

                        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Warning);
                        print("c");
                        Database.RootReference.Child(AppSettings.SubscriptionsVaultPLUSKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).RemoveValueAsync();
                        print("d");
                        Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.UserCustomBackgroundKey).Child("Image_Size_512.jpg").DeleteAsync();
                        print("ciao");
                    }
                    else if (AppSettings.VaultPLUS && AppManager.VIEW_CONTROLLER.UserProfileObject.activeInHierarchy)
                        AppManager.VIEW_CONTROLLER.UserProfileObject.GetComponent<UserProfileLoader>().OnEnable();
                }
            });
        }

        public void LogOut()
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }

        public void SendVerifcationEmail()
        {
            FirebaseAuth.DefaultInstance.CurrentUser.SendEmailVerificationAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendEmailVerificationAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Email sent successfully.");
            });
        }

        public void UploadAvatar(UploadImageRequest _request, Action<UploadImageCallBack> _callback)
        {
            UploadImageCallBack uploadCallback = new UploadImageCallBack();
            Firebase.Storage.StorageReference avatar_ref = Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_request.OwnerId).Child(AppSettings.UserAvatarKey + "/" + "Image_" + _request.Size.ToString() + ".jpg");
            avatar_ref.PutBytesAsync(_request.ImageBytes)
                .ContinueWith((task) =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        print("immagine fallita");
                        uploadCallback.IsFinish = true;
                        uploadCallback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(uploadCallback));
                        CleanTask(task);
                    }
                    else
                    {
                        print("riscito immagine");
                        uploadCallback.IsFinish = true;
                        uploadCallback.IsSuccess = true;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(uploadCallback));
                        CleanTask(task);
                    }
                });
        }

        public void GetProfileImage(GetProfileImageRequest _request, Action<GetProfileImageCallback> _callback)
        {
            GetProfileImageCallback _profileCallback = new GetProfileImageCallback();
            const long maxAllowedSize = 1 * 2048 * 2048;
            Firebase.Storage.StorageReference avatar_ref = Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_request.Id).Child(AppSettings.UserAvatarKey + "/" + "Image_" + _request.Size.ToString() + ".jpg");
            avatar_ref.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _profileCallback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
                else
                {
                    byte[] fileContents = task.Result;
                    _profileCallback.IsSuccess = true;
                    _profileCallback.ImageBytes = fileContents;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
            });
        }

        public void GetProfileImageUrl(GetProfileImageRequest _request, Action<GetProfileImageCallback> _callback)
        {
            GetProfileImageCallback _profileCallback = new GetProfileImageCallback();
            Firebase.Storage.StorageReference avatar_ref = Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_request.Id).Child(AppSettings.UserAvatarKey + "/" + "Image_" + _request.Size.ToString() + ".jpg");
            avatar_ref.GetDownloadUrlAsync().ContinueWith((Task<Uri> task) =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    _profileCallback.IsSuccess = true;
                    _profileCallback.DownloadUrl = task.Result.ToString();
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
                else
                {
                    _profileCallback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
            });
        }

        public void UploadCustomBackgroundVaultPLUS(UploadImageRequest _request, Action<UploadImageCallBack> _callback)
        {
            UploadImageCallBack uploadCallback = new UploadImageCallBack();
            StorageReference background_ref = Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_request.OwnerId).Child(AppSettings.UserCustomBackgroundKey + "/" + "Image_" + _request.Size.ToString() + ".jpg");
            background_ref.PutBytesAsync(_request.ImageBytes).ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        print("immagine fallita");
                        uploadCallback.IsFinish = true;
                        uploadCallback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(uploadCallback));
                        CleanTask(task);
                    }
                    else
                    {
                        print("riscito immagine");
                        uploadCallback.IsFinish = true;
                        uploadCallback.IsSuccess = true;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(uploadCallback));
                        CleanTask(task);
                    }
                });
        }

        public void GetProfileCustomBackgroundVaultPLUS(GetProfileImageRequest _request, Action<GetProfileImageCallback> _callback)
        {
            GetProfileImageCallback _profileCallback = new GetProfileImageCallback();
            const long maxAllowedSize = 1 * 2048 * 2048;
            StorageReference background_ref = Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_request.Id).Child(AppSettings.UserCustomBackgroundKey + "/" + "Image_" + _request.Size.ToString() + ".jpg");
            background_ref.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _profileCallback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
                else
                {
                    byte[] fileContents = task.Result;
                    _profileCallback.IsSuccess = true;
                    _profileCallback.ImageBytes = fileContents;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
            });
        }

        public void GetCustomBackgroundUrl(GetProfileImageRequest _request, Action<GetProfileImageCallback> _callback)
        {
            GetProfileImageCallback _profileCallback = new GetProfileImageCallback();
            StorageReference background_ref = Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_request.Id).Child(AppSettings.UserCustomBackgroundKey + "/" + "Image_" + _request.Size.ToString() + ".jpg");
            background_ref.GetDownloadUrlAsync().ContinueWith((Task<Uri> task) =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    _profileCallback.IsSuccess = true;
                    _profileCallback.DownloadUrl = task.Result.ToString();
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
                else
                {
                    _profileCallback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_profileCallback));
                    CleanTask(task);
                }
            });
        }

        public void GetFeedsAt(FeedQuery _feedQuery)
        {
            LoadKeysFeeds(_feedQuery, Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID));
            /*Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().LimitToLast(2);
            }
            else if (_feedQuery.forward)
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(3);
            }
            else
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().StartAt(_feedQuery.indexKey).LimitToFirst(3);
            }

            FeedCallback _callback = new FeedCallback();
            databaseQuery.GetValueAsync().ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task1.IsCompleted)
                {
                    if (!task1.Result.Exists)
                    {
                        _callback.IsSuccess = true;
                        _callback.feeds = new List<Feed>();
                        _callback.forward = _feedQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task1);
                        return;
                    }
                    List<Feed> feeds = new List<Feed>();
                    for (int i = 0; i < task1.Result.ChildrenCount; i++)
                    {
                        Database.RootReference.Child(AppSettings.AllPostsKey).Child(task1.Result.Children.ElementAt(i).Key).GetValueAsync().ContinueWithOnMainThread(task =>
                        {
                            if (task.IsFaulted)
                            {
                                _callback.IsSuccess = false;
                                // Handle the error...
                                UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                print("err");
                            }
                            else if (task.IsCompleted)
                            {
                                if (!task1.Result.Exists)
                                {
                                    _callback.IsSuccess = true;
                                    _callback.feeds = new List<Feed>();
                                    _callback.forward = _feedQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                    CleanTask(task1);
                                    print("v");
                                    return;
                                }
                                _callback.IsSuccess = true;
                                DataSnapshot feedSnapshot = task.Result;
                                string _feedId = feedSnapshot.Key;
                                string jsonFeed = feedSnapshot.GetRawJsonValue();
                                if (!string.IsNullOrEmpty(jsonFeed))
                                {
                                    Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);
                                    if (_dataFeed != null)
                                    {
                                        _dataFeed.Key = _feedId;
                                        feeds.Add(_dataFeed);
                                    }
                                }
                                if (feeds.Count == task1.Result.ChildrenCount)
                                {
                                    _callback.IsSuccess = true;
                                    if (_feedQuery.forward)
                                        feeds.Reverse();

                                    _callback.feeds = feeds;
                                    _callback.forward = _feedQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                    CleanTask(task1);
                                }
                                CleanTask(task);
                            }
                        });
                    }
                }
            });
            /*Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().LimitToLast(2);
            }
            else if (_feedQuery.forward)
            {
                int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(3);
                //databaseQuery = Database.RootReference.Child(AppSettings.AllPostsKey).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(3);//EndAt(_feedQuery.indexKey)
            }
            else
            {
                int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().StartAt(_feedQuery.indexKey).LimitToFirst(3);
            }

            FeedCallback _callback = new FeedCallback();
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    print("errore");
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task.IsCompleted)
                {
                    if (!task.Result.Exists)
                    {
                        _callback.IsSuccess = true;
                        _callback.feeds = new List<Feed>();
                        _callback.forward = _feedQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task);
                        return;
                    }
                    _callback.IsSuccess = true;
                    DataSnapshot snapshot = task.Result;
                    List<Feed> feeds = new List<Feed>();
                    int _feedCount = (int)snapshot.ChildrenCount;
                    for (int i = 0; i < _feedCount; i++)
                    {
                        DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);
                        string _feedId = feedSnapshot.Key;

                        string jsonFeed = feedSnapshot.GetRawJsonValue();
                        if (!string.IsNullOrEmpty(jsonFeed))
                        {
                            Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);

                            if (_dataFeed != null)
                            {
                                _dataFeed.Key = _feedId;
                                feeds.Add(_dataFeed);
                            }
                        }
                        print(_feedId);
                    }
                    _callback.IsSuccess = true;
                    if (_feedQuery.forward)
                    {
                        feeds.Reverse();
                    }

                    _callback.feeds = feeds;
                    _callback.forward = _feedQuery.forward;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));

                    CleanTask(task);
                }
            });
            Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().LimitToLast(_feedQuery.endIndex);
            }
            else if (_feedQuery.forward)
            {
                int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(count);
            }
            else
            {
                int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feedQuery.ownerID).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(count);
            }
            FeedCallback _callback = new FeedCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _callback.IsSuccess = false;
                        // Handle the error...
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result.Exists)
                    {

                        DataSnapshot snapshot = task.Result;
                        List<Feed> feeds = new List<Feed>();

                        int _feedCount = (int)snapshot.ChildrenCount;

                        List<Task> TaskList = new List<Task>();

                        for (int i = 0; i < _feedCount; i++)
                        {
                            DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);
                            string _feedId = feedSnapshot.Key;
                            DatabaseReference _feedRef = Database.RootReference.Child(AppSettings.AllPostsKey).Child(_feedId);
                            Task<DataSnapshot> _task = _feedRef.GetValueAsync();
                            TaskList.Add(_task);

                        }
                        Task.WhenAll(TaskList).ContinueWith(task2 =>
                        {
                            if (task2.IsCompleted)
                            {
                                foreach (Task<DataSnapshot> t in TaskList)
                                {
                                    if (t.IsCompleted && t.Result.Exists)
                                    {
                                        string _feedId = t.Result.Key;
                                        string jsonFeed = t.Result.GetRawJsonValue();
                                        if (!string.IsNullOrEmpty(jsonFeed))
                                        {
                                            Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);

                                            if (_dataFeed != null)
                                            {
                                                _dataFeed.Key = _feedId;
                                                if (_feedQuery.forward)
                                                {
                                                    if (_feedId != _feedQuery.indexKey)
                                                    {
                                                        feeds.Add(_dataFeed);
                                                    }
                                                }
                                                else
                                                {
                                                    feeds.Add(_dataFeed);
                                                }
                                            }
                                        }
                                        CleanTask(t);
                                    }
                                }
                                CleanTask(task);
                            }
                            _callback.IsSuccess = true;
                            feeds = feeds.OrderBy(p => DateTime.Parse(p.DateCreated)).ToList();
                            feeds.Reverse();
                            if (!_feedQuery.forward)
                            {
                                feeds.Reverse();
                            }

                            _callback.feeds = feeds;
                            _callback.forward = _feedQuery.forward;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        });
                        CleanTask(task);
                    }
                });*/
        }

        public void CheckYouAreAFollower(string _id, Action<bool> action)
        {
            Database.RootReference.Child("FollowerUsers").Child(_id).Child("List").Child(Auth.CurrentUser.UserId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action(task.Result.Exists));
                    CleanTask(task);
                }
            });
        }

        public void FollowUser(string _id, Action<bool> action)
        {
            Database.RootReference.Child("FollowerUsers").Child(_id).Child("List").Child(Auth.CurrentUser.UserId).SetValueAsync(0).ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    Database.RootReference.Child("UserPosts").Child(_id).GetValueAsync().ContinueWith(task1 =>
                    {
                        if (task1.IsCanceled || task1.IsFaulted)
                            CleanTask(task1);
                        else if (task1.IsCompleted)
                        {
                            for (int i = 0; i < task1.Result.ChildrenCount; i++)
                                Database.RootReference.Child("FollowedPosts").Child(Auth.CurrentUser.UserId).Child(Convert.ToString(task1.Result.Children.ElementAt(i).Key)).SetValueAsync(0);
                        }
                    });
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action(true));
                    CleanTask(task);
                }
            });
        }

        public void StopFollowUser(string _id, Action<bool> action)
        {
            Database.RootReference.Child("FollowerUsers").Child(_id).Child("List").Child(Auth.CurrentUser.UserId).RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    Database.RootReference.Child("UserPosts").Child(_id).GetValueAsync().ContinueWith(task1 =>
                    {
                        if (task1.IsCanceled || task1.IsFaulted)
                            CleanTask(task1);
                        else if (task1.IsCompleted)
                        {
                            for (int i = 0; i < task1.Result.ChildrenCount; i++)
                                Database.RootReference.Child("FollowedPosts").Child(Auth.CurrentUser.UserId).Child(Convert.ToString(task1.Result.Children.ElementAt(i).Key)).RemoveValueAsync();
                        }
                    });
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action(true));
                    CleanTask(task);
                }
            });
        }

        /*public void GetFriendsFeedsAt(FeedQuery _feedQuery)
        {
            Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.FriendsPostsKey).Child(_feedQuery.ownerID).Child(AppSettings.ContainerListKey).OrderByKey().LimitToLast(_feedQuery.endIndex);
            }
            else if (_feedQuery.forward)
            {
                int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.FriendsPostsKey).Child(_feedQuery.ownerID).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(count);
            }
            else
            {
                int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.FriendsPostsKey).Child(_feedQuery.ownerID).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(count);
            }
            FeedCallback _callback = new FeedCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _callback.IsSuccess = false;
                        // Handle the error...
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result.Exists)
                    {

                        DataSnapshot snapshot = task.Result;
                        List<Feed> feeds = new List<Feed>();

                        int _feedCount = (int)snapshot.ChildrenCount;

                        List<Task> TaskList = new List<Task>();

                        for (int i = 0; i < _feedCount; i++)
                        {
                            DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);
                            string _feedId = feedSnapshot.Key;
                            DatabaseReference _feedRef = Database.RootReference.Child(AppSettings.AllPostsKey).Child(_feedId);
                            Task<DataSnapshot> _task = _feedRef.GetValueAsync();
                            TaskList.Add(_task);

                        }
                        Task.WhenAll(TaskList).ContinueWith(task2 =>
                        {
                            if (task2.IsCompleted)
                            {
                                foreach (Task<DataSnapshot> t in TaskList)
                                {
                                    if (t.IsCompleted && t.Result.Exists)
                                    {
                                        string _feedId = t.Result.Key;
                                        string jsonFeed = t.Result.GetRawJsonValue();
                                        if (!string.IsNullOrEmpty(jsonFeed))
                                        {
                                            Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);

                                            if (_dataFeed != null)
                                            {
                                                _dataFeed.Key = _feedId;
                                                if (_feedQuery.forward)
                                                {
                                                    if (_feedId != _feedQuery.indexKey)
                                                    {
                                                        feeds.Add(_dataFeed);
                                                    }
                                                }
                                                else
                                                {
                                                    feeds.Add(_dataFeed);
                                                }
                                            }
                                        }
                                        CleanTask(t);
                                    }
                                }
                                CleanTask(task);
                            }
                            _callback.IsSuccess = true;
                            feeds = feeds.OrderBy(p => DateTime.Parse(p.DateCreated)).ToList();
                            feeds.Reverse();
                            if (!_feedQuery.forward)
                            {
                                feeds.Reverse();
                            }

                            _callback.feeds = feeds;
                            _callback.forward = _feedQuery.forward;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        });
                        CleanTask(task);
                    }
                });
        }*/

        public void GetFriendsAt(UsersQuery _friendQuery)
        {
            Query databaseQuery;
            string rootKey = AppSettings.UserFriendsKey;
            if (_friendQuery.Type == FriendsTabState.Pending)
                rootKey = AppSettings.UserPendingFriendsKey;
            if (_friendQuery.Type == FriendsTabState.Requested)
                rootKey = AppSettings.UserRequestedFriendsKey;
            if (string.IsNullOrEmpty(_friendQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(rootKey).Child(_friendQuery.ownerID).Child(AppSettings.ContainerListKey).OrderByKey().LimitToLast(_friendQuery.endIndex);
            }
            else if (_friendQuery.forward)
            {
                int count = _friendQuery.endIndex - _friendQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(rootKey).Child(_friendQuery.ownerID).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_friendQuery.indexKey).LimitToLast(count);
            }
            else
            {
                int count = _friendQuery.endIndex - _friendQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(rootKey).Child(_friendQuery.ownerID).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_friendQuery.indexKey).LimitToLast(count);
            }

            UsersCallback _callback = new UsersCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _callback.IsSuccess = false;
                        // Handle the error...
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _friendQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result.Exists)
                    {
                        DataSnapshot snapshot = task.Result;
                        List<User> friends = new List<User>();

                        int _friendsCount = (int)snapshot.ChildrenCount;

                        List<Task> TaskList = new List<Task>();

                        for (int i = 0; i < _friendsCount; i++)
                        {
                            DataSnapshot friendSnapshot = snapshot.Children.ElementAt(i);
                            string _friendId = friendSnapshot.Key;
                            DatabaseReference _friendRef = Database.RootReference.Child(AppSettings.RootUserKey).Child(_friendId);
                            Task<DataSnapshot> _task = _friendRef.GetValueAsync();
                            TaskList.Add(_task);

                        }
                        Task.WhenAll(TaskList).ContinueWith(task2 =>
                        {
                            if (task2.IsCompleted)
                            {
                                foreach (Task<DataSnapshot> t in TaskList)
                                {
                                    if (t.IsCompleted && t.Result.Exists)
                                    {
                                        string _friendId = t.Result.Key;
                                        string jsonFriend = t.Result.GetRawJsonValue();
                                        if (!string.IsNullOrEmpty(jsonFriend))
                                        {
                                            User _dataFriend = JsonUtility.FromJson<User>(jsonFriend);

                                            if (_dataFriend != null)
                                            {
                                                _dataFriend.UserID = _friendId;
                                                if (_friendQuery.forward)
                                                {
                                                    if (_friendId != _friendQuery.indexKey)
                                                    {
                                                        friends.Add(_dataFriend);
                                                    }
                                                }
                                                else
                                                {
                                                    friends.Add(_dataFriend);
                                                }
                                            }
                                        }
                                        CleanTask(t);
                                    }
                                }
                            }
                            _callback.IsSuccess = true;
                            friends.Reverse();
                            if (!_friendQuery.forward)
                            {
                                friends.Reverse();
                            }
                            _callback.RequestID = _friendQuery.RequestID;
                            _callback.users = friends;
                            _callback.forward = _friendQuery.forward;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _friendQuery.callback.Invoke(_callback));
                            CleanTask(task2);
                        });
                        CleanTask(task);
                    }
                });
        }


        public void GetGroupUsersAt(UsersQuery _friendQuery)
        {
            Query databaseQuery;
            string rootKey = AppSettings.UserMessagesGroups;
            if (string.IsNullOrEmpty(_friendQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(rootKey).Child(_friendQuery.ownerID).Child(AppSettings.RootUserKey).OrderByKey().LimitToLast(_friendQuery.endIndex);
            }
            else if (_friendQuery.forward)
            {
                int count = _friendQuery.endIndex - _friendQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(rootKey).Child(_friendQuery.ownerID).Child(AppSettings.RootUserKey).OrderByKey().EndAt(_friendQuery.indexKey).LimitToLast(count);
            }
            else
            {
                int count = _friendQuery.endIndex - _friendQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(rootKey).Child(_friendQuery.ownerID).Child(AppSettings.RootUserKey).OrderByKey().EndAt(_friendQuery.indexKey).LimitToLast(count);
            }

            UsersCallback _callback = new UsersCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _callback.IsSuccess = false;
                        // Handle the error...
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _friendQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result.Exists)
                    {
                        DataSnapshot snapshot = task.Result;
                        List<User> friends = new List<User>();

                        int _friendsCount = (int)snapshot.ChildrenCount;

                        List<Task> TaskList = new List<Task>();

                        for (int i = 0; i < _friendsCount; i++)
                        {
                            DataSnapshot friendSnapshot = snapshot.Children.ElementAt(i);
                            string _friendId = friendSnapshot.Value.ToString();
                            DatabaseReference _friendRef = Database.RootReference.Child(AppSettings.RootUserKey).Child(_friendId);
                            Task<DataSnapshot> _task = _friendRef.GetValueAsync();
                            TaskList.Add(_task);

                        }
                        Task.WhenAll(TaskList).ContinueWith(task2 =>
                        {
                            if (task2.IsCompleted)
                            {
                                foreach (Task<DataSnapshot> t in TaskList)
                                {
                                    if (t.IsCompleted && t.Result.Exists)
                                    {
                                        string _friendId = t.Result.Key;
                                        string jsonFriend = t.Result.GetRawJsonValue();
                                        if (!string.IsNullOrEmpty(jsonFriend))
                                        {
                                            User _dataFriend = JsonUtility.FromJson<User>(jsonFriend);

                                            if (_dataFriend != null)
                                            {
                                                _dataFriend.UserID = _friendId;
                                                if (_friendQuery.forward)
                                                {
                                                    if (_friendId != _friendQuery.indexKey)
                                                    {
                                                        friends.Add(_dataFriend);
                                                    }
                                                }
                                                else
                                                {
                                                    friends.Add(_dataFriend);
                                                }
                                            }
                                        }
                                        CleanTask(t);
                                    }
                                }
                            }
                            _callback.IsSuccess = true;
                            friends.Reverse();
                            if (!_friendQuery.forward)
                            {
                                friends.Reverse();
                            }
                            _callback.RequestID = _friendQuery.RequestID;
                            _callback.users = friends;
                            _callback.forward = _friendQuery.forward;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _friendQuery.callback.Invoke(_callback));
                            CleanTask(task2);
                        });
                        CleanTask(task);
                    }
                });
        }

        public void GetWorldFeedsAt(FeedQuery _feedQuery)
        {
            LoadKeysFeeds(_feedQuery, Database.RootReference.Child(AppSettings.AllPostsKey));
            /*
            List<string> l = new List<string>(0);
            Database.RootReference.Child(AppSettings.RootUserKey).Child(Auth.CurrentUser.UserId).Child("blockedUser").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    for (int i = 0; i < task.Result.ChildrenCount; i++)
                        l.Add((string)task.Result.Children.ElementAt(i).Value);
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
            });
            
            Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.AllPostsKey).OrderByKey().LimitToLast(2);
            }
            else if (_feedQuery.forward)
            {
                databaseQuery = Database.RootReference.Child(AppSettings.AllPostsKey).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(3);
            }
            else
            {
                databaseQuery = Database.RootReference.Child(AppSettings.AllPostsKey).OrderByKey().StartAt(_feedQuery.indexKey).LimitToFirst(3);
            }

            FeedCallback _callback = new FeedCallback();
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task.IsCompleted)
                {
                    if (!task.Result.Exists)
                    {
                        _callback.IsSuccess = true;
                        _callback.feeds = new List<Feed>();
                        _callback.forward = _feedQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task);
                        return;
                    }
                    _callback.IsSuccess = true;
                    DataSnapshot snapshot = task.Result;
                    List<Feed> feeds = new List<Feed>();

                    int _feedCount = (int)snapshot.ChildrenCount;

                    for (int i = 0; i < _feedCount; i++)
                    {
                        DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);
                        string _feedId = feedSnapshot.Key;
                        string jsonFeed = feedSnapshot.GetRawJsonValue();
                        if (!string.IsNullOrEmpty(jsonFeed))
                        {
                            Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);
                            
                            if (_dataFeed != null)
                            {
                                if (l.Contains(_dataFeed.ToUserID))
                                {
                                    continue;
                                }

                                _dataFeed.Key = _feedId;
                                feeds.Add(_dataFeed);
                            }
                        }
                    }
                    _callback.IsSuccess = true;
                    if (_feedQuery.forward)
                    {
                        feeds.Reverse();
                    }

                    _callback.feeds = feeds;
                    _callback.forward = _feedQuery.forward;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));

                    CleanTask(task);
                }
            });*/
        }

        public void GetWorldFeedsAtRandom(FeedQuery _feedQuery)
        {
            FeedCallback _callback = new FeedCallback();
            Database.RootReference.Child("AllRandomPosts").Child("number").GetValueAsync().ContinueWithOnMainThread(task1 =>
            {
                if (task1.IsFaulted)
                {
                    _callback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task1.IsCompleted)
                {
                    Database.RootReference.Child("AllRandomPosts").Child(UnityEngine.Random.Range(0, Convert.ToInt32(task1.Result.Value)).ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted)
                        {
                            _callback.IsSuccess = false;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        }
                        else if (task.IsCompleted)
                        {
                            if (!task.Result.Exists)
                            {
                                _callback.IsSuccess = true;
                                _callback.feeds = new List<Feed>();
                                _callback.forward = _feedQuery.forward;
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                CleanTask(task);
                                return;
                            }
                            _callback.IsSuccess = true;
                            DataSnapshot snapshot = task.Result;
                            List<Feed> feeds = new List<Feed>(0);
                            int _feedCount = (int)snapshot.ChildrenCount;
                            List<string> keys = new List<string>();
                            if (string.IsNullOrEmpty(_feedQuery.indexKey))
                            {
                                List<int> indexes = new List<int>();
                                void TrovaIndexes()
                                {
                                    int a = Convert.ToInt32(UnityEngine.Random.Range(0, snapshot.ChildrenCount - 1));
                                    int b = Convert.ToInt32(UnityEngine.Random.Range(0, snapshot.ChildrenCount - 1));
                                    if (a == b)
                                        TrovaIndexes();
                                    else
                                        indexes = new List<int>() { a, b };
                                }
                                TrovaIndexes();
                                for (int i = 0; i < 2; i++)
                                    keys.Add(snapshot.Children.ElementAt(indexes[i]).Key);
                            }
                            else if (_feedQuery.forward)
                            {
                                if (string.IsNullOrEmpty(_feedQuery.targetKey))
                                    keys.Add(snapshot.Children.ElementAt((int)UnityEngine.Random.Range(0, snapshot.ChildrenCount - 1)).Key);
                                else
                                    keys.Add(_feedQuery.targetKey);
                            }
                            else
                                keys.Add(_feedQuery.targetKey);
                            LoadFeeds(keys, _feedQuery);
                        /*
                        for (int i = 0; i < keys.Count; i++)
                        {
                            GetPostAt(keys[i], r =>
                            {
                                feeds.Add(r.feeds[0]);
                                if (feeds.Count == keys.Count)
                                {
                                    _callback.IsSuccess = true;
                                    _callback.feeds = feeds;
                                    _callback.forward = _feedQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                }
                            });
                /*Database.RootReference.Child("AllPosts").Child(keys[i]).GetValueAsync().ContinueWith(task1 =>
                {
                    DataSnapshot feedSnapshot = task1.Result;
                    string jsonFeed = feedSnapshot.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(jsonFeed))
                    {
                        Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);
                        if (_dataFeed != null)
                        {
                            _dataFeed.Key = feedSnapshot.Key;
                            print("m " + feeds.Count);
                            feeds.Add(_dataFeed);
                            print("l " + feeds.Count);
                        }
                    }
                    print(feeds.Count + "  " + keys.Count);
                    if (feeds.Count == keys.Count && !fine)
                    {
                        print("oo");
                        fine = true;
                        _callback.IsSuccess = true;
                        _callback.feeds = feeds;
                        _callback.forward = _feedQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                    }
                    CleanTask(task1);
                });*/
                            CleanTask(task);
                        }
                    });
                }
            });
        }

        public void GetPostAt(string _id, Action<FeedCallback> action)
        {
            FeedCallback _callback = new FeedCallback();
            Database.RootReference.Child(AppSettings.AllPostsKey).Child(_id).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action.Invoke(_callback));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    if (!task.Result.Exists)
                    {
                        _callback.IsSuccess = true;
                        _callback.feeds = new List<Feed>() { FeedViewController.postEliminato };
                        _callback.feeds[0].Key = _id;
                        _callback.forward = true;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => action.Invoke(_callback));
                        CleanTask(task);
                        return;
                    }
                    DataSnapshot feedSnapshot = task.Result;
                    List<Feed> feeds = new List<Feed>();

                    string _feedId = feedSnapshot.Key;

                    string jsonFeed = feedSnapshot.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(jsonFeed))
                    {
                        Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);
                        _dataFeed.Key = _feedId;
                        feeds.Add(_dataFeed);
                    }
                    _callback.IsSuccess = true;
                    _callback.feeds = feeds;
                    _callback.forward = true;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action.Invoke(_callback));
                    CleanTask(task);
                }
            });
        }

        public void GetMainFeedsAt(FeedQuery _feedQuery)
        {/*
            List<string> l = new List<string>(0);
            Database.RootReference.Child(AppSettings.RootUserKey).Child(Auth.CurrentUser.UserId).Child("blockedUser").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    for (int i = 0; i < task.Result.ChildrenCount; i++)
                        l.Add((string)task.Result.Children.ElementAt(i).Value);
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
            });
            */
            LoadKeysFeeds(_feedQuery, Database.RootReference.Child(AppSettings.PremiumPosts));
        }

        void LoadKeysFeeds(FeedQuery _feedQuery, Query _databaseQuery)
        {
            Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
                databaseQuery = _databaseQuery.OrderByKey().LimitToLast(2);
            else if (_feedQuery.forward)
            {
                if (string.IsNullOrEmpty(_feedQuery.targetKey))
                    databaseQuery = _databaseQuery.OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(2);
                else
                    databaseQuery = null;
            }
            else
                databaseQuery = null;

            FeedCallback _callback = new FeedCallback();
            if (databaseQuery != null)
            {
                databaseQuery.GetValueAsync().ContinueWith(task1 =>
                {
                    if (task1.IsFaulted || task1.IsCanceled)
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                    }
                    else if (task1.IsCompleted)
                    {
                        if (!task1.Result.Exists)
                        {
                            _callback.IsSuccess = true;
                            _callback.feeds = new List<Feed>();
                            _callback.forward = _feedQuery.forward;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                            CleanTask(task1);
                            return;
                        }
                        List<string> keys = new List<string>();
                        for (int i = 0; i < task1.Result.ChildrenCount; i++)
                            keys.Add(task1.Result.Children.ElementAt(i).Key);
                        LoadFeeds(keys, _feedQuery);
                        CleanTask(task1);
                    }
                });
            }
            else
                LoadFeeds(new List<string>() { _feedQuery.targetKey }, _feedQuery);
        }

        void LoadFeeds(List<string> keysList, FeedQuery _feedQuery)
        {
            FeedCallback _callback = new FeedCallback();
            List<Feed> feeds = new List<Feed>();
            for (int i = 0; i < keysList.Count; i++)
            {
                if (keysList[i] != _feedQuery.indexKey)
                {
                    GetPostAt(keysList[i], result =>
                    {
                        if (!result.IsSuccess)
                        {
                            _callback.IsSuccess = false;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        }
                        else
                        {
                            feeds.Add(result.feeds[0]);
                            if ((string.IsNullOrEmpty(_feedQuery.indexKey) && keysList.Count == feeds.Count) || !string.IsNullOrEmpty(_feedQuery.indexKey))
                            {
                                _callback.IsSuccess = true;
                                _callback.feeds = feeds.OrderBy(x => x.Key).ToList();
                                _callback.feeds.Reverse();
                                _callback.forward = _feedQuery.forward;
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                            }
                        }
                    });
                }
                else if (keysList.Count == 1)
                {
                    _callback.IsSuccess = true;
                    _callback.feeds = new List<Feed>();
                    _callback.forward = _feedQuery.forward;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
            }
        }

        public void GetFollowedFeedsAt(FeedQuery _feedQuery)
        {
            LoadKeysFeeds(_feedQuery, Database.RootReference.Child("FollowedPosts").Child(Auth.CurrentUser.UserId));
            /*Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child("FollowedPosts").Child(Auth.CurrentUser.UserId).OrderByKey().LimitToLast(2);
            }
            else if (_feedQuery.forward)
            {
                databaseQuery = Database.RootReference.Child("FollowedPosts").Child(Auth.CurrentUser.UserId).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(3);
            }
            else
            {
                databaseQuery = Database.RootReference.Child("FollowedPosts").Child(Auth.CurrentUser.UserId).OrderByKey().StartAt(_feedQuery.indexKey).LimitToFirst(3);
            }

            FeedCallback _callback = new FeedCallback();
            databaseQuery.GetValueAsync().ContinueWith(task1 =>
            {
                if (task1.IsFaulted || task1.IsCanceled)
                {
                    print("nulla1");
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task1.IsCompleted)
                {
                    if (!task1.Result.Exists)
                    {
                        print("nulla0");
                        _callback.IsSuccess = true;
                        _callback.feeds = new List<Feed>();
                        _callback.forward = _feedQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task1);
                        return;
                    }
                    List<Feed> feeds = new List<Feed>();
                    for (int i = 0; i < task1.Result.ChildrenCount; i++)
                    {
                        Database.RootReference.Child(AppSettings.AllPostsKey).Child(task1.Result.Children.ElementAt(i).Key).GetValueAsync().ContinueWithOnMainThread(task =>
                        {
                            if (task.IsFaulted)
                            {
                                print(task.IsFaulted + "  " + task.IsCanceled + "  ");
                                print(task.Exception.ToString());
                                _callback.IsSuccess = false;
                                // Handle the error...
                                UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                            }
                            else if (task.IsCompleted)
                            {
                                if (!task.Result.Exists)
                                {
                                    _callback.IsSuccess = true;
                                    _callback.feeds = new List<Feed>();
                                    _callback.forward = _feedQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                    CleanTask(task1);
                                    return;
                                }
                                _callback.IsSuccess = true;
                                DataSnapshot feedSnapshot = task.Result;
                                string _feedId = feedSnapshot.Key;
                                string jsonFeed = feedSnapshot.GetRawJsonValue();
                                if (!string.IsNullOrEmpty(jsonFeed))
                                {
                                    Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);
                                    if (_dataFeed != null)
                                    {
                                        _dataFeed.Key = _feedId;
                                        feeds.Add(_dataFeed);
                                    }
                                }
                                if (feeds.Count == task1.Result.ChildrenCount)
                                {
                                    _callback.IsSuccess = true;
                                    if (_feedQuery.forward)
                                        feeds.Reverse();

                                    _callback.feeds = feeds;
                                    _callback.forward = _feedQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                    CleanTask(task1);
                                }
                                CleanTask(task);
                            }
                        });
                    }
                }
            });*/
        }

        public void GetReportsFeedsAt(FeedQuery _feedQuery)
        {
            LoadKeysFeeds(_feedQuery, Database.RootReference.Child(AppSettings.ReportedPosts));
            /*
            Query databaseQuery;
            if (string.IsNullOrEmpty(_feedQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.ReportedPosts).OrderByKey().LimitToLast(2);
            }
            else if (_feedQuery.forward)
            {
                databaseQuery = Database.RootReference.Child(AppSettings.ReportedPosts).OrderByKey().EndAt(_feedQuery.indexKey).LimitToLast(3);
            }
            else
            {
                databaseQuery = Database.RootReference.Child(AppSettings.ReportedPosts).OrderByKey().StartAt(_feedQuery.indexKey).LimitToFirst(3);
            }

            FeedCallback _callback = new FeedCallback();
            databaseQuery.GetValueAsync().ContinueWith(task1 =>
            {
                if (task1.IsFaulted)
                {
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task1.IsCompleted)
                {
                    if (!task1.Result.Exists)
                    {
                        _callback.IsSuccess = true;
                        _callback.feeds = new List<Feed>();
                        _callback.forward = _feedQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                        CleanTask(task1);
                        return;
                    }    
                    List<Feed> feeds = new List<Feed>();
                    for (int i = 0; i < task1.Result.ChildrenCount; i++)
                    {
                        print(task1.Result.Children.ElementAt(i).Key);
                        Database.RootReference.Child(AppSettings.AllPostsKey).Child(task1.Result.Children.ElementAt(i).Key).GetValueAsync().ContinueWithOnMainThread(task =>
                        {
                            if (task.IsFaulted)
                            {
                                print("errore");
                                _callback.IsSuccess = false;
                                // Handle the error...
                                UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                            }
                            else if (task.IsCompleted && task.Result.Exists)
                            {
                                _callback.IsSuccess = true;
                                DataSnapshot feedSnapshot = task.Result;
                                string _feedId = feedSnapshot.Key;
                                string jsonFeed = feedSnapshot.GetRawJsonValue();
                                print(_feedId);
                                if (!string.IsNullOrEmpty(jsonFeed))
                                {
                                    Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);
                                    if (_dataFeed != null)
                                    {
                                        _dataFeed.Key = _feedId;
                                        feeds.Add(_dataFeed);
                                    }
                                }
                                if (feeds.Count == task1.Result.ChildrenCount)
                                {
                                    _callback.IsSuccess = true;
                                    if (_feedQuery.forward)
                                        feeds.Reverse();

                                    _callback.feeds = feeds;
                                    _callback.forward = _feedQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                                    CleanTask(task1);
                                }
                                CleanTask(task);
                            }
                        });
                    }
                }
            });
            /*List<string> l = new List<string>(0);
            Database.RootReference.Child(AppSettings.ReportedPosts).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    for (int i = 0; i < task.Result.ChildrenCount; i++)
                        l.Add((string)task.Result.Children.ElementAt(i).Key);
                    print(l[0]);
                }
                else if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
            });

            Query databaseQuery;
            
            int count = _feedQuery.endIndex - _feedQuery.startIndex + 1;
            databaseQuery = Database.RootReference.Child(AppSettings.AllPostsKey);
            FeedCallback _callback = new FeedCallback();
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _callback.IsSuccess = false;
                    // Handle the error...
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));
                }
                else if (task.IsCompleted && task.Result.Exists)
                {
                    _callback.IsSuccess = true;
                    DataSnapshot snapshot = task.Result;
                    List<Feed> feeds = new List<Feed>();
                    
                    int _feedCount = (int)snapshot.ChildrenCount;

                    for (int i = 0; i < _feedCount; i++)
                    {
                        DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);
                        string _feedId = feedSnapshot.Key;

                        string jsonFeed = feedSnapshot.GetRawJsonValue();
                        if (!string.IsNullOrEmpty(jsonFeed))
                        {
                            Feed _dataFeed = JsonUtility.FromJson<Feed>(jsonFeed);

                            if (_dataFeed != null)
                            {
                                if (l.Contains(_feedId))
                                {
                                    print("ccc");
                                    _dataFeed.Key = _feedId;
                                    if (_feedQuery.forward)
                                    {
                                        if (_feedId != _feedQuery.indexKey)
                                        {
                                            if (!premium)
                                            {
                                                feeds.Add(_dataFeed);
                                            }
                                            else if (_dataFeed.premium)
                                            {
                                                feeds.Add(_dataFeed);
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                feeds.Add(_dataFeed);
                            }
                        }
                    }
                    _callback.IsSuccess = true;
                    feeds.Reverse();
                    if (!_feedQuery.forward)
                    {
                        feeds.Reverse();
                    }

                    _callback.feeds = feeds;
                    _callback.forward = _feedQuery.forward;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _feedQuery.callback.Invoke(_callback));

                    CleanTask(task);
                }
            });*/
        }

        public void GetMessageListAt(MessageListQuery _listQuery)
        {
            Query databaseQuery;
            if (string.IsNullOrEmpty(_listQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesList).Child(_listQuery.ownerID).OrderByKey().LimitToLast(_listQuery.endIndex);
            }
            else if (_listQuery.forward)
            {
                int count = _listQuery.endIndex - _listQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesList).Child(_listQuery.ownerID).OrderByKey().EndAt(_listQuery.indexKey).LimitToLast(count);
            }
            else
            {
                int count = _listQuery.endIndex - _listQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesList).Child(_listQuery.ownerID).OrderByKey().EndAt(_listQuery.indexKey).LimitToLast(count);
            }

            MessageListCallback _callback = new MessageListCallback();
            databaseQuery
                .GetValueAsync().ContinueWithOnMainThread(task =>
                {

                    if (task.IsCompleted && task.Result.Exists)
                    {
                        _callback.IsSuccess = true;
                        DataSnapshot snapshot = task.Result;
                        List<string> messages = new List<string>();

                        int _feedCount = (int)snapshot.ChildrenCount;

                        for (int i = 0; i < _feedCount; i++)
                        {
                            DataSnapshot userSnapshot = snapshot.Children.ElementAt(i);
                            string _childId = userSnapshot.Key;
                            string _userId = userSnapshot.Value.ToString();

                            string jsonFeed = userSnapshot.GetRawJsonValue();
                            if (!string.IsNullOrEmpty(jsonFeed))
                            {
                                if (_listQuery.forward)
                                {
                                    if (_childId != _listQuery.indexKey)
                                    {
                                        messages.Add(_userId);
                                    }
                                }
                                else
                                {
                                    messages.Add(_userId);
                                }

                            }
                        }
                        _callback.IsSuccess = true;
                        messages.Reverse();
                        if (!_listQuery.forward)
                        {
                            messages.Reverse();
                        }

                        _callback.usersIds = messages;
                        _callback.forward = _listQuery.forward;
                        _listQuery.callback.Invoke(_callback);
                        CleanTask(task);
                    }
                    else
                    {
                        Debug.Log("fail to get form DB ");
                        _callback.IsSuccess = false;
                        // Handle the error...
                        AppManager.VIEW_CONTROLLER.HideLoading();
                        _listQuery.callback.Invoke(_callback);
                        CleanTask(task);
                    }
                });
        }

        public void UploadFile(FileUploadRequset _request, Action<FileUploadCallback> _callback)
        {
            FileUploadCallback _uploadCallback = new FileUploadCallback();

            string pathKey = string.Empty;
            if (_request.FeedType == FeedType.Video)
            {
                pathKey = AppSettings.FeedUploadVideoPath;
            }
            if (_request.FeedType == FeedType.Image)
            {
                pathKey = AppSettings.FeedUploadImagePath;
            }
            StorageReference upload_ref = Storage.RootReference.Child(pathKey + _request.FileName);

            upload_ref.PutBytesAsync(_request.UploadBytes).ContinueWith((Task<StorageMetadata> task) =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        _uploadCallback.IsComplete = true;
                        _uploadCallback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                        CleanTask(task);
                    }
                    else
                    {
                        StorageMetadata metadata = task.Result;
                        metadata.Reference.GetDownloadUrlAsync().ContinueWith((Task<Uri> task2) =>
                        {
                            if (!task2.IsFaulted && !task2.IsCanceled)
                            {
                                _uploadCallback.IsComplete = true;
                                _uploadCallback.IsSuccess = true;
                                _uploadCallback.DownloadUrl = task2.Result.ToString();
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                                CleanTask(task2);
                            }
                            else
                            {
                                _uploadCallback.IsComplete = true;
                                _uploadCallback.IsSuccess = false;
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                                CleanTask(task2);
                            }
                        });
                        CleanTask(task);
                    }
                });
        }

        public void AddNewPost(Feed _feed, Action<FeedUploadCallback> _callback)
        {
            FeedUploadCallback _uploadCallback = new FeedUploadCallback();
            DatabaseReference _feedref = Database.RootReference.Child(AppSettings.AllPostsKey).Push();

            string _feedKey = _feedref.Key;
            _feed.Key = _feedKey;
            string json = JsonUtility.ToJson(_feed);
            _feedref.SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _uploadCallback.IsSuccess = false;
                    _uploadCallback.IsComplete = true;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                    CleanTask(task);
                }
                else
                {
                    DatabaseReference _feedLinkRef = Database.RootReference.Child(AppSettings.UserPostsKey).Child(_feed.ToUserID).Child(_feedKey);
                    _feedLinkRef.SetValueAsync(0).ContinueWith(task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            _uploadCallback.IsSuccess = false;
                            _uploadCallback.IsComplete = true;
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                            CleanTask(task2);
                        }
                        else
                        {
                            Database.RootReference.Child("FollowerUsers").Child(_feed.ToUserID).Child("List").GetValueAsync().ContinueWith(task4 =>
                            {
                                if (task4.IsFaulted || task4.IsCanceled)
                                {
                                    _uploadCallback.IsSuccess = false;
                                    _uploadCallback.IsComplete = true;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                                    CleanTask(task4);
                                }
                                else if (task4.IsCompleted)
                                {
                                    for (int i = 0; i < task4.Result.ChildrenCount; i++)
                                    {
                                        Database.RootReference.Child("FollowedPosts").Child(task4.Result.Children.ElementAt(i).Key).Child(_feedKey).SetValueAsync(0).ContinueWith(task3 =>
                                        {
                                            if (task3.IsFaulted || task3.IsCanceled)
                                            {
                                                _uploadCallback.IsSuccess = false;
                                                _uploadCallback.IsComplete = true;
                                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                                                CleanTask(task3);
                                            }
                                            else
                                            {
                                                Debug.Log("Upload to my feed complete");
                                                _uploadCallback.IsSuccess = true;
                                                _uploadCallback.IsComplete = true;

                                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_uploadCallback));
                                                CleanTask(task3);
                                            }
                                        });
                                    }
                                    CleanTask(task4);
                                }
                            });
                            CleanTask(task2);
                        }
                    });
                    CleanTask(task);
                }
            });
        }

        public void SearchUsers(UsersQuery _userQuery, string _search)
        {
            Query databaseQueryFirstName = Database.RootReference.Child(AppSettings.RootUserKey).OrderByChild(AppSettings.UserFirstNameKey).StartAt(_search).EndAt(_search + "\uf8ff");
            Query databaseQueryLastName = Database.RootReference.Child(AppSettings.RootUserKey).OrderByChild(AppSettings.UserLastNameKey).StartAt(_search).EndAt(_search + "\uf8ff");
            Query databaseQueryFullName = Database.RootReference.Child(AppSettings.RootUserKey).OrderByChild(AppSettings.UserFullNameKey).StartAt(_search).EndAt(_search + "\uf8ff");
            Query databaseQueryPhone = Database.RootReference.Child(AppSettings.RootUserKey).OrderByChild(AppSettings.UserPhoneKey).StartAt(_search).EndAt(_search + "\uf8ff");


            UsersCallback _callback = new UsersCallback();

            List<Task> TaskList = new List<Task>();

            TaskList.Add(databaseQueryFirstName.GetValueAsync());
            TaskList.Add(databaseQueryLastName.GetValueAsync());
            TaskList.Add(databaseQueryFullName.GetValueAsync());
            TaskList.Add(databaseQueryPhone.GetValueAsync());

            Task.WhenAll(TaskList).ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    _callback.IsSuccess = true;
                    List<User> users = new List<User>();
                    List<string> usersKeys = new List<string>();
                    foreach (Task<DataSnapshot> t in TaskList)
                    {
                        if (t.IsCompleted && t.Result.Exists)
                        {
                            DataSnapshot snapshot = t.Result;

                            for (int i = 0; i < snapshot.ChildrenCount; i++)
                            {
                                DataSnapshot userSnapshot = snapshot.Children.ElementAt(i);
                                string jsonMessage = userSnapshot.GetRawJsonValue();
                                if (!string.IsNullOrEmpty(jsonMessage))
                                {
                                    User _dataUser = JsonUtility.FromJson<User>(jsonMessage);
                                    //_dataUser.Key = userSnapshot.Key;
                                    if (_dataUser != null)
                                    {
                                        if (userSnapshot.Key != _userQuery.indexKey && !AppManager.USER_PROFILE.IsMine(_dataUser.UserID))
                                        {
                                            if (!usersKeys.Contains(userSnapshot.Key))
                                            {
                                                users.Add(_dataUser);
                                                usersKeys.Add(userSnapshot.Key);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!_userQuery.forward)
                    {
                        users.Reverse();
                    }
                    _callback.RequestID = _userQuery.RequestID;
                    _callback.users = users;
                    _callback.forward = _userQuery.forward;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _userQuery.callback.Invoke(_callback));
                    CleanTask(task2);
                }

            });
        }

        public void GetUserDateRegistration(string _userID, Action<string> _callback)
        {
            Query databaseQuery = Database.RootReference.Child(AppSettings.RootUserKey).Child(_userID).Child("DataRegistration");
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result.Exists)
                    {
                        if (string.IsNullOrEmpty(task.Result.Value.ToString()))
                        {
                            print("a");
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke("!Errore1111"));
                        }
                        else
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(task.Result.Value.ToString()));
                        }
                    }
                    CleanTask(task);
                }
                else
                {
                    print("a");
                    CleanTask(task);
                }
            });
        }

        public void GetUserFullName(string _userID, Action<string> _callback)
        {
            Query databaseQuery = Database.RootReference.Child(AppSettings.RootUserKey).Child(_userID).Child(AppSettings.UserFullNameKey);
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    print("fallito");
                if (task.IsCompleted)
                {
                    if (task.Result.Exists)
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(task.Result.Value)))
                        {
                            print("a");
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke("!Errore1111"));
                        }
                        else
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(Convert.ToString(task.Result.Value)));
                    }
                    else
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke("NoName"));
                    CleanTask(task);
                }
                else
                {
                    print("a");
                    CleanTask(task);
                }
            });
        }

        public void GetUserFriendsCount(string _userID, Action<string> _callback)
        {
            Query databaseQuery = Database.RootReference.Child(AppSettings.UserFriendsKey).Child(_userID).Child(AppSettings.ListCountKey);
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result != null && task.Result.Value != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(task.Result.Value.ToString()));
                    }
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void GetPostLikesCount(string _postID, Action<string> _callback)
        {
            Query databaseQuery = Database.RootReference.Child(AppSettings.PostLikesKey).Child(_postID).Child(AppSettings.ListCountKey);
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result != null && task.Result.Value != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(task.Result.Value.ToString()));
                    }
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void AddPostInteraction(string _postID, string _userID, InteractionType _type, Action<bool> _callback)
        {
            Interaction _interaction = new Interaction() { UserID = AppManager.USER_PROFILE.FIREBASE_USER.UserId, Date = DateTime.UtcNow.ToString(), Type = (int)_type, PostID = _postID};

            Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_userID).Child("List").Push().SetRawJsonValueAsync(JsonUtility.ToJson(_interaction)).ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void RemovePostInteraction(string _postID, string _userID, InteractionType _type, Action<bool> _callback)
        {
            Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_userID).Child("List").GetValueAsync().ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    for (int i = 0; i < task2.Result.ChildrenCount; i++)
                    {
                        DataSnapshot snapshot = task2.Result.Children.ElementAt(i);
                        if ((string)snapshot.Child("PostID").Value == _postID && (string)snapshot.Child("UserID").Value == AppManager.USER_PROFILE.FIREBASE_USER.UserId && Convert.ToInt32(snapshot.Child("Type").Value) == (int)_type)
                        {
                            if (task2.Result.ChildrenCount != 1)
                                snapshot.Reference.RemoveValueAsync();
                            else
                                task2.Result.Reference.Parent.RemoveValueAsync();
                            break;
                        }
                    }
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void LikePost(string _postID, Action<bool> _callback)
        {
            Task fTask = Database.RootReference.Child(AppSettings.PostLikesKey).Child(_postID).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).SetValueAsync(0);

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void DislikePost(string _postID, Action<bool> _callback)
        {
            Task fTask = Database.RootReference.Child(AppSettings.PostDislikesKey).Child(_postID).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).SetValueAsync(0);

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void UnLikPost(string _postID, Action<bool> _callback)
        {
            Task fTask = Database.RootReference.Child(AppSettings.PostLikesKey).Child(_postID).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).RemoveValueAsync();

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void UnDislikPost(string _postID, Action<bool> _callback)
        {
            Task fTask = Database.RootReference.Child(AppSettings.PostDislikesKey).Child(_postID).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).RemoveValueAsync();

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void IsLikedPost(string _postID, Action<bool> _callback)
        {

            Task<DataSnapshot> fTask = Database.RootReference.Child(AppSettings.PostLikesKey).Child(_postID).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).GetValueAsync();

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    if (fTask.Result == null || fTask.Result.Value == null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    }
                    else
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    }
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void IsDislikedPost(string _postID, Action<bool> _callback)
        {
            Task<DataSnapshot> fTask = Database.RootReference.Child(AppSettings.PostDislikesKey).Child(_postID).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).GetValueAsync();

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    if (fTask.Result == null || fTask.Result.Value == null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    }
                    else
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    }
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void GetPostCommentsCount(string _postID, Action<string> _callback)
        {
            Query databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(_postID).Child(AppSettings.ListCountKey);
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result != null && task.Result.Value != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(task.Result.Value.ToString()));
                    }
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void GetLastMessageWithUser(string _userId, Action<string> _callback)
        {
            GetMessageReferece(_userId).LimitToLast(1).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Message _dataMsg = JsonUtility.FromJson<Message>(task.Result.Children.ElementAt<DataSnapshot>(0).GetRawJsonValue());
                    string _body = string.Empty;
                    if (_dataMsg.Type == ContentMessageType.TEXT) _body = _dataMsg.BodyTXT;
                    if (_dataMsg.Type == ContentMessageType.IMAGE) _body = _dataMsg.Type.ToString();
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_body));
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void GetLastMessageAtGroup(string _chatId, Action<string> _callback)
        {
            GetGroupChatReferece(_chatId).LimitToLast(1).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Message _dataMsg = JsonUtility.FromJson<Message>(task.Result.Children.ElementAt<DataSnapshot>(0).GetRawJsonValue());
                    string _body = string.Empty;
                    if (_dataMsg.Type == ContentMessageType.TEXT) _body = _dataMsg.BodyTXT;
                    if (_dataMsg.Type == ContentMessageType.IMAGE) _body = _dataMsg.Type.ToString();
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_body));
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }


        public void GetGroupMessagesAt(MessagesQuery _messageQuery)
        {
            string messageKey = _messageQuery.UserId;
            Query databaseQuery;
            if (string.IsNullOrEmpty(_messageQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().LimitToLast(_messageQuery.endIndex);
            }
            else if (_messageQuery.forward)
            {
                int count = _messageQuery.endIndex - _messageQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().StartAt(_messageQuery.indexKey).LimitToFirst(count);
            }
            else
            {
                int count = _messageQuery.endIndex - _messageQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_messageQuery.indexKey).LimitToLast(count);
            }
            MessagesCallback _callback = new MessagesCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                    {
                        _callback.IsSuccess = true;
                        DataSnapshot snapshot = task.Result;
                        List<Message> feeds = new List<Message>();
                        for (int i = 0; i < snapshot.ChildrenCount; i++)
                        {
                            DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);

                            string jsonMessage = feedSnapshot.GetRawJsonValue();
                            Message _dataFeed = JsonUtility.FromJson<Message>(jsonMessage);
                            _dataFeed.Key = feedSnapshot.Key;
                            if (_dataFeed != null)
                            {
                                if (feedSnapshot.Key != _messageQuery.indexKey)
                                {
                                    feeds.Add(_dataFeed);
                                }
                            }

                        }
                        if (!_messageQuery.forward)
                        {
                            feeds.Reverse();
                        }
                        _callback.messages = feeds;
                        _callback.forward = _messageQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                });
        }

        public void GetMessagesAt(MessagesQuery _messageQuery)
        {
            string messageKey = GetUserMessageKey(AppManager.USER_PROFILE.FIREBASE_USER.UserId, _messageQuery.UserId);
            Query databaseQuery;
            if (string.IsNullOrEmpty(_messageQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().LimitToLast(_messageQuery.endIndex);
            }
            else if (_messageQuery.forward)
            {
                int count = _messageQuery.endIndex - _messageQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().StartAt(_messageQuery.indexKey).LimitToFirst(count);
            }
            else
            {
                int count = _messageQuery.endIndex - _messageQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_messageQuery.indexKey).LimitToLast(count);
            }
            MessagesCallback _callback = new MessagesCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                    {
                        _callback.IsSuccess = true;
                        DataSnapshot snapshot = task.Result;
                        List<Message> feeds = new List<Message>();
                        for (int i = 0; i < snapshot.ChildrenCount; i++)
                        {
                            DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);

                            string jsonMessage = feedSnapshot.GetRawJsonValue();
                            Message _dataFeed = JsonUtility.FromJson<Message>(jsonMessage);
                            _dataFeed.Key = feedSnapshot.Key;
                            if (_dataFeed != null)
                            {
                                if (feedSnapshot.Key != _messageQuery.indexKey)
                                {
                                    feeds.Add(_dataFeed);
                                }
                            }

                        }
                        if (!_messageQuery.forward)
                        {
                            feeds.Reverse();
                        }
                        _callback.messages = feeds;
                        _callback.forward = _messageQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                });
        }

        public void GetCommentsAt(MessagesQuery _messageQuery)
        {
            string messageKey = _messageQuery.UserId;
            Query databaseQuery;
            if (string.IsNullOrEmpty(_messageQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().LimitToLast(7);
            }
            else if (_messageQuery.forward)
            {
                databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_messageQuery.indexKey).LimitToLast(8);
            }
            else
            {
                databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().StartAt(_messageQuery.indexKey).LimitToFirst(8);
            }
            MessagesCallback _callback = new MessagesCallback();
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _callback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                    CleanTask(task);
                }
                else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                {
                    _callback.IsSuccess = true;
                    DataSnapshot snapshot = task.Result;
                    List<Message> feeds = new List<Message>();
                    bool uguale = false;
                    for (int i = 0; i < snapshot.ChildrenCount; i++)
                    {
                        DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);

                        string jsonMessage = feedSnapshot.GetRawJsonValue();
                        Message _dataFeed = JsonUtility.FromJson<Message>(jsonMessage);
                        _dataFeed.Key = feedSnapshot.Key;
                        //print(2 + " " + i);
                        if (_dataFeed != null)
                        {
                            void Fatto()
                            {
                                List<Message> f = new List<Message>();
                                static int Compara(string a, string b) => DateTime.Parse(a, System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) > DateTime.Parse(b, System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) ? 1 : -1;
                                feeds.Sort((a, b) => Compara(a.DateCreated, b.DateCreated));
                                _callback.messages = feeds;
                                _callback.forward = _messageQuery.forward;
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                                CleanTask(task);
                            }
                            if (feedSnapshot.Key != _messageQuery.indexKey)
                            {
                                GetUserFullName(_dataFeed.UserID, _name =>
                                {
                                    _dataFeed.FullName = _name;
                                    feeds.Add(_dataFeed);
                                    if (snapshot.ChildrenCount == feeds.Count || (snapshot.ChildrenCount == feeds.Count + 1 && uguale))
                                        Fatto();
                                });
                            }
                            else
                            {
                                uguale = true;
                                if (snapshot.ChildrenCount == 1)
                                {
                                    _callback.messages = new List<Message>();
                                    _callback.forward = _messageQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                                    CleanTask(task);
                                }
                            }

                        }
                    }
                }
                else
                {
                    _callback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                    CleanTask(task);
                }
            });
            /*string messageKey = _messageQuery.UserId;
            Query databaseQuery;
            if (string.IsNullOrEmpty(_messageQuery.indexKey))
            {
                databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().LimitToLast(_messageQuery.endIndex);
            }
            else if (_messageQuery.forward)
            {
                int count = _messageQuery.endIndex - _messageQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().StartAt(_messageQuery.indexKey).LimitToFirst(count);
            }
            else
            {
                int count = _messageQuery.endIndex - _messageQuery.startIndex + 1;
                databaseQuery = Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).OrderByKey().EndAt(_messageQuery.indexKey).LimitToLast(count);
            }
            MessagesCallback _callback = new MessagesCallback();
            databaseQuery
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                    {
                        _callback.IsSuccess = true;
                        DataSnapshot snapshot = task.Result;
                        List<Message> feeds = new List<Message>();
                        for (int i = 0; i < snapshot.ChildrenCount; i++)
                        {
                            DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);

                            string jsonMessage = feedSnapshot.GetRawJsonValue();
                            Message _dataFeed = JsonUtility.FromJson<Message>(jsonMessage);
                            _dataFeed.Key = feedSnapshot.Key;
                            if (_dataFeed != null)
                            {
                                if (feedSnapshot.Key != _messageQuery.indexKey)
                                {
                                    feeds.Add(_dataFeed);
                                }
                            }

                        }
                        if (!_messageQuery.forward)
                        {
                            feeds.Reverse();
                        }
                        _callback.messages = feeds;
                        _callback.forward = _messageQuery.forward;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else
                    {
                        _callback.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _messageQuery.callback.Invoke(_callback));
                        CleanTask(task);
                    }
                });*/
        }

        public void GetCommentsDaEliminare(int tipo, Action<eliminacommenti.DeleteMessagesCallback> callback)
        {
            if (tipo == 0)
            {
                Query databaseQuery = Database.RootReference.Child("PostComments");
                eliminacommenti.DeleteMessagesCallback _callback = new eliminacommenti.DeleteMessagesCallback();
                databaseQuery.GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                    {
                        List<eliminacommenti.CommentoDaEl> feeds = new List<eliminacommenti.CommentoDaEl>();
                        for (int o = 0; o < task.Result.ChildrenCount; o++)
                        {
                            DataSnapshot snapshot = task.Result.Children.ElementAt(o).Child("List");
                            for (int i = 0; i < snapshot.ChildrenCount; i++)
                            {
                                DataSnapshot feedSnapshot = snapshot.Children.ElementAt(i);

                                string jsonMessage = feedSnapshot.GetRawJsonValue();
                                Message _dataFeed = JsonUtility.FromJson<Message>(jsonMessage);
                                _dataFeed.Key = feedSnapshot.Key;
                                if (_dataFeed != null)
                                {
                                    //GetUserFullName(_dataFeed.UserID, _name =>
                                    //{
                                    _dataFeed.FullName = "name";// _name;
                                    feeds.Add(new eliminacommenti.CommentoDaEl() { BodyTXT = _dataFeed.BodyTXT, FullName = _dataFeed.FullName, DateCreated = _dataFeed.DateCreated, UserID = _dataFeed.UserID, Key = _dataFeed.Key, PostId = _dataFeed.TargetId });
                                    print(feeds.Count);
                                    //});
                                }
                            }
                        }

                        static int Compara(string a, string b) => DateTime.Parse(a, System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) > DateTime.Parse(b, System.Globalization.CultureInfo.CreateSpecificCulture("en-US")) ? 1 : -1;
                        //feeds.Sort((a, b) => Compara(a.DateCreated, b.DateCreated));
                        _callback.messages = feeds;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(_callback));
                        CleanTask(task);
                    }
                });
            }
            else
            {
                eliminacommenti.DeleteMessagesCallback _callback = new eliminacommenti.DeleteMessagesCallback();
                Database.RootReference.Child("ReportedComments").GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                        UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(_callback));
                        CleanTask(task);
                    }
                    else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                    {
                        List<eliminacommenti.CommentoDaEl> feeds = new List<eliminacommenti.CommentoDaEl>();
                        List<string> id = new List<string>();
                        List<string> postId = new List<string>();
                        for (int o = 0; o < task.Result.ChildrenCount; o++)
                        {
                            id.Add(task.Result.Children.ElementAt(o).Key);
                            postId.Add(Convert.ToString(task.Result.Children.ElementAt(o).Value));
                        }
                        bool fatto = false;
                        for (int o = 0; o < id.Count; o++)
                        {
                            Database.RootReference.Child("PostComments").Child(postId[o]).Child("List").Child(id[o]).GetValueAsync().ContinueWith(task1 =>
                            {
                                if (task1.IsFaulted || task1.IsCanceled)
                                {
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(_callback));
                                    CleanTask(task);
                                }
                                else if (task1.IsCompleted && task1.Result != null)
                                {
                                    string jsonMessage = task1.Result.GetRawJsonValue();
                                    Message _dataFeed = JsonUtility.FromJson<Message>(jsonMessage);
                                    _dataFeed.Key = task1.Result.Key;
                                    if (_dataFeed != null)
                                    {
                                        _dataFeed.FullName = "name";
                                        feeds.Add(new eliminacommenti.CommentoDaEl() { BodyTXT = _dataFeed.BodyTXT, FullName = _dataFeed.FullName, DateCreated = _dataFeed.DateCreated, UserID = _dataFeed.UserID, Key = _dataFeed.Key, PostId = _dataFeed.TargetId });
                                    }
                                    if (feeds.Count == id.Count && !fatto)
                                    {
                                        fatto = true;
                                        _callback.messages = feeds;
                                        UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(_callback));
                                    }
                                    CleanTask(task1);
                                }
                            });
                        }
                        CleanTask(task);
                    }
                });
            }
        }

        public void GetAccountNamesDaEliminare(Action<List<string>, List<DatabaseReference>> callback)
        {
            Query databaseQuery = Database.RootReference.Child("Users");
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(new List<string>(), new List<DatabaseReference>()));
                    CleanTask(task);
                }
                else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                {
                    List<string> s = new List<string>();
                    List<DatabaseReference> r = new List<DatabaseReference>();
                    for (int o = 0; o < task.Result.ChildrenCount; o++)
                    {
                        DataSnapshot feedSnapshot = task.Result.Children.ElementAt(o).Child("FullName");
                        s.Add(Convert.ToString(feedSnapshot.Value));
                        r.Add(feedSnapshot.Reference);
                    }
                    UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(s, r));
                    CleanTask(task);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => callback.Invoke(new List<string>(), new List<DatabaseReference>()));
                    CleanTask(task);
                }
            });
        }

        public void GetInteractionsCount(Action<int> action)
        {
            Database.RootReference.Child(AppSettings.PostInteractionKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child("Count").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled || (task.IsCompleted && !task.Result.Exists))
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action.Invoke(0));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => action.Invoke(System.Convert.ToInt32(task.Result.Value)));
                    CleanTask(task);
                }
            });
        }

        public void SetInteractionsCount(int _count)
        {
            Database.RootReference.Child(AppSettings.PostInteractionKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child("Count").SetValueAsync(_count);
        }

        public void GetInteractionsAt(InteractionsQuery _interationQuery)
        {
            Query databaseQuery;
            if (string.IsNullOrEmpty(_interationQuery.indexKey))
                databaseQuery = Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_interationQuery.UserId).Child("List").OrderByKey().LimitToLast(5);
            else if (_interationQuery.forward)
                databaseQuery = Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_interationQuery.UserId).Child("List").OrderByKey().EndAt(_interationQuery.indexKey).LimitToLast(6);
            else
                databaseQuery = Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_interationQuery.UserId).Child("List").OrderByKey().StartAt(_interationQuery.indexKey).LimitToFirst(6);
            print("indexkey " + _interationQuery.indexKey);
            InteractionsCallback _callback = new InteractionsCallback();
            databaseQuery.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _callback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _interationQuery.callback.Invoke(_callback));
                    CleanTask(task);
                }
                else if (task.IsCompleted && task.Result != null && task.Result.ChildrenCount > 0)
                {
                    _callback.IsSuccess = true;
                    DataSnapshot snapshot = task.Result;
                    List<Interaction> feeds = new List<Interaction>();
                    bool uguale = false;
                    for (int i = 0; i < snapshot.ChildrenCount; i++)
                    {
                        DataSnapshot feedSnapsho = snapshot.Children.ElementAt(i);
                        string jsonInteraction = feedSnapsho.GetRawJsonValue();
                        Interaction _dataFeed = JsonUtility.FromJson<Interaction>(jsonInteraction);
                        _dataFeed.Key = feedSnapsho.Key;
                        if (_dataFeed != null)
                        {
                            void Fatto()
                            {
                                print(snapshot.ChildrenCount);
                                List<Interaction> f = new List<Interaction>();
                                static int Compara(string a, string b) => string.Compare(a, b, StringComparison.Ordinal);
                                feeds.Sort((a, b) => Compara(a.Key, b.Key));
                                _callback.forward = _interationQuery.forward;
                                //if (!_interationQuery.forward)
                                  //  feeds.Reverse();
                                _callback.interactions = feeds;
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _interationQuery.callback.Invoke(_callback));
                                CleanTask(task);
                            }

                            if (_dataFeed.Key != _interationQuery.indexKey)
                            {
                                GetUserFullName(_dataFeed.UserID, _name =>
                                {
                                    _dataFeed.UserID = _name;
                                    feeds.Add(_dataFeed);
                                    if (snapshot.ChildrenCount == feeds.Count || (snapshot.ChildrenCount == feeds.Count + 1 && uguale))
                                        Fatto();
                                });
                            }
                            else
                            {
                                uguale = true;
                                if (snapshot.ChildrenCount == 1)
                                {
                                    _callback.interactions = new List<Interaction>();
                                    _callback.forward = _interationQuery.forward;
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => _interationQuery.callback.Invoke(_callback));
                                    CleanTask(task);
                                }
                            }
                        }
                    }
                }
                else
                {
                    _callback.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => AppManager.VIEW_CONTROLLER.HideLoading());
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _interationQuery.callback.Invoke(_callback));
                    CleanTask(task);
                }
            });
        }

        public void GetFeedVideoFileUrl(string _fileName, Action<string> _callback)
        {
            StorageReference video_ref = Storage.RootReference.Child(AppSettings.FeedUploadVideoPath + _fileName + AppSettings.ConvertedVideoOutPath);
            video_ref.GetDownloadUrlAsync().ContinueWith((Task<Uri> task2) =>
            {
                if (!task2.IsFaulted && !task2.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(task2.Result.ToString()));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(string.Empty));
                    CleanTask(task2);
                }
            });
        }

        public void UploadAndCompressVideo(string _path, string _databasePath)
        {
            // Create the arguments to the callable function.
            var data = new Dictionary<string, object>();
            data["uploadPath"] = _path;
            data["bucketUrl"] = Storage.App.Options.StorageBucket;
            data["databasePath"] = _databasePath;

            // Call the function and extract the operation from the result.
            HttpsCallableReference _function = Functions.GetHttpsCallable("UploadAndCompressVideo");
            _function.CallAsync(data).ContinueWith((task) =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }

            });
        }

        public void AddWatermarkToVideo(Feed feed, Action<byte[]> _action)
        {
            /*if (feed.MeidaHeight > 1024)
            {
                feed.MediaWidth = feed.MeidaHeight / 1024;
                feed.MeidaHeight = 1024;
            }
            if (feed.MediaWidth > 1024)
            {
                feed.MediaWidth = feed.MediaWidth / 1024;
                feed.MeidaHeight = 1024;
            }*/
            List<int> sizes = new List<int>() { 25, 50, 100, 150, 200, 300, 400 };
            int size = 0;
            if (feed.MeidaHeight < 35)
                Adatta(0);
            else if (feed.MeidaHeight < 65)
                Adatta(1);
            else if (feed.MeidaHeight < 125)
                Adatta(2);
            else if (feed.MeidaHeight < 175)
                Adatta(3);
            else if (feed.MeidaHeight < 225)
                Adatta(4);
            else if (feed.MeidaHeight < 325)
                Adatta(5);
            else
                Adatta(6);

            void Adatta(int a)
            {
                size = sizes[a];
                if (feed.MediaWidth <= size * 4)
                {
                    if (a - 1 >= 0)
                        Adatta(a - 1);
                    else
                        size = sizes[a];
                }
            }
            // Create the arguments to the callable function.
            var data = new Dictionary<string, object>();
            data["uploadPath"] = "Feeds/Video/" + feed.VideoFileName + "_output.mp4";
            data["bucketUrl"] = Storage.App.Options.StorageBucket;
            data["watermarkPath"] = "watermark video/watermark " + size + ".png";
            print(size + "  " + feed.MediaWidth + "  " + feed.MeidaHeight);
            // Call the function and extract the operation from the result.
            Functions.GetHttpsCallable("AddWatermartToVideo").CallAsync(data).ContinueWith((task) =>
            {
                if (task.IsFaulted || task.IsCanceled && task.IsCompleted)
                {
                    print("fallito 0");
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(null));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    string url = (string)((Dictionary<object, object>)task.Result.Data)["url"];
                    print("risultato: " + task.Result.Data.ToString());
                    print("url: " + url);
                    Storage.RootReference.Child(url).GetBytesAsync(1024 * 1024 * 100).ContinueWith(task1 =>
                    {
                        if (task.IsFaulted || task1.IsCanceled)
                        {
                            Storage.RootReference.Child(JsonUtility.FromJson<Dictionary<string, string>>(task.Result.Data.ToString())["url"]).DeleteAsync();
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(null));
                            CleanTask(task1);
                        }
                        else if (task1.IsCompleted)
                        {
                            Storage.RootReference.Child(url).DeleteAsync();
                            UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(task1.Result));
                            CleanTask(task1);
                        }
                    });
                    CleanTask(task);
                }
            });
        }

        public void SharePostWithFriends(string _userId, string _postId)
        {
            Debug.Log("SharePostWithFriends " + _userId + " " + _postId);
            // Create the arguments to the callable function.
            var data = new Dictionary<string, object>();
            data["_userId"] = _userId;
            data["_postId"] = _postId;

            // Call the function and extract the operation from the result.
            HttpsCallableReference _function = Functions.GetHttpsCallable("SharePostWithFriends");
            _function.CallAsync(data).ContinueWith((task) =>
            {
                if (!task.IsFaulted && !task.IsCanceled && task.IsCompleted)
                {
                    Debug.Log("Success SharePostWithFriends");
                    CleanTask(task);
                }
                else
                {
                    Debug.Log("Failed SharePostWithFriends " + task.Exception.StackTrace);
                    CleanTask(task);
                }
            });
        }

        public DatabaseReference GetMessageReferece(string _userId)
        {
            string messageKey = GetUserMessageKey(AppManager.USER_PROFILE.FIREBASE_USER.UserId, _userId);
            return Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey);
        }

        public DatabaseReference GetGroupChatReferece(string _chatId)
        {
            string messageKey = _chatId;
            return Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey);
        }

        public DatabaseReference GetTypingMessageReferece(string _userId)
        {
            string messageKey = GetUserMessageKey(AppManager.USER_PROFILE.FIREBASE_USER.UserId, _userId);
            return Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.TypingMSGKey).Child(_userId);
        }

        public void UpdateTypingMessage(string _userId, string _s)
        {
            string messageKey = GetUserMessageKey(AppManager.USER_PROFILE.FIREBASE_USER.UserId, _userId);
            Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.TypingMSGKey).Child((AppManager.USER_PROFILE.FIREBASE_USER.UserId)).SetValueAsync(_s);
        }

        public DatabaseReference GetPostCommentsReferece(string _postId)
        {
            return Database.RootReference.Child(AppSettings.PostCommentsKey).Child(_postId).Child(AppSettings.ContainerListKey);
        }

        public DatabaseReference GetMessageListReferece()
        {
            return Database.RootReference.Child(AppSettings.UserMessagesList).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
        }

        public DatabaseReference GetUnreadMessageWithUserReferense(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UnreadMessagesKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userID);
        }

        public void CheckAndAddNewChatInfo(MessageGroupInfo _info, Action<ChatInfoMessage> _callback = null)
        {
            Task<DataSnapshot> fTask = Database.RootReference.Child(AppSettings.UserMessagesGroups).Child(_info.ChatID).GetValueAsync();
            ChatInfoMessage _callbackInfo = new ChatInfoMessage();

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    if (fTask.Result == null || fTask.Result.Value == null)
                    {
                        AddOrUpdateChatInfo(_info, _callback);
                    }
                    else
                    {
                        _callbackInfo.IsSuccess = false;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback?.Invoke(_callbackInfo));
                    }
                    CleanTask(task2);
                }
                else
                {
                    _callbackInfo.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback?.Invoke(_callbackInfo));
                    CleanTask(task2);
                }
            });
        }

        public void AddOrUpdateChatInfo(MessageGroupInfo _info, Action<ChatInfoMessage> _callback = null)
        {
            string json = JsonUtility.ToJson(_info);
            ChatInfoMessage _callbackInfo = new ChatInfoMessage();

            Database.RootReference.Child(AppSettings.UserMessagesGroups).Child(_info.ChatID).SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    _callbackInfo.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback?.Invoke(_callbackInfo));
                    CleanTask(task);
                    // Uh-oh, an error occurred!
                }
                else
                {
                    _callbackInfo.IsSuccess = true;
                    _callbackInfo.Info = _info;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback?.Invoke(_callbackInfo));
                    CleanTask(task);
                }
            });
        }

        public void GetGroupChatInfo(string _chatId, Action<ChatInfoMessage> _callback)
        {
            ChatInfoMessage _callbackInfo = new ChatInfoMessage();

            DatabaseReference _tokenRef = Database.RootReference.Child(AppSettings.UserMessagesGroups).Child(_chatId);
            _tokenRef.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _callbackInfo.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_callbackInfo));
                }
                else if (task.IsCompleted && task.Result.Exists)
                {
                    _callbackInfo.IsSuccess = true;

                    DataSnapshot snapshot = task.Result;
                    string _json = snapshot.GetRawJsonValue();
                    MessageGroupInfo _info = JsonUtility.FromJson<MessageGroupInfo>(_json);
                    _callbackInfo.Info = _info;

                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_callbackInfo));
                }
                else
                {
                    _callbackInfo.IsSuccess = false;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(_callbackInfo));
                }
            });
        }

        public void UploadMessage(Message _msg, MessageGroupInfo _info)
        {
            string messageKey = _info.ChatID;
            string json = JsonUtility.ToJson(_msg);
            Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).Push().SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    CleanTask(task);
                    // Uh-oh, an error occurred!
                }
                else
                {
                    UpdateMessagesList(_info);
                    CleanTask(task);
                }
            });
        }

        public void UploadGroupMessage(Message _msg, MessageGroupInfo _info)
        {
            string messageKey = _info.ChatID;
            string json = JsonUtility.ToJson(_msg);
            Database.RootReference.Child(AppSettings.UserMessagesKey).Child(messageKey).Child(AppSettings.ContainerListKey).Push().SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    CleanTask(task);
                    // Uh-oh, an error occurred!
                }
                else
                {
                    // to do : update group list
                    UpdateMessagesList(_info);
                    CleanTask(task);
                }
            });
        }

        public void UploadComments(Message _msg, string _postId)
        {
            string messageKey = _postId;
            string json = JsonUtility.ToJson(_msg);
            Database.RootReference.Child(AppSettings.PostCommentsKey).Child(messageKey).Child(AppSettings.ContainerListKey).Push().SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    CleanTask(task);
                    // Uh-oh, an error occurred!
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void RemoveFromMessageList(MessageGroupInfo _info)
        {
            Database.RootReference.Child(AppSettings.UserMessagesList).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).OrderByValue().EqualTo(_info.ChatID).GetValueAsync().ContinueWithOnMainThread(task1 =>
            {
                if (task1.IsFaulted || task1.IsCanceled)
                {
                    CleanTask(task1);
                }
                else
                {
                    List<Task> TaskList = new List<Task>();
                    if (task1.Result != null)
                    {
                        foreach (DataSnapshot _data in task1.Result.Children)
                        {
                            Debug.Log(_data.Key);
                            Task _task = Database.RootReference.Child(AppSettings.UserMessagesList).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(_data.Key).RemoveValueAsync();
                            TaskList.Add(_task);
                        }
                    }
                    if (TaskList.Count > 0)
                    {
                        Task.WhenAll(TaskList).ContinueWithOnMainThread(task2 =>
                        {
                            if (task2.IsCompleted)
                            {

                            }
                        });
                    }

                    CleanTask(task1);
                }
            });
        }

        public void UpdateMessagesList(MessageGroupInfo _info)
        {
            //string _userId1 = _info.Users[0];
            //string _userId2 = _info.Users[1];

            for (int i = 0; i < _info.Users.Count; i++)
            {
                string _userID = _info.Users[i];

                FirebaseDatabase.DefaultInstance.RootReference.Child(AppSettings.UserMessagesList).Child(_userID).OrderByValue().EqualTo(_info.ChatID).GetValueAsync().ContinueWithOnMainThread(task1 =>
                {
                    if (task1.IsFaulted || task1.IsCanceled)
                    {
                        CleanTask(task1);
                    }
                    else
                    {
                        List<Task> TaskList = new List<Task>();
                        if (task1.Result != null)
                        {
                            foreach (DataSnapshot _data in task1.Result.Children)
                            {
                                Task _task = Database.RootReference.Child(AppSettings.UserMessagesList).Child(_userID).Child(_data.Key).RemoveValueAsync();
                                TaskList.Add(_task);
                            }
                        }
                        if (TaskList.Count > 0)
                        {
                            Task.WhenAll(TaskList).ContinueWithOnMainThread(task2 =>
                            {
                                if (task2.IsCompleted)
                                {
                                    Database.RootReference.Child(AppSettings.UserMessagesList).Child(_userID).Push().SetValueAsync(_info.ChatID);
                                }
                            });
                        }
                        else
                        {
#if !UNITY_EDITOR
                            Debug.Log("push new message to list without removing");
                            Database.RootReference.Child(AppSettings.UserMessagesList).Child(_userID).Push().SetValueAsync((_info.ChatID));
#endif
                        }

                        CleanTask(task1);
                    }
                });
            }
        }

        public void AddToFriends(string _userId, Action _callback)
        {
            DatabaseReference _requestRef = Database.RootReference.Child(AppSettings.UserRequestedFriendsKey).Child(_userId).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DatabaseReference _pendingRef = Database.RootReference.Child(AppSettings.UserPendingFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userId);
            _requestRef.SetValueAsync(0).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    CleanTask(task);
                }
                else
                {
                    _pendingRef.SetValueAsync(0).ContinueWith(task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            CleanTask(task2);
                        }
                        else
                        {
                            if (_callback != null)
                            {
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke());
                            }
                            AppManager.USER_PROFILE.GetUserFullName(_name =>
                            {
                                NotificationMessage _msg = new NotificationMessage();
                                _msg.UserId = _userId;
                                _msg.Title = _name;
                                _msg.Body = "Wants to be your friend";
                                SendPushNotification(_msg);
                            });

                            CleanTask(task2);
                        }
                    });
                    CleanTask(task);
                }
            });
        }

        public void RemoveFromFriend(string _userId, Action _callback)
        {
            DatabaseReference _requestRef = Database.RootReference.Child(AppSettings.UserFriendsKey).Child(_userId).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DatabaseReference _pendingRef = Database.RootReference.Child(AppSettings.UserFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userId);
            _requestRef.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    CleanTask(task);
                }
                else
                {
                    _pendingRef.RemoveValueAsync().ContinueWith(task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            CleanTask(task2);
                        }
                        else
                        {
                            if (_callback != null)
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke());
                            CleanTask(task2);
                        }
                    });
                    CleanTask(task);
                }
            });
        }

        public void CancelPendingFromFriend(string _userId, Action _callback)
        {
            DatabaseReference _requestRef = Database.RootReference.Child(AppSettings.UserRequestedFriendsKey).Child(_userId).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DatabaseReference _pendingRef = Database.RootReference.Child(AppSettings.UserPendingFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userId);
            _requestRef.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    CleanTask(task);
                }
                else
                {
                    _pendingRef.RemoveValueAsync().ContinueWith(task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            CleanTask(task2);
                        }
                        else
                        {
                            if (_callback != null)
                                UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke());
                            CleanTask(task2);
                        }
                    });
                    CleanTask(task);
                }
            });
        }

        public void AcceptFriend(string _userId, Action _callback)
        {
            DatabaseReference _requestRef = Database.RootReference.Child(AppSettings.UserPendingFriendsKey).Child(_userId).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DatabaseReference _pendingRef = Database.RootReference.Child(AppSettings.UserRequestedFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userId);
            DatabaseReference _friendRef1 = Database.RootReference.Child(AppSettings.UserFriendsKey).Child(_userId).Child(AppSettings.ContainerListKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            DatabaseReference _friendRef2 = Database.RootReference.Child(AppSettings.UserFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userId);
            Task _task = _requestRef.RemoveValueAsync();
            Task _task2 = _pendingRef.RemoveValueAsync();
            Task _task3 = _friendRef1.SetValueAsync(0);
            Task _task4 = _friendRef2.SetValueAsync(0);
            List<Task> taskList = new List<Task>();
            taskList.Add(_task);
            taskList.Add(_task2);
            taskList.Add(_task3);
            taskList.Add(_task4);
            Task.WhenAll(taskList).ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke());
                    CleanTask(task2);
                    CleanTask(_task);
                    CleanTask(_task2);
                    CleanTask(_task3);
                    CleanTask(_task4);
                }
            });
        }

        public DatabaseReference GetFriendReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UserFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userID);
        }

        public DatabaseReference GetRequestFriendReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UserRequestedFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userID);
        }

        public DatabaseReference GetPendingFriendReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UserPendingFriendsKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userID);
        }

        public DatabaseReference GetFriendCountReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UserFriendsKey).Child(_userID).Child(AppSettings.ListCountKey);
        }

        public DatabaseReference GetRequestFriendCountReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UserRequestedFriendsKey).Child(_userID).Child(AppSettings.ListCountKey);
        }

        public DatabaseReference GetPendingFriendCountReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UserPendingFriendsKey).Child(_userID).Child(AppSettings.ListCountKey);
        }

        public DatabaseReference GetFriendFeedCountReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.FriendsPostsKey).Child(_userID).Child(AppSettings.ListCountKey);
        }

        public DatabaseReference GetAllUnreadCountReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UnreadMessagesKey).Child(_userID).Child(AppSettings.ListCountKey);
        }

        public DatabaseReference GetUnreadCountWithUserReferece(string _userID)
        {
            return Database.RootReference.Child(AppSettings.UnreadMessagesKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_userID);
        }

        public DatabaseReference GetUnreadCountInGroupReferece(string _groupID)
        {
            return Database.RootReference.Child(AppSettings.UnreadMessagesKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ContainerListKey).Child(_groupID);
        }

        public DatabaseReference GetPostLikesCountReferense(string _postID)
        {
            return Database.RootReference.Child(AppSettings.PostLikesKey).Child(_postID).Child(AppSettings.ListCountKey);
        }

        public DatabaseReference GetPostDislikesCountReferense(string _postID)
        {
            return Database.RootReference.Child(AppSettings.PostDislikesKey).Child(_postID).Child(AppSettings.ListCountKey);
        }

        public void ClearUnreadMessagesWithUser(string _userID)
        {
            GetUnreadCountWithUserReferece(_userID).RemoveValueAsync();
        }

        public void ClearUnreadMessagesGroup(string _groupID)
        {
            GetUnreadCountInGroupReferece(_groupID).RemoveValueAsync();
        }

        public void ClearUnreadFriendsFeed()
        {
            GetFriendFeedCountReferece(AppManager.USER_PROFILE.FIREBASE_USER.UserId).RemoveValueAsync();
        }

        public void CanAddToFriend(string _userID, Action<bool> _callback)
        {
            Task<DataSnapshot> fTask = GetFriendReferece(_userID).GetValueAsync();
            Task<DataSnapshot> rTask = GetRequestFriendReferece(_userID).GetValueAsync();
            Task<DataSnapshot> pTask = GetPendingFriendReferece(_userID).GetValueAsync();
            List<Task> TaskList = new List<Task>();
            TaskList.Add(fTask);
            TaskList.Add(rTask);
            TaskList.Add(pTask);
            Task.WhenAll(TaskList).ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    if ((fTask.Result == null && rTask.Result == null && pTask.Result == null) ||
                    (fTask.Result.Value == null && rTask.Result.Value == null && pTask.Result.Value == null))
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));

                    }
                    else
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    }
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void IsInFriendsList(string _userID, Action<bool> _callback)
        {
            Task<DataSnapshot> fTask = GetFriendReferece(_userID).GetValueAsync();

            fTask.ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    if (fTask.Result == null || fTask.Result.Value == null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    }
                    else
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    }
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void SendActivity()
        {
            Database.RootReference.Child(AppSettings.RootUserKey).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.ActivityKey).SetValueAsync(ServerValue.Timestamp);
        }

        public void GetActivity(string _userID, Action<string> _callback)
        {
            Database.RootReference.Child(AppSettings.RootUserKey).Child(_userID).Child(AppSettings.ActivityKey).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result != null && task.Result.Value != null && !string.IsNullOrEmpty(task.Result.Value.ToString()))
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(task.Result.Value.ToString()));
                    }
                    CleanTask(task);
                }
                else
                {
                    CleanTask(task);
                }
            });
        }

        public void RemovePost(string _PostID, string _UserID, Action _callback)
        {
            Database.RootReference.Child(AppSettings.AllPostsKey).Child(_PostID).GetValueAsync().ContinueWith(task2 =>
            {
                if (task2.IsCompleted && task2.Result.Exists)
                {
                    string _userId = Convert.ToString(task2.Result.Child("OwnerID").Value);
                    print(30);
                    string s = "";
                    if (Convert.ToInt32(task2.Result.Child("Type").Value) == 1)
                    {
                        s = Convert.ToString(task2.Result.Child("ImageURL").Value).Split(new string[] { "Image%2F" }, StringSplitOptions.None)[1].Split(new string[] { "?" }, StringSplitOptions.None)[0];
                        print(31);
                        Storage.RootReference.Child("Feeds").Child("Image").Child(s).DeleteAsync();
                    }
                    else if (Convert.ToInt32(task2.Result.Child("Type").Value) == 2)
                    {
                        s = Convert.ToString(task2.Result.Child("VideoURL").Value).Split(new string[] { "Video%2F" }, StringSplitOptions.None)[1].Split(new string[] { "?" }, StringSplitOptions.None)[0];
                        print(32);
                        Storage.RootReference.Child("Feeds").Child("Video").Child(s).DeleteAsync();
                        Storage.RootReference.Child("Feeds").Child("Image").Child(Convert.ToString(task2.Result.Child("ImageURL").Value).Split(new string[] { "Image%2F" }, StringSplitOptions.None)[1].Split(new string[] { "?" }, StringSplitOptions.None)[0]).DeleteAsync();
                    }
                    print(34);
                    Database.RootReference.Child(AppSettings.AllPostsKey).Child(_PostID).RemoveValueAsync();
                    print(35);
                    Database.RootReference.Child(AppSettings.PremiumPosts).Child(_PostID).RemoveValueAsync();
                    print(22);
                    Database.RootReference.Child(AppSettings.ReportedPosts).Child(_PostID).RemoveValueAsync();
                    print(23);
                    Database.RootReference.Child(AppSettings.PostCommentsKey).Child(_PostID).Child("List").GetValueAsync().ContinueWith(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                            CleanTask(task);
                        else if (task.IsCompleted)
                        {
                            if (task.Result.Exists)
                                for (int i = 0; i < task.Result.ChildrenCount; i++)
                                    Database.RootReference.Child("ReportedComments").Child(task.Result.Children.ElementAt(i).Key).RemoveValueAsync();
                            Database.RootReference.Child(AppSettings.PostCommentsKey).Child(_PostID).RemoveValueAsync();
                        }
                    });
                    print(24);
                    Database.RootReference.Child(AppSettings.PostLikesKey).Child(_userId).Child("List").Child(_PostID).RemoveValueAsync();
                    print(25);
                    Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_PostID).RemoveValueAsync();
                    print(26);
                    Database.RootReference.Child(AppSettings.UserPostsKey).Child(_UserID).Child(_PostID).RemoveValueAsync();
                    print(27);
                }
            });
        }

        public void DeleteUserDada(string _userID, bool _eliminaAccount = false)
        {
            print(_userID);
            Database.RootReference.Child(AppSettings.RootUserKey).Child(_userID).RemoveValueAsync();
            print(0);
            Database.RootReference.Child(AppSettings.PostInteractionKey).Child(_userID).RemoveValueAsync();
            print(1);
            Database.RootReference.Child(AppSettings.DeviceTokensKey).Child(_userID).RemoveValueAsync();
            print(2);
            Database.RootReference.Child(AppSettings.UserPostsKey).Child(_userID).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    print(20);
                    for (int i = 0; i < task.Result.ChildrenCount; i++)
                    {
                        print(21 + " " + i);
                        string key = (string)task.Result.Children.ElementAt(i).Key;
                        print(key);
                        RemovePost(key, _userID, () => { });
                    }
                    Database.RootReference.Child(AppSettings.UserPostsKey).Child(_userID).RemoveValueAsync();
                }
                else if (task.IsCanceled)
                    print("canc");
                else if (task.IsFaulted)
                    print("fall");
            });

            Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_userID).Child(AppSettings.UserAvatarKey).Child("Image_Origin.jpg").DeleteAsync();
            print(3);
            Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_userID).Child(AppSettings.UserAvatarKey).Child("Image_Size_1024.jpg").DeleteAsync();
            print(4);
            Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_userID).Child(AppSettings.UserAvatarKey).Child("Image_Size_512.jpg").DeleteAsync();
            print(5);
            Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_userID).Child(AppSettings.UserAvatarKey).Child("Image_Size_256.jpg").DeleteAsync();
            print(6);
            Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_userID).Child(AppSettings.UserAvatarKey).Child("Image_Size_128.jpg").DeleteAsync();
            print(7);
            Database.RootReference.Child(AppSettings.SubscriptionsVaultPLUSKey).Child(_userID).RemoveValueAsync();
            print(8);
            Storage.RootReference.Child(AppSettings.RootUserStorageKey).Child(_userID).Child(AppSettings.UserCustomBackgroundKey).DeleteAsync();
            print(9);
            //Database.RootReference.Child(AppSettings.RootUserKey).Child(_user.UserID).SetRawJsonValueAsync
            if (_eliminaAccount)
            {
                FirebaseAuth.DefaultInstance.CurrentUser.DeleteAsync();
                AppManager.USER_SETTINGS.Logout(false);
            }
            else
            {
                Database.RootReference.Child(AppSettings.RootUserDeletedKey).Child(_userID).SetValueAsync(0).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("creato");
                    }
                    if (task.IsFaulted)
                    {
                        Debug.Log("error");
                    }
                });
                print(10);
            }
        }

        public void PostInMainFeed(string _postID)
        {
            Database.RootReference.Child(AppSettings.PremiumPosts).Child(_postID).SetValueAsync(0).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("funziona");
                }
                if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
            });
        }

        public void BlockUser(string _userID)
        {
            Database.RootReference.Child(AppSettings.RootUserKey).Child(Auth.CurrentUser.UserId).Child("blockedUser").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    int num = (int)task.Result.ChildrenCount;
                    bool a = true;
                    for (int i = 0; i < num; i++)
                    {
                        if ((string)task.Result.Children.ElementAt(i).Value == _userID)
                        {
                            a = false;
                            break;
                        }
                    }
                    if (a)
                        Database.RootReference.Child(AppSettings.RootUserKey).Child(Auth.CurrentUser.UserId).Child("blockedUser").Child(num.ToString()).SetValueAsync(_userID).ContinueWith(task1 => { if (task1.IsCompleted) Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success); });
                }
                if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
            });
        }

        public void ReportPost(string _postID)
        {
            Database.RootReference.Child(AppSettings.ReportedPosts).Child(_postID).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    int num = (int)task.Result.ChildrenCount;
                    bool a = true;
                    for (int i = 0; i < num; i++)
                    {
                        if ((string)task.Result.Children.ElementAt(i).Value == Auth.CurrentUser.UserId)
                        {
                            print((string)task.Result.Children.ElementAt(i).Value);
                            a = false;
                            break;
                        }
                    }
                    if (a)
                        Database.RootReference.Child(AppSettings.ReportedPosts).Child(_postID).Child(num.ToString()).SetValueAsync(Auth.CurrentUser.UserId).ContinueWith(task1 => { if (task1.IsCompleted) Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success); });
                }
                if (task.IsFaulted)
                    Debug.Log("error");
            });
        }

        public void ReportComments(string _id, string _postId, Action<bool> _action)
        {
            Database.RootReference.Child("ReportedComments").Child(_id).SetValueAsync(_postId).ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(false));
                    CleanTask(task);
                }
                else if (task.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _action.Invoke(true));
                    CleanTask(task);
                }
            });
        }

        public void AddModerator(string s, Action<bool> _callback)
        {
            Database.RootReference.Child("Moderators").Push().SetValueAsync(s).ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(true));
                    CleanTask(task2);
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _callback.Invoke(false));
                    CleanTask(task2);
                }
            });
        }

        public void GetModerators()
        {
            Database.RootReference.Child("Moderators").GetValueAsync().ContinueWith(task2 =>
            {
                if (task2.IsCompleted)
                {
                    for (int i = 0; i < task2.Result.ChildrenCount; i++)
                        AppManager.moderatorsList.Add(Convert.ToString(task2.Result.Children.ElementAt(i).Value));
                    AppManager.IsAdmin();
                    CleanTask(task2);
                }
                else
                    CleanTask(task2);
            });
        }

        /// <summary>
        /// Update user last activity value
        /// </summary>
        /// <param name="_callback">Add completion method</param>
        public void UpdateUserActivity(Action<CallbackSetUserActivity> _callback = null)
        {
            CallbackSetUserActivity _response = new CallbackSetUserActivity();
            DatabaseReference databaseReferense = Database.RootReference.Child(AppSettings.RootUserKey)
                .Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.UserActivityKey);
            databaseReferense.SetValueAsync(ServerValue.Timestamp).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    _response.IsSuccess = true;
                    CleanTask(task);
                }
                else
                {
                    _response.ErrorMessage = task.Exception.Message;
                    CleanTask(task);
                }
                if (_callback != null)
                {
                    _callback.Invoke(_response);
                }
            });
        }

        /// <summary>
        /// Get current server time. Availbale when user is logined
        /// </summary>
        /// <param name="_callback">Add completion method</param>
        public void GetServerTimestamp(Action<CallbackGetServerTimestamp> _callback)
        {
            CallbackGetServerTimestamp _response = new CallbackGetServerTimestamp();
            UpdateUserActivity(_msg =>
            {
                if (_msg.IsSuccess)
                {
                    DatabaseReference databaseReferense = Database.RootReference.Child(AppSettings.RootUserKey)
                    .Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId).Child(AppSettings.UserActivityKey);
                    databaseReferense.GetValueAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            _response.IsSuccess = true;
                            _response.Data = task.Result.Value.ToString();
                            CleanTask(task);
                        }
                        else
                        {
                            _response.ErrorMessage = task.Exception.Message;
                            CleanTask(task);
                        }
                        if (_callback != null)
                        {
                            _callback.Invoke(_response);
                        }
                    });
                }
                else
                {
                    if (_callback != null)
                    {
                        _callback.Invoke(_response);
                    }
                }
            });
        }

        /// <summary>
        /// Get user last activity by user id.
        /// </summary>
        /// <param name="_userID">User ID</param>
        /// <param name="_callback">Add completion method</param>
        public void GetUserLastActivity(string _userID, Action<CallbackGetUserActivity> _callback)
        {
            CallbackGetUserActivity _response = new CallbackGetUserActivity();
            DatabaseReference databaseReferense = Database.RootReference.Child(AppSettings.RootUserKey)
                .Child(_userID).Child(AppSettings.UserActivityKey);
            databaseReferense.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    _response.IsSuccess = true;
                    _response.Data = task.Result.Value.ToString();
                    CleanTask(task);
                }
                else
                {
                    _response.ErrorMessage = task.Exception.Message;
                    CleanTask(task);
                }
                if (_callback != null)
                {
                    _callback.Invoke(_response);
                }
            });
        }

        public void MakeCallOffer(CallObject _call, Action<CallbackMakeCallMessage> _callback)
        {
            CallbackMakeCallMessage _response = new CallbackMakeCallMessage();

            GetServerTimestamp(_msg =>
            {
                if (_msg.IsSuccess)
                {
                    string _timeStamp = _msg.Data;
                    _call.CreateTimeStamp = _timeStamp;
                    //string _callKey = System.Guid.NewGuid().ToString();
                    DatabaseReference _callref = Database.RootReference.Child(AppSettings.UserCallList).Child(_call.TargetID).Push();
                    _call.DataKey = _callref.Key;
                    string json = JsonUtility.ToJson(_call);

                    _callref.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            CleanTask(task);
                            _callback.Invoke(_response);
                            // Uh-oh, an error occurred!
                        }
                        else
                        {
                            _response.IsSuccess = true;
                            _callback.Invoke(_response);
                            CleanTask(task);
                        }
                    });
                }
                else
                {
                    _callback.Invoke(_response);
                }
            });
        }

        public void CancelCallOffer(CallObject _call, Action _onComplete = null)
        {
            GetCallActiveStateReference(_call).SetValueAsync(false).ContinueWithOnMainThread(task =>
            {
                if (_onComplete != null)
                {
                    _onComplete.Invoke();
                }
            });
        }

        public void AnswerCallOffer(CallObject _call, Action _onComplete = null)
        {
            GetCallAnswerStateReference(_call).SetValueAsync(true).ContinueWithOnMainThread(task =>
            {
                if (_onComplete != null)
                {
                    _onComplete.Invoke();
                }
            });
        }

        public void SetCallBisy(CallObject _call, Action _onComplete = null)
        {
            GetCallBisyStateReference(_call).SetValueAsync(true).ContinueWithOnMainThread(task =>
            {
                if (_onComplete != null)
                {
                    _onComplete.Invoke();
                }
            });
        }

        public DatabaseReference GetCallReference()
        {
            return Database.RootReference.Child(AppSettings.UserCallList).Child(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
        }

        public DatabaseReference GetCallActiveStateReference(CallObject _call)
        {
            return Database.RootReference.Child(AppSettings.UserCallList).Child(_call.TargetID).Child(_call.DataKey).Child(AppSettings.CallActiveKey);
        }

        public DatabaseReference GetCallBisyStateReference(CallObject _call)
        {
            return Database.RootReference.Child(AppSettings.UserCallList).Child(_call.TargetID).Child(_call.DataKey).Child(AppSettings.CallBisyKey);
        }

        public DatabaseReference GetCallAnswerStateReference(CallObject _call)
        {
            return Database.RootReference.Child(AppSettings.UserCallList).Child(_call.TargetID).Child(_call.DataKey).Child(AppSettings.CallHasAnswerKey);
        }

        public string GetUserMessageKey(string _id1, string _id2)
        {
            List<string> sList = new List<string>();
            sList.Add(_id1);
            sList.Add(_id2);
            sList.Sort();
            return sList[0] + "-" + sList[1];
        }

        public bool IsFirebaseInited()
        {
            return FirebaseIsInited;
        }

        private void CleanTask(Task _task)
        {
            _task.Dispose();
            _task = null;
        }
    }

    public class SubscriptionVaultPLUS
    {
        public string StartDate;
        public int Duration;
    }

    public class RegistrationMessage
    {
        public bool IsComplete;
        public string ErrorMessage;
        public string UserID;
    }

    public class SetUserDataMessage
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public string UserID;
    }

    public class LoginMessage
    {
        public bool IsSuccess;
        public bool isGuest;
        public string ErrorMessage;
        public string UserID;
        public FirebaseUser FUser;
    }

    public class NotificationMessage
    {
        public string UserToken;
        public string Title;
        public string Body;
        public string UserId;
    }

    public class ChatInfoMessage
    {
        public bool IsSuccess;
        public MessageGroupInfo Info;
    }

    [System.Serializable]
    public class MessageGroupInfo
    {
        public string ChatID;
        public string ChatName;
        public List<string> Users;
        public MessageType Type;
    }

    /// <summary>
    /// Get user activity callback class
    /// </summary>
    public class CallbackGetUserActivity
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public string Data;
    }

    /// <summary>
    /// Get server time stamp callback class
    /// </summary>
    public class CallbackGetServerTimestamp
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public string Data;
    }

    /// <summary>
    /// Set user activity callback class
    /// </summary>
    public class CallbackSetUserActivity
    {
        public bool IsSuccess;
        public string ErrorMessage;
    }

    public class CallbackMakeCallMessage
    {
        public bool IsSuccess;
    }
}
public enum InteractionType { Like, Comment, Dislike }