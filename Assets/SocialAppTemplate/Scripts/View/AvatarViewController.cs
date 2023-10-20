using UnityEngine;
using UnityEngine.UI;
using UniTools;
using System.Collections;

namespace SocialApp
{
    public class AvatarViewController : MonoBehaviour
    {
        [SerializeField]
        private Image AvatarImage = default;
        [SerializeField]
        private RectTransform AvatarRect = default;
        [SerializeField]
        private float AvatarSize = default;
        float DefaultAvatarSize = default;
        [SerializeField]
        private bool CacheTexture = default;


        private string CurrentUserID;

        public void Start() => DefaultAvatarSize = AvatarSize;
        public void LoadBigAvatar(string _id)
        {
            ClearData();
            CurrentUserID = _id;
            GetProfileImage(ImageSize.Size_512);
        }

        public void LoadAvatar(string _id)
        {
            ClearData();
            CurrentUserID = _id;
            GetProfileImage(ImageSize.Size_256);
        }

        private void OnDisable()
        {
            AvatarImage.sprite = null;
        }

        private void ClearData()
        {
            DisplayDefaultAvatar();
            CurrentUserID = string.Empty; ;
            SetFollowerBorder(false);
        }

        private void GetProfileImage(ImageSize _size)
        {
            GetProfileImageRequest _request = new GetProfileImageRequest();
            _request.Id = CurrentUserID;
            _request.Size = _size;
            if (CacheTexture || AppManager.USER_PROFILE.IsMine(CurrentUserID))
                AppManager.FIREBASE_CONTROLLER.GetProfileImageUrl(_request, OnProfileImageUrlGetted);
            else
                AppManager.FIREBASE_CONTROLLER.GetProfileImage(_request, OnProfileImageGetted);
            AppManager.FIREBASE_CONTROLLER.CheckYouAreAFollower(CurrentUserID, SetFollowerBorder);
        }

        public void OnProfileImageGetted(GetProfileImageCallback _callback)
        {
            if (_callback.IsSuccess)
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(_callback.ImageBytes);
                AvatarImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                ResizeAvarar(AvatarSize);
            }
            else
            {
                DisplayDefaultAvatar();
            }
        }

        public void OnProfileImageUrlGetted(GetProfileImageCallback _callback)
        {
            if (_callback.IsSuccess)
            {
                if (_callback.DownloadUrl != null)
                {
                    CoroutineExecuter _ce = new CoroutineExecuter();
                    ImageService _is = new ImageService(_ce);
                    _is.DownloadOrLoadTexture(_callback.DownloadUrl, _texture =>
                    {
                        if (_texture != null)
                        {
                            AvatarImage.sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, _texture.width, _texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                            ResizeAvarar(AvatarSize);
                        }
                        else
                        {
                            DisplayDefaultAvatar();
                        }
                    });
                }
                else
                {
                    DisplayDefaultAvatar();
                }
            }
            else
            {
                DisplayDefaultAvatar();
            }
        }

        public void DisplayDefaultAvatar()
        {
            if (AppManager.APP_SETTINGS != null)
            {
                Texture2D _defaultAvatar = AppManager.APP_SETTINGS.DefaultAvatarTexture;
                AvatarImage.sprite = Sprite.Create(_defaultAvatar, new Rect(0.0f, 0.0f, _defaultAvatar.width, _defaultAvatar.height), new Vector2(0.5f, 0.5f), 100.0f);
                ResizeAvarar(AvatarSize);
            }
        }

        public void DisplayGroupChatAvatar()
        {
            if (AppManager.APP_SETTINGS != null)
            {
                Texture2D _defaultAvatar = AppManager.APP_SETTINGS.DefaultGroupChatTexture;
                AvatarImage.sprite = Sprite.Create(_defaultAvatar, new Rect(0.0f, 0.0f, _defaultAvatar.width, _defaultAvatar.height), new Vector2(0.5f, 0.5f), 100.0f);
                ResizeAvarar(AppSettings.DefaultGroupChatIconSize);
            }
        }

        private void ResizeAvarar(float _size)
        {
            float _bodyWidth = _size;
            float _bodyHeight = _size;
            float _imageWidth = (float)AvatarImage.sprite.texture.width;
            float _imageHeight = (float)AvatarImage.sprite.texture.height;
            float _ratio = _imageWidth / _imageHeight;
            if (_imageWidth > _imageHeight)
            {
                _ratio = _imageHeight / _imageWidth;
            }
            float _expectedHeight = _bodyWidth / _ratio;
            AvatarRect.sizeDelta = new Vector2(AvatarSize, AvatarSize);
            //AvatarRect.sizeDelta = new Vector2(AvatarSize, AvatarSize);
            if (_imageWidth > _imageHeight)
            {
                AvatarImage.rectTransform.sizeDelta = new Vector2(_expectedHeight, _bodyHeight);
            }
            else
            {
                AvatarImage.rectTransform.sizeDelta = new Vector2(_bodyWidth, _expectedHeight);
            }
        }

        public void SetCacheTexture(bool _value)
        {
            CacheTexture = _value;
        }

        public void SetFollowerBorder(bool b) => AvatarRect.GetComponent<Image>().enabled = b;
    }
}
