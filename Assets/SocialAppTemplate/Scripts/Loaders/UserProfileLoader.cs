using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SocialApp
{

    public class UserProfileLoader : MonoBehaviour
    {

        [SerializeField]
        private ImageSize ProfileImageSize = ImageSize.Size_512;
        [SerializeField]
        private ProfileType CurrentProfileType = ProfileType.My;
        [SerializeField]
        private List<ImageSize> SizesToUpload = new List<ImageSize>();
        [SerializeField]
        private Image ProfileImage = default;
        [SerializeField]
        private RectTransform ProfileImageRect = default;
        [SerializeField]
        private TMP_InputField NewsInput = default;
        [SerializeField]
        private Text UserNameLabel = default;
        [SerializeField]
        private Text FriendsCountLabel = default;
        [SerializeField]
        private GameObject AddToFriendBtn = default;
        [SerializeField]
        private GameObject SendMessageBtn = default;
        [SerializeField]
        private GameObject SendPostBlocker = default;
        [SerializeField]
        private GameObject[] FriendsAvatarObjects = default;
        [SerializeField]
        private FeedDadaUoloader FeedUploader = default;
        [SerializeField]
        private Text InteractionsCount;
        public Canvas accountBannato;
        private string CurrentUserID;

        private float DefaultProfileWidth;
        private float DefaultProfileHeight;
        [SerializeField]
        private Image CustomBackgroundImage = default, CustomBackgroundImagePreview;

        public void SelectPhotoFromGalleryForCustomBackground()
        {
            NativeGallery.GetImageFromGallery(OnBackgroundPicked);
        }

        private void OnBackgroundPicked(string _path)
        {
            if (string.IsNullOrEmpty(_path))
                return;
            StartCoroutine(UploadCustomBackground(_path));
        }

        private IEnumerator UploadCustomBackground(string _path)
        {
            AppManager.VIEW_CONTROLLER.ShowLoading();
            Texture2D _texture = NativeGallery.LoadImageAtPath(_path, -1, false, false);
            ImageSize _size = ImageSize.Size_512;
            bool isFinishUpload = false;
            bool isSuccess = false;
            UploadImageRequest uploadRequest = new UploadImageRequest();
            uploadRequest.OwnerId = AppManager.USER_PROFILE.FIREBASE_USER.UserId;
            ResizeTexture(_texture, _size);
            byte[] uploadBytes = ImageConversion.EncodeToJPG(_texture, AppManager.APP_SETTINGS.UploadImageQuality);
            uploadRequest.ImageBytes = uploadBytes;
            uploadRequest.Size = _size;
            AppManager.FIREBASE_CONTROLLER.UploadCustomBackgroundVaultPLUS(uploadRequest, _callback =>
            {
                isFinishUpload = _callback.IsFinish;
                isSuccess = _callback.IsSuccess;
            });
            while (!isFinishUpload)
            {
                yield return null;
            }
            if (!isSuccess)
            {
                AppManager.VIEW_CONTROLLER.HideLoading();
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.FailedUploadImage);
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                yield break;
            }
            AppManager.VIEW_CONTROLLER.HideLoading();
            GetProfileCustomBackground();
            //FeedUploader.PostProfileUpdate(_path);
        }

        private void GetProfileCustomBackground()
        {
            GetProfileImageRequest _request = new GetProfileImageRequest();
            _request.Id = CurrentUserID;
            _request.Size = ProfileImageSize;
            AppManager.FIREBASE_CONTROLLER.GetProfileCustomBackgroundVaultPLUS(_request, OnCustomBackgroundGetted);
        }

        public void OnCustomBackgroundGetted(GetProfileImageCallback _callback)
        {
            if (_callback.IsSuccess)
            {
                AppManager.USER_PROFILE.CUSTOM_BACKGROUND_LOADED = true;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(_callback.ImageBytes);
                CustomBackgroundImage.preserveAspect = CustomBackgroundImagePreview.preserveAspect = true;
                CustomBackgroundImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                CustomBackgroundImagePreview.sprite = CustomBackgroundImage.sprite;
                CustomBackgroundImage.color = Color.white;
                CustomBackgroundImagePreview.color = Color.white;
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
            }
            else
            {
                DisplayDefaultBackground();
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
            }
        }

        private void DisplayDefaultBackground()
        {
            CustomBackgroundImagePreview.sprite = null;
            CustomBackgroundImage.color = new Color(1, 1, 1, 0);
        }

        private void Awake()
        {
            DefaultProfileWidth = ProfileImageRect.rect.width;
            DefaultProfileHeight = ProfileImageRect.rect.height;
        }

        public void OnEnable()
        {
            if (CurrentProfileType == ProfileType.My)
            {
                LoadMyInfo();
            }
            else
            {
                ProfileImageRect.sizeDelta = new Vector2(DefaultProfileWidth, DefaultProfileHeight);
            }
        }

        private void LoadMyInfo()
        {
            CurrentUserID = AppManager.USER_PROFILE.FIREBASE_USER.UserId;
            if (!AppManager.USER_PROFILE.PROFILE_IMAGE_LOADED)
            {
                ProfileImageRect.sizeDelta = new Vector2(DefaultProfileWidth, DefaultProfileHeight);
                DisplayDefaultAvatar();
                GetProfileImage();
            }
            if (AppSettings.VaultPLUS && !AppManager.USER_PROFILE.CUSTOM_BACKGROUND_LOADED)
            {
                DisplayDefaultBackground();
                GetProfileCustomBackground();
            }
            //NewsInput.text = string.Empty;
            AppManager.USER_PROFILE.GetUserFullName(_userName =>
            {
                UserNameLabel.text = _userName;
            });

            GetInteractionsCount();
            //GetFriendList();
        }
        
        public void LoadUserInfo(string _id)
        {
            CurrentUserID = _id;
            DisplayDefaultAvatar();
            GetProfileImage();
            //CheckFriendsList();
            UserNameLabel.text = string.Empty;
            NewsInput.text = string.Empty;
            AppManager.FIREBASE_CONTROLLER.GetUserFullName(CurrentUserID, (_userName =>
            {
                UserNameLabel.text = _userName;
            }));
            //GetFriendList();
        }

        public void GetInteractionsCount()
        {
            InteractionsCount.text = "?";
            InteractionsCount.transform.parent.gameObject.SetActive(true);
            AppManager.FIREBASE_CONTROLLER.GetInteractionsCount(_count =>
            {
                InteractionsCount.text = _count.ToString();
                InteractionsCount.transform.parent.gameObject.SetActive(_count != 0);
            });
        }

        private void GetFriendList()
        {/*
            FriendsCountLabel.text = string.Empty;
            foreach (GameObject _go in FriendsAvatarObjects)
            {
                _go.SetActive(false);
            }
            UsersQuery _usersQuery = new UsersQuery();
            _usersQuery.startIndex = 0;
            _usersQuery.endIndex = 6;
            _usersQuery.callback = OnFriendsLoaded;
            _usersQuery.forward = true;
            _usersQuery.ownerID = CurrentUserID;
            _usersQuery.Type = FriendsTabState.Friend;
            AppManager.FIREBASE_CONTROLLER.GetFriendsAt(_usersQuery);

            AppManager.FIREBASE_CONTROLLER.GetUserFriendsCount(CurrentUserID, _count =>
            {
                FriendsCountLabel.text = "Friends (" + _count + ")";
            });*/
        }

        public void OnFriendsLoaded(UsersCallback _callback)
        {
            if (_callback.IsSuccess)
            {
                for (int i = 0; i < _callback.users.Count; i++)
                {
                    FriendsAvatarObjects[i].SetActive(true);
                    FriendsAvatarObjects[i].GetComponent<AvatarViewController>().LoadAvatar(_callback.users[i].UserID);
                }
            }
        }

        private void CheckFriendsList()
        {
            AddToFriendBtn.SetActive(false);
            SendMessageBtn.SetActive(false);
            SendPostBlocker.SetActive(true);
            AppManager.FIREBASE_CONTROLLER.CanAddToFriend(CurrentUserID, _canAdd =>
            {
                if (_canAdd)
                {
                    AddToFriendBtn.SetActive(true);
                }
                else
                {
                    AppManager.FIREBASE_CONTROLLER.IsInFriendsList(CurrentUserID, _isFriend =>
                    {
                        if (_isFriend)
                        {
                            SendMessageBtn.SetActive(true);
                            SendPostBlocker.SetActive(false);
                        }
                    });
                }
            });
        }

        public void SelectPhotoFromGallery()
        {
            NativeGallery.GetImageFromGallery(OnImagePicked);
        }

        public void SelectPhotoFromCamera()
        {
            NativeCamera.TakePicture(OnImageTaken);
        }

        private void OnImagePicked(string _path)
        {
            if (string.IsNullOrEmpty(_path))
                return;
            StartCoroutine(UploadAvatar(_path));
        }

        private void OnImageTaken(string _path)
        {
            if (string.IsNullOrEmpty(_path))
                return;
            StartCoroutine(UploadAvatar(_path));
        }

        public void ResizeTexture(Texture2D _texture, ImageSize _size)
        {
            if (_size != ImageSize.Origin)
            {
                int _width = _texture.width;
                int _height = _texture.height;
                if (_width > _height)
                {
                    if (_width > (int)_size)
                    {
                        float _delta = (float)_width / (float)((int)_size);
                        _height = Mathf.FloorToInt((float)_height / _delta);
                        _width = (int)_size;
                    }
                }
                else
                {
                    if (_height > (int)_size)
                    {
                        float _delta = (float)_height / (float)((int)_size);
                        _width = Mathf.FloorToInt((float)_width / _delta);
                        _height = (int)_size;
                    }
                }
                TextureScale.Bilinear(_texture, _width, _height);
            }
        }

        private IEnumerator UploadAvatar(string _path)
        {
            print("carica " + 0 + "  " + _path);
            AppManager.VIEW_CONTROLLER.ShowLoading();
            Texture2D _texture = NativeGallery.LoadImageAtPath(_path, -1, false, false);
            for (int i = 0; i < SizesToUpload.Count; i++)
            {
                print("carica " + 1 + "" + i);
                ImageSize _size = SizesToUpload[i];
                bool isFinishUpload = false;
                bool isSuccess = false;
                UploadImageRequest uploadRequest = new UploadImageRequest();
                uploadRequest.OwnerId = AppManager.USER_PROFILE.FIREBASE_USER.UserId;
                ResizeTexture(_texture, _size);
                byte[] uploadBytes = ImageConversion.EncodeToJPG(_texture, AppManager.APP_SETTINGS.UploadImageQuality);
                uploadRequest.ImageBytes = uploadBytes;
                uploadRequest.Size = _size;
                AppManager.FIREBASE_CONTROLLER.UploadAvatar(uploadRequest, (_callback =>
                   {
                       isFinishUpload = _callback.IsFinish;
                       isSuccess = _callback.IsSuccess;
                   }
                ));
                while (!isFinishUpload)
                {
                    yield return null;
                }
                if (!isSuccess)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.FailedUploadImage);
                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                    yield break;
                }
            }
            AppManager.VIEW_CONTROLLER.HideLoading();
            GetProfileImage();
            FeedUploader.PostProfileUpdate(_path);
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
            print("carica " + 2);
        }

        private void GetProfileImage()
        {
            GetProfileImageRequest _request = new GetProfileImageRequest();
            _request.Id = CurrentUserID;
            _request.Size = ProfileImageSize;
            AppManager.FIREBASE_CONTROLLER.GetProfileImage(_request, OnProfileImageGetted);
        }

        public void OnProfileImageGetted(GetProfileImageCallback _callback)
        {
            if (_callback.IsSuccess)
            {
                AppManager.USER_PROFILE.PROFILE_IMAGE_LOADED = true;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(_callback.ImageBytes);
                ProfileImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                UpdateImageRect();
            }
            else
            {
                DisplayDefaultAvatar();
            }
        }

        private void DisplayDefaultAvatar()
        {
            Texture2D texture = AppManager.APP_SETTINGS.DefaultAvatarTexture;
            ProfileImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            UpdateImageRect();
        }

        public void ShowUserFriends()
        {
            AppManager.VIEW_CONTROLLER.HideNavigationGroup();
            if (CurrentProfileType == ProfileType.My)
            {
                //AppManager.VIEW_CONTROLLER.ShowUserFriend(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
            }
            else if (CurrentProfileType == ProfileType.Another)
            {
                //AppManager.VIEW_CONTROLLER.ShowUserFriend(CurrentUserID);
            }
        }

        private void UpdateImageRect()
        {
            ProfileImage.preserveAspect = true;
            float _bodyWidth = ProfileImageRect.rect.width;
            float _bodyHeight = ProfileImageRect.rect.height;
            float _imageWidth = (float)ProfileImage.sprite.texture.width;
            float _imageHeight = (float)ProfileImage.sprite.texture.height;
            float _ratio = _imageWidth / _imageHeight;
            if (_imageWidth > _imageHeight)
            {
                _ratio = _imageHeight / _imageWidth;
            }
            float _expectedHeight = _bodyWidth / _ratio;
            if (_imageWidth > _imageHeight)
            {
                ProfileImageRect.sizeDelta = new Vector2(_expectedHeight, _bodyHeight);
            }
            else
            {
                ProfileImageRect.sizeDelta = new Vector2(_bodyWidth, _expectedHeight);
            }

        }

        public void AddToFriend()
        {
            if (string.IsNullOrEmpty(CurrentUserID))
                return;
            AppManager.FIREBASE_CONTROLLER.AddToFriends(CurrentUserID, OnAddedToFriend);
        }

        private void OnAddedToFriend()
        {
            AddToFriendBtn.SetActive(false);
        }

        public void ShowYourInfo()
        {
            AppManager.USER_INFO.ShowUserInfo(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
        }
    }

    public enum ProfileType
    {
        My,
        Another
    }

    public class UploadImageRequest
    {
        public byte[] ImageBytes;
        public string OwnerId;
        public ImageSize Size;
    }

    public class UploadImageCallBack
    {
        public bool IsFinish = false;
        public bool IsSuccess = false;
    }

    public class GetProfileImageRequest
    {
        public string Id;
        public ImageSize Size;
    }

    public class GetProfileImageCallback
    {
        public bool IsSuccess;
        public byte[] ImageBytes;
        public string DownloadUrl;
    }
}