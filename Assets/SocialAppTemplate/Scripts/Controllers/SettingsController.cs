using System.Collections;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace SocialApp
{
    public class SettingsController : MonoBehaviour
    {
        public GameObject pan;
        public InputField firstNameInput, lastNameInput, emailInput, passwordInput, confirmPasswordInput, currentPasswordInput, moderatorIf;
        public Button cambiaNomeBt, cambiaEmailBt, cambiaPasswordBt;

        public void ChiudiMenù()
        {
            StartCoroutine(Apri());
        }

        IEnumerator Apri()
        {
            pan.GetComponent<Animator>().SetBool("avvia", false);
            yield return new WaitForSeconds(.5f);
            pan.SetActive(false);
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.SoftImpact);
        }

        public void ApriMenù()
        {
            pan.SetActive(true);
            pan.GetComponent<Animator>().SetBool("avvia", true);
        }

        public void CambiaNome()
        {
            if (firstNameInput.text.Length < 6)
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "The username is too short. It must be at least 6 characters long." });
                return;
            }
            AppManager.FIREBASE_CONTROLLER.UpdateUserName(firstNameInput.text);
        }

        public void AttivaPulsanteCambaiNome() => cambiaNomeBt.interactable = !string.IsNullOrEmpty(firstNameInput.text);

        public void CambiaEmail()
        {
            if (!emailInput.text.Contains(AppManager.APP_SETTINGS.EmailValidationCharacter) || emailInput.text.Length - emailInput.text.IndexOf(System.Convert.ToChar("@")) < 5)
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.EmailNotValid);
                return;
            }
            AppManager.VIEW_CONTROLLER.ShowLoading();
            Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UpdateEmailAsync(emailInput.text).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.UpdateEmailFailed);
                }
                else if (task.IsCompleted)
                {
                    Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.SendEmailVerificationAsync().ContinueWithOnMainThread(task1 =>
                    {
                        if (task1.IsCompleted)
                        {
                            AppManager.VIEW_CONTROLLER.HideLoading();
                            Logout();
                        }
                    });
                }
            });
        }

        public void AttivaPulsanteCambaiEmail() => cambiaEmailBt.interactable = !string.IsNullOrEmpty(emailInput.text);

        public void CambiaPassword()
        {
            if (!string.Equals(currentPasswordInput.text, PlayerPrefs.GetString(AppSettings.PasswordSaveKey)))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.CurrentPasswordIsIncorrect);
                return;
            }
            else if (!string.Equals(passwordInput.text, confirmPasswordInput.text))
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.PasswordNotMatch);
                return;
            }
            else if (passwordInput.text.Length < AppManager.APP_SETTINGS.MinAllowPasswordCharacters)
            {
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.SmallPassword);
                return;
            }
            AppManager.VIEW_CONTROLLER.ShowLoading();
            Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UpdatePasswordAsync(passwordInput.text).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                }
                else if (task.IsCompleted)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    Logout();
                }
            });
        }

        public void AttivaPulsanteCambaiPassword() => cambiaPasswordBt.interactable = !string.IsNullOrEmpty(passwordInput.text) && !string.IsNullOrEmpty(confirmPasswordInput.text) && !string.IsNullOrEmpty(currentPasswordInput.text);

        public void Logout(bool _signout = true)
        {
            AppManager.NAVIGATION.RemoveListeners();
            PlayerPrefs.DeleteKey(AppSettings.LoginSaveKey);
            PlayerPrefs.DeleteKey(AppSettings.PasswordSaveKey);
            AppManager.VIEW_CONTROLLER.HideAllScreen();
            AppManager.VIEW_CONTROLLER.ShowPrima();
            AppManager.VIEW_CONTROLLER.primaPaginaObject.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            //AppManager.DEVICE_CONTROLLER.StopOnlineChecker();
            AppManager.FIREBASE_CONTROLLER.RemoveDeviceTokens();
            AppManager.FIREBASE_CONTROLLER.RemovePushNotificationEvents();
            if (_signout)
                AppManager.LOGIN_CONTROLLER.OnSignOut();
            AppSettings.TutorialMode = FasiTutorial.Nulla;
            AppSettings.VaultPLUS = false;
            //AppManager.FIREBASE_CONTROLLER.LogOut();
            AppManager.USER_PROFILE.ClearUser();
        }

        public void EliminAccount()
        {
            AppManager.FIREBASE_CONTROLLER.DeleteUserDada(Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId, true);
        }

        public void AddModerator()
        {
            AppManager.FIREBASE_CONTROLLER.AddModerator(moderatorIf.text, s => { if (s) AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "You added " + moderatorIf.text }); moderatorIf.text = ""; });
        }
        
        public void ReportComment(string id, string postId)
        {
            AppManager.VIEW_CONTROLLER.ShowLoading();
            AppManager.FIREBASE_CONTROLLER.ReportComments(id, postId, s =>
            {
                if (s)
                    AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Success", Message = "You have reported this comment.\nThanks for your help to make the app better." });
                  else
                    AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "Impossible to report this comment.\nTry again." });
                AppManager.VIEW_CONTROLLER.HideLoading();
            });
        }
    }
}