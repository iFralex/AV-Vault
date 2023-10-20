using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniTools;
using System.Collections;
using System;

namespace SocialApp
{
    public class MessageViewController : MonoBehaviour
    {
        [SerializeField]
        private Image BubleImage = default;
        [SerializeField]
        private Image ContentImage = default;
        [SerializeField]
        public Text BodyText = default;
        [SerializeField]
        private Text UserNameText = default;
        [SerializeField]
        private Text DateText = default;
        public Button reportBt;

        [SerializeField]
        private RectTransform TextRect = default;
        [SerializeField]
        private RectTransform BubleRect = default;
        [SerializeField]
        private RectTransform ContentRect = default;
        [SerializeField]
        private RectTransform MainRect = default;
        [SerializeField]
        private RectTransform ProfileRect = default;
        [SerializeField]
        private float StartBubbleWidth = default;

        [SerializeField]
        private Color UserBubbleColor = default;
        [SerializeField]
        private Color MyBubbleColor = default;
        [SerializeField]
        private AvatarViewController AvatarView = default;
        [SerializeField]
        private OpenHyperlinks LinksChecker = default;
        [SerializeField]
        private bool CacheAvatar = default;
        
        public Message CurrentMessage;

        private Vector2 TextOffsetMin;
        private Vector2 BubbleOffsetMin;
        private Vector2 BubbleOffsetMax;
        private float MaxContentWidth = 600f;

        private void Awake()
        {
            SaveResetValue();
        }

        private void SaveResetValue()
        {
            TextOffsetMin = TextRect.offsetMin;
            BubbleOffsetMin = BubleRect.offsetMin;
            BubbleOffsetMax = BubleRect.offsetMax;
        }

        public void LoadMedia(Message _msg)
        {
            gameObject.SetActive(true);
            AvatarView.SetCacheTexture(CacheAvatar);
            ResetRects();
            CurrentMessage = _msg;
            if (_msg.Type == ContentMessageType.TEXT)
            {
                LoadText();
            }
            else if (_msg.Type == ContentMessageType.IMAGE)
            {
                LoadContent();
            }
            LoadGraphics();
            StartCoroutine(UpdateUIRectCor());
            GetProfileImage();
        }

        IEnumerator UpdateUIRectCor()
        {
            yield return null;
            UpdateUIRect();
            transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(transform.parent.GetComponent<RectTransform>().sizeDelta.x, transform.parent.GetComponent<RectTransform>().sizeDelta.y + BubleRect.rect.height + transform.parent.GetComponent<VerticalLayoutGroup>().spacing);
            BodyText.transform.parent.GetComponent<RectTransform>().sizeDelta = BodyText.GetComponent<RectTransform>().sizeDelta;
        }

        public void LoadGraphics()
        {
            if (isMine())
            {
                BubleImage.color = MyBubbleColor;
            }
            else
            {
                BubleImage.color = UserBubbleColor;
            }
        }

