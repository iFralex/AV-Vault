using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocialApp;

namespace SocialApp
{
    public class RegistrationController : MonoBehaviour
    {

        [SerializeField]
        private InputField EmailInput = default;
        [SerializeField]
        private InputField FirstNameInput = default;
        public InputField PasswordInput = default;
        [SerializeField]
        private InputField ConfirmPasswordInput = default;
        public Button pulsante;

        private void OnEnable()
        {
            ClearFields();
        }

        public void SendRegistration()
        {
            if (CheckError())
            {
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Warning);
                return;
            }
            string _login = EmailInput.text.Trim();
            string _password = PasswordInput.text.Trim();
            AppManager.FIREBASE_CONTROLLER.AddNewUser(_login, _password, OnRegistrationComplete);
            AppManager.VIEW_CONTROLLER.ShowLoading();
        }

        public void CloseWindow()
        {
            AppManager.VIEW_CONTROLLER.HideRegistration();
            AppManager.VIEW_CONTROLLER.ShowLogin();
        }

        public void OnRegistrationComplete(RegistrationMessage _msg)
        {
            if (_msg.IsComplete)
            {
                string _login = EmailInput.text.Trim();
                string _password = PasswordInput.text.Trim();
                AppManager.FIREBASE_CONTROLLER.LogIn(_login, _password, OnLoginComplete, true);
            }
            else
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = _msg.ErrorMessage;
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
                AppManager.VIEW_CONTROLLER.HideLoading();
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
            }
        }

        public void OnLoginComplete(LoginMessage _msg)
        {
            if (_msg.IsSuccess)
            {
                User _user = new User();
                _user.UserID = _msg.UserID;
                _user.DataRegistration = AppManager.DEVICE_CONTROLLER.GetSystemDate();
                _user.FirstName = FirstNameInput.text;
                //_user.LastName = LastNameInput.text;
                _user.FullName = FirstNameInput.text;
                AppManager.FIREBASE_CONTROLLER.SetUserData(_user, OnSetUserDataComplete);
            }
            else
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = _msg.ErrorMessage;
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
                AppManager.VIEW_CONTROLLER.HideLoading();
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
            }
        }

        public void OnSetUserDataComplete(SetUserDataMessage _msg)
        {
            if (_msg.IsSuccess)
            {
                if (AppManager.APP_SETTINGS.UseEmailConfirm)
                {
                    print("1");
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.RegistrationSuccessWithConfirm, CloseWindow);
                    AppManager.FIREBASE_CONTROLLER.SendVerifcationEmail();
                }
                else
                {
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.RegistrationSuccess, CloseWindow);
                }
                AppManager.VIEW_CONTROLLER.HideLoading();
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
                AppManager.LOGIN_CONTROLLER.EmailInput.text = EmailInput.text;
                AppManager.LOGIN_CONTROLLER.PasswordInput.text = PasswordInput.text;
            }
            else
            {
                PopupMessage msg = new PopupMessage();
                msg.Title = "Error";
                msg.Message = _msg.ErrorMessage;
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(msg);
                AppManager.VIEW_CONTROLLER.HideLoading();
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
            }
        }

        private void ClearFields()
        {
            EmailInput.text = string.Empty;
            FirstNameInput.text = string.Empty;
            //LastNameInput.text = string.Empty;
            PasswordInput.text = string.Empty;
            ConfirmPasswordInput.text = string.Empty;
        }

        private bool CheckError()
        {
            bool IsError = false;
            if (string.IsNullOrEmpty(EmailInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyEmail);
                IsError = true;
            }
            if (string.IsNullOrEmpty(FirstNameInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyFirstName);
                IsError = true;
            }
            /*if (string.IsNullOrEmpty(LastNameInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyLastName);
                IsError = true;
            }*/
            else if (string.IsNullOrEmpty(PasswordInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmptyPassword);
                IsError = true;
            }
            else if (!string.Equals(ConfirmPasswordInput.text, ConfirmPasswordInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.PasswordNotMatch);
                IsError = true;
            }
            else if (!EmailInput.text.Contains(AppManager.APP_SETTINGS.EmailValidationCharacter) || EmailInput.text.Length - EmailInput.text.IndexOf(System.Convert.ToChar("@")) < 5)
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
        public void Check(bool b) => pulsante.interactable = b;
    }
}