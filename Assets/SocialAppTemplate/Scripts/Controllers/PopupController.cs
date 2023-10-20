using UnityEngine;
using UnityEngine.UI;
using System;

namespace SocialApp
{
    public class PopupController : MonoBehaviour
    {

        [SerializeField]
        private Text TitleLabel = default;
        [SerializeField]
        private Text MessageLabel = default;

        private Action Callback;

        public void ShowMessage(PopupMessage _msg)
        {
            TitleLabel.text = _msg.Title;
            MessageLabel.text = _msg.Message;
            Callback = _msg.Callback;
        }

        public void CloseWindow()
        {
            if (Callback != null)
            {
                Callback.Invoke();
            }
            AppManager.VIEW_CONTROLLER.HidePopupMessage();
        }
    }

    public class PopupMessage
    {
        public string Title;
        public string Message;
        public Action Callback = null;
    }
}