        public void LoadText()
        {
            AvatarView.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            AvatarView.GetComponentInChildren<Button>().onClick.AddListener(() => { if (AppManager.IsAdmin()) AppManager.FIREBASE_CONTROLLER.DeleteImageProfile(CurrentMessage.UserID, s => { if (s) AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Deleted avatar!", Message = "" }); }); });
            DateText.GetComponent<Button>().onClick.RemoveAllListeners();
            DateText.GetComponent<Button>().onClick.AddListener(() => { if (AppManager.IsAdmin()) AppManager.FIREBASE_CONTROLLER.DeleteComment(CurrentMessage.Key, MessagesDataLoader.loadedID, s => { if (s) AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Deleted comment!", Message = "" }); }); });
            reportBt.onClick.RemoveAllListeners();
            reportBt.onClick.AddListener(() => { AppManager.USER_SETTINGS.ReportComment(CurrentMessage.Key, CurrentMessage.TargetId); reportBt.transform.parent.gameObject.SetActive(false); });
            BodyText.text = CurrentMessage.BodyTXT;
            UserNameText.text = CurrentMessage.FullName;
            if (CurrentMessage.DateCreated.Length > 6)
            {
                DateTime data = new DateTime();
                if (DateTime.TryParse(CurrentMessage.DateCreated, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), System.Globalization.DateTimeStyles.None, out data))
                    DateText.text = (data + (DateTime.Now - DateTime.UtcNow)).ToString("yy/MM/dd HH:mm");
                else
                    DateText.text = CurrentMessage.DateCreated;
            }
            else
                DateText.text = string.Empty;
            //LinksChecker.CheckLinks();
            //ContentImage.gameObject.SetActive(false);
        }

        public void LoadContent()
        {
            UserNameText.text = CurrentMessage.FullName;
            DateText.text = CurrentMessage.DateCreated;
            ContentImage.gameObject.SetActive(true);
            ContentImage.color = Color.grey;
            float width = CurrentMessage.MediaInfo.ContentWidth;
            float height = CurrentMessage.MediaInfo.ContentHeight;
            if (width > MaxContentWidth)
            {
                height = MaxContentWidth * height / width;
                width = MaxContentWidth;
            }

            ContentRect.sizeDelta = new Vector2(width, height);
            ContentRect.anchoredPosition = new Vector2(ContentRect.anchoredPosition.x, -height/2f);

            if (!string.IsNullOrEmpty(CurrentMessage.MediaInfo.ContentURL))
            {
                CoroutineExecuter _ce = new CoroutineExecuter();
                ImageService _is = new ImageService(_ce);
                _is.DownloadOrLoadTexture(CurrentMessage.MediaInfo.ContentURL, _texture =>
                {
                    if (_texture != null)
                    {
                        ContentImage.color = Color.white;
                        ContentImage.sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    }
                });
            }
        }

        private void ResetRects()
        {
            TextRect.offsetMin = TextOffsetMin;
            BubleRect.offsetMin = BubbleOffsetMin;
            BubleRect.offsetMax = BubbleOffsetMax;
        }

        private bool isMine()
        {
            if (AppManager.USER_PROFILE.FIREBASE_USER == null)
                return false;
            return CurrentMessage.UserID == AppManager.USER_PROFILE.FIREBASE_USER.UserId;
        }
        
        private void UpdateUIRect()
        {
            if (CurrentMessage.Type == ContentMessageType.TEXT)
            {
                // update buble text rect
                /*float txtPreferredWidth = BodyText.preferredWidth;
                if (txtPreferredWidth > TextRect.rect.width)
                    txtPreferredWidth = TextRect.rect.width;
                TextRect.offsetMin = new Vector2(TextRect.offsetMin.x, TextRect.offsetMin.y - BodyText.preferredHeight + (float)BodyText.fontSize);
                BubleRect.offsetMin = new Vector2(BubleRect.offsetMin.x, BubleRect.offsetMin.y - TextRect.rect.height + StartBubbleWidth);
                BubleRect.offsetMax = new Vector2(BubleRect.offsetMax.x + txtPreferredWidth - StartBubbleWidth, BubleRect.offsetMax.y);*/
                BubleRect.sizeDelta = new Vector2(BubleRect.rect.width, ProfileRect.rect.height + TextRect.rect.height);
            }
            else if (CurrentMessage.Type == ContentMessageType.IMAGE)
            {
                // update buble content rect
                BubleRect.offsetMin = new Vector2(BubleRect.offsetMin.x, BubleRect.offsetMin.y - ContentRect.rect.height + StartBubbleWidth);
                BubleRect.offsetMax = new Vector2(BubleRect.offsetMax.x + ContentRect.rect.width - StartBubbleWidth, BubleRect.offsetMax.y );
            }
            // update message rect
            //float _height = BubleRect.rect.height + ProfileRect.rect.height;
            //MainRect.sizeDelta = new Vector2(MainRect.rect.width, _height);
        }

        private void GetProfileImage()
        {
            AvatarView.LoadAvatar(CurrentMessage.UserID);
        }
    }
}
