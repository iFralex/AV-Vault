using UnityEngine;
using System;

namespace SocialApp
{
    public class FeedPopupViewController : MonoBehaviour
    {
        private Action<FeedPopupAction> CurrentAction;
        
        private void OnDisable()
        {
            CurrentAction = null;
        }

        public void SetupWindows(Action<FeedPopupAction> _action)
        {
            CurrentAction = _action;
        }

        public void OnDeletePost()
        {
            CurrentAction?.Invoke(FeedPopupAction.DELETE);
            HideWindows();
        }

        public void OnDeleteUser()
        {
            CurrentAction?.Invoke(FeedPopupAction.DELETE_USER);
            HideWindows();
        }

        public void OnPostInMainFeed()
        {
            CurrentAction?.Invoke(FeedPopupAction.PREMIUM);
            HideWindows();
        }

        public void OnReportPost()
        {
            CurrentAction?.Invoke(FeedPopupAction.REPORT_POST);
            HideWindows();
        }

        public void OnBlockUser()
        {
            CurrentAction?.Invoke(FeedPopupAction.BLOCK_USER);
            HideWindows();
        }

        public void OnSavePost()
        {
            CurrentAction?.Invoke(FeedPopupAction.SAVE_POST);
            HideWindows();
        }

        public void HideWindows()
        {
            AppManager.VIEW_CONTROLLER.HideFeedPopup();
        }
    }

    public enum FeedPopupAction
    {
        NONE,
        DELETE,
        DELETE_USER,
        PREMIUM,
        REPORT_POST,
        BLOCK_USER,
        SAVE_POST,
    }
}
