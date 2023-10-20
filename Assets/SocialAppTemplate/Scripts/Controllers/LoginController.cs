using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocialApp;
using System;
using Firebase.Extensions;

namespace SocialApp
{
    public class LoginController : MonoBehaviour
    {
        [SerializeField]
        private InputField emailForgotInput;
        public List<Toggle> toggle;
        public InputField EmailInput = default;
        public InputField PasswordInput = default;
        [SerializeField]
        private Button loginBt, forgotBt;

        public event Action OnLoginEvent;
        public event Action OnLogoutEvent;

        private void OnEnable()
        {
            ClearFields();
        }

        public void SendLogIn()
        {
            if (CheckError())
            {
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Warning);
                return;
            }
            string _login = EmailInput.text.Trim();
            string _password = PasswordInput.text.Trim();
            OnLogin(_login, _password);
        }

        public void EnterAsGuest()
        {
            AppManager.VIEW_CONTROLLER.ShowLoading();
            AppManager.FIREBASE_CONTROLLER.LogInAsGuest(OnLoginSuccess);
        }

        public void AutoLogin(string _mail, string _password)
        {
            OnLogin(_mail, _password);
        }

        private void OnLogin(string _mail, string _password)
        {
            AppManager.VIEW_CONTROLLER.ShowLoading();
            AppManager.FIREBASE_CONTROLLER.LogIn(_mail, _password, OnLoginSuccess);
        }

        public void OnRegistration()
        {
            AppManager.VIEW_CONTROLLER.HideLogin();
            AppManager.VIEW_CONTROLLER.ShowRegistration();
        }

        public void OnLoginSuccess(LoginMessage _msg)
        {
            if (_msg.IsSuccess)
            {
                AppManager.FIREBASE_CONTROLLER.CheckUserDeleted(_msg.FUser.UserId, elim =>
                {
                    if (!elim)
                    {
                        if (!string.IsNullOrEmpty(EmailInput.text))
                        {
                            PlayerPrefs.SetString(AppSettings.LoginSaveKey, EmailInput.text.Trim());
                            PlayerPrefs.SetString(AppSettings.PasswordSaveKey, PasswordInput.text.Trim());
                            PlayerPrefs.Save();
                        }
                        AppSettings.guest = _msg.isGuest;
                        //print(_msg.FUser.UserId);
                        AppManager.USER_PROFILE.FIREBASE_USER = _msg.FUser;
                        AppManager.VIEW_CONTROLLER.HideLogin();
                        AppManager.VIEW_CONTROLLER.HideLoading();
                        AppManager.NAVIGATION.ShowUserProfile();
                        AppManager.VIEW_CONTROLLER.ShowNavigationPanel();
                        AppManager.FIREBASE_CONTROLLER.InitPushNotificationEvents();
                        if (!AppSettings.guest)
                            AppManager.FIREBASE_CONTROLLER.GetSubscriptionVaultPLUS();
                        if (!AppSettings.mostraCondividi)
                        {
                            int n = PlayerPrefs.GetInt("accessi");
                            if (n == 3 || n == 20 || n == 50 || n == 70 || n == 100)
                                AppSettings.mostraCondividi = true;
                        }
                        if (!PlayerPrefs.HasKey("tutorial") && !_msg.isGuest)
                            AppSettings.TutorialMode = FasiTutorial.ApriPublicaPost;
                        AppManager.TUTORIAL_CONTROLLER.AzioneCompletata();
                        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
                        OnLoginEvent?.Invoke();
                    }
                    else
                    {
                        AppManager.VIEW_CONTROLLER.HideLoading();
                        AppManager.VIEW_CONTROLLER.ShowPrima();
                        AppManager.VIEW_CONTROLLER.primaPaginaObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                        AppManager.LOGIN_CONTROLLER.OnSignOut();
                        AppSettings.TutorialMode = FasiTutorial.Nulla;
                        AppSettings.VaultPLUS = false;
                        AppManager.USER_PROFILE.ClearUser();
                        AppManager.USER_SETTINGS.Logout();
                        AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Fatal Error", Message = "Your account was deleted by the admins." });
                        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                    }
                });
            }
            else
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = _msg.ErrorMessage;
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
                AppManager.VIEW_CONTROLLER.HideLoading();
                AppManager.VIEW_CONTROLLER.ShowErrorScreen();
                print("errore nel login " + _msg.ErrorMessage);
                
            }
        }

        private void ClearFields()
        {
            EmailInput.text = string.Empty;
            PasswordInput.text = string.Empty;
        }

        private bool CheckError()
        {
            bool IsError = false;
            if (string.IsNullOrEmpty(EmailInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyEmail);
                IsError = true;
            }
            else if (string.IsNullOrEmpty(PasswordInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyPassword);
                IsError = true;
            }
            else if (!EmailInput.text.Contains(AppManager.APP_SETTINGS.EmailValidationCharacter))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmailNotValid);
                IsError = true;
            }
            else if (PasswordInput.text.Length < AppManager.APP_SETTINGS.MinAllowPasswordCharacters)
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.SmallPassword);
                IsError = true;
            }
            return IsError;
        }

        public void OnSignOut()
        {
            OnLogoutEvent?.Invoke();
        }

        public void Check()
        {
            for (int i = 0; i < toggle.Count; i++)
                if (!toggle[i].isOn)
                {
                    loginBt.interactable = false;
                    return;
                }
            loginBt.interactable = true;
        }

        public void ForgotPassword()
        {
            if (string.IsNullOrEmpty(emailForgotInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyEmail);
                return;
            }
            else if (!emailForgotInput.text.Contains(AppManager.APP_SETTINGS.EmailValidationCharacter))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmailNotValid);
                return;
            }
            Firebase.Auth.FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(emailForgotInput.text).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "There was a problem.\nCheck that the email is correct and associated with an existing account." });
                else if (task.IsCompleted)
                    AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "We have sent you an email to reset your password.", Callback = () => { emailForgotInput.text = ""; emailForgotInput.transform.parent.parent.gameObject.SetActive(false); } });
            });
        }

        public void AttivaForgotBt(string s) => forgotBt.interactable = s != string.Empty;
    }
}