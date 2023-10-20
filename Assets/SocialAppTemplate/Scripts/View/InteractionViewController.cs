using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SocialApp
{
    public class InteractionViewController : MonoBehaviour
    {
        [SerializeField]
        private Image CommentsImage = default;
        [SerializeField]
        private Image LikeImage = default;
        [SerializeField]
        public Text BodyText = default;
        Vector2 iniz = Vector2.one * -100000;
        InteractionViewController _interact;

        public Interaction CurrentInteraction;

        void Start() => _interact = this;

        public void LoadMedia(Interaction _interaction)
        {
            gameObject.SetActive(true);
            CurrentInteraction = _interaction;
            LoadText();
        }

        public void LoadText()
        {
            BodyText.text = "<b>" + CurrentInteraction.UserID + "</b> " + (CurrentInteraction.Type == (int)InteractionType.Like ? "liked" : "commented on") + " your post on <i>" + (CurrentInteraction.Date.Length > 6 ? (DateTime.Parse(CurrentInteraction.Date) + (DateTime.Now - DateTime.UtcNow)).ToString("yy/MM/dd HH:mm:ss") : string.Empty) + "</i>";
            CommentsImage.gameObject.SetActive(CurrentInteraction.Type == (int)InteractionType.Comment);
            LikeImage.gameObject.SetActive(CurrentInteraction.Type == (int)InteractionType.Like);
        }
        
        public void IniziaClickApriInterazione() => iniz = Input.mousePosition;

        public void FineClickApriInterazione()
        {
            if (iniz == Vector2.one * -100000)
                return;
            if (Vector2.Distance(iniz, Input.mousePosition) < Screen.width / 15 && _interact.CurrentInteraction != null)
                AppManager.DYNAMICS_CONTROLLER.OnDynamicLink("memeId=" + _interact.CurrentInteraction.PostID);
            iniz = Vector2.one * -100000;
        }
    }
}