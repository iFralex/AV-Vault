using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SocialApp
{
    public class FeedPreviewController : MonoBehaviour
    {

        [SerializeField]
        private Image PreviewImage = default;
        [SerializeField]
        private Text CommentInput = default;

        private FeedPreviewRequest CurrentRequest;

        private void OnDisable() => CancelPost();

        public void DisplayPreview(FeedPreviewRequest _request)
        {
            CommentInput.text = string.Empty;
            CurrentRequest = _request;
            PreviewImage.sprite = Sprite.Create(_request.PreviewImage, new Rect(0.0f, 0.0f, _request.PreviewImage.width, _request.PreviewImage.height), new Vector2(0.5f, 0.5f), 100.0f);
            PreviewImage.preserveAspect = true;
        }

        public void CancelPost()
        {
            CurrentRequest.BodyText = CommentInput.text;
            CurrentRequest.IsComplete = true;
            CurrentRequest.IsSuccess = false;
            CurrentRequest = null;
            AppManager.VIEW_CONTROLLER.HideFeedPreview();
        }

        public void StartPost()
        {
            CurrentRequest.IsComplete = true;
            CurrentRequest.IsSuccess = true;
            CurrentRequest.BodyText = CommentInput.text;

            CurrentRequest = null;
            AppManager.VIEW_CONTROLLER.HideFeedPreview();
        }
    }
}