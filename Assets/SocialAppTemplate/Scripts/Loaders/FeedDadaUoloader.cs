using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using UnityEngine.Video;
using System;
using TMPro;
using SocialApp;
using Firebase.DynamicLinks;

namespace SocialApp
{
    public class FeedDadaUoloader : MonoBehaviour
    {
        public GameObject avviso;
        public string[] paroleProib;
        [SerializeField]
        private InputField BodyTextInput = default;
        [SerializeField]
        private VideoPlayer VPlayer = default;
        [SerializeField]
        private FeedsDataLoader feedDadaLoader = default;
        public Button postBt;
        private int FeedIndex = 0;
        // 0 - text
        // 1 - image
        // 2 - video


        public void SelectPhotoFromGallery()
        {
            NativeGallery.GetImageFromGallery(OnImagePicked);
        }

        public void SelectPhotoFromCamera()
        {
            NativeCamera.TakePicture(OnImagePicked);
        }

        private void OnImagePicked(string _path)
        {
            print(_path);
            if (string.IsNullOrEmpty(_path))
                return;
            FeedIndex = 1;
            UploadFile(_path);
        }

        public void SelectVideoFromGallery()
        {
            NativeGallery.GetVideoFromGallery(OnVideoPicked);
        }

        public void SelectVideoFromCamera()
        {
            NativeCamera.RecordVideo(OnVideoPicked);
        }

        private void OnVideoPicked(string _path)
        {
            if (string.IsNullOrEmpty(_path))
                return;
            FeedIndex = 2;
            UploadFile(_path);
        }

        public void PostProfileUpdate(string _url)
        {
            FeedIndex = 1;
            UploadFile(_url, false);
        }

        public void PostSimpleText()
        {
            if (string.IsNullOrEmpty(BodyTextInput.text))
                return;
            FeedIndex = 0;
            UploadFile(string.Empty);
        }

        public void UploadFile(string _url, bool _showPreview = true)
        {
            AppManager.VIEW_CONTROLLER.ShowLoading();
            StartCoroutine(GetFile(_url, _showPreview));
        }

        private IEnumerator GetFile(string _url, bool _showPreview)
        {
            string s = BodyTextInput.text.ToLower();
            for (int i = 0; i < paroleProib.Length; i++)
                if (s.Contains(paroleProib[i]))
                {
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.PostFailed);
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                    yield break;
                }

            FeedType _type = GetFeedType();
            Feed _feed = new Feed();
            _feed.Type = _type;
            _feed.OwnerID = AppManager.USER_PROFILE.FIREBASE_USER.UserId;
            _feed.ToUserID = feedDadaLoader.GetUserID();
            byte[] imageBytes = null;
            byte[] videoBytes = null;
            string fileName = Guid.NewGuid().ToString();
            Texture2D previewTexure = new Texture2D(2, 2);


            if (_type == FeedType.Image || _type == FeedType.Video)
            {
                
                byte[] fileBytes = System.IO.File.ReadAllBytes(_url);

                if (_type == FeedType.Video)
                {

                    if (!CheckVideoSize(fileBytes))
                    {
                        AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.MaxVideoSize);
                        yield break;
                    }

                    VPlayer.url = "file://" + _url;
                    VPlayer.Prepare();
                    while (!VPlayer.isPrepared)
                    {
                        yield return null;
                    }
                    VPlayer.Play();
                    while (!VPlayer.isPlaying)
                    {
                        yield return null;
                    }
                    yield return null;

                    NativeCamera.VideoProperties _videoProp = NativeCamera.GetVideoProperties(_url);

                    float videoWidth = NativeGallery.GetVideoProperties(_url).width;
                    float videoHeight = NativeGallery.GetVideoProperties(_url).height;

                    videoBytes = fileBytes;
                    while (VPlayer.frame < 2)
                    {
                        yield return null;
                    }
                    Texture2D _texture = ReadExternalTexture(VPlayer.texture);

                    // rotate image 90 degrees
                    // if is vertical
                    if (videoHeight < videoWidth)
                    {
                        //_texture = rotateTexture(_texture);
                    }

                    if (_videoProp.rotation == 270)
                    {
                        /*for (int i=0;i<2;i++)
                        {
                            _texture = rotateTexture(_texture);
                        }*/
                    }
                        

                    ResizeTexture(_texture);
                    imageBytes = _texture.EncodeToJPG(AppManager.APP_SETTINGS.UploadImageQuality);
                    _feed.MediaWidth = (int)videoWidth;
                    _feed.MeidaHeight = (int)videoHeight;
                    _feed.VideoFileName = fileName;

                    previewTexure = _texture;
                    VPlayer.Stop();
                }

                if (_type == FeedType.Image)
                {
                    Texture2D _texture = new Texture2D(2, 2);
                    _texture = NativeCamera.LoadImageAtPath(_url, -1, false);
                    //_texture.LoadImage(fileBytes);
                    yield return new WaitForEndOfFrame();

                    ResizeTexture(_texture);
                    imageBytes = _texture.EncodeToJPG(AppManager.APP_SETTINGS.UploadImageQuality);
                    _feed.MediaWidth = _texture.width;
                    _feed.MeidaHeight = _texture.height;

                    previewTexure = _texture;
                }
            }

            if (_type == FeedType.Image || _type == FeedType.Video)
            {
                if (_showPreview)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    FeedPreviewRequest _previreRequest = new FeedPreviewRequest();
                    _previreRequest.PreviewImage = previewTexure;
                    AppManager.VIEW_CONTROLLER.ShowFeedPreview(_previreRequest, _type);
                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.HeavyImpact);
                    postBt.interactable = true;
                    while (!_previreRequest.IsComplete)
                    {
                        yield return null;
                    }
                    if (!_previreRequest.IsSuccess)
                    {
                        postBt.interactable = false;
                        yield break;
                    }
                    _feed.BodyTXT = _previreRequest.BodyText;
                }
            }
            else
            {
                _feed.BodyTXT = BodyTextInput.text;
                print(_feed.BodyTXT);
            }

            AppManager.VIEW_CONTROLLER.ShowLoading();

            // wait for preview callback
            if (_type == FeedType.Image)
            {
                // upload image
                FileUploadRequset _imageUploadRequest = new FileUploadRequset();
                _imageUploadRequest.FeedType = _type;
                _imageUploadRequest.FileName = fileName + "." + Utils.GetFileExtension(_url);
                _imageUploadRequest.UploadBytes = imageBytes;

                FileUploadCallback _callBack = new FileUploadCallback();
                AppManager.FIREBASE_CONTROLLER.UploadFile(_imageUploadRequest, callback =>
                {
                    _callBack = callback;
                });
                while (!_callBack.IsComplete)
                {
                    yield return null;
                }
                if (!_callBack.IsSuccess)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.FailedUploadFeed);
                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                    yield break;
                }
                _feed.ImageURL = _callBack.DownloadUrl;
            }
            if (_type == FeedType.Video)
            {
                // upload video
                FileUploadRequset _videoUploadRequest = new FileUploadRequset();
                _videoUploadRequest.FeedType = _type;
                _videoUploadRequest.FileName = fileName + "." + Utils.GetFileExtension(_url);
                _videoUploadRequest.UploadBytes = videoBytes;

                FileUploadCallback _callBack = new FileUploadCallback();
                AppManager.FIREBASE_CONTROLLER.UploadFile(_videoUploadRequest, callback =>
                {
                    _callBack = callback;
                }
                );
                while (!_callBack.IsComplete)
                {
                    yield return null;
                }
                if (!_callBack.IsSuccess)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.FailedUploadFeed);
                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                    yield break;
                }
                // upload video preview
                FileUploadRequset _imageUploadRequest = new FileUploadRequset();
                _imageUploadRequest.FeedType = FeedType.Image;
                _imageUploadRequest.FileName = Guid.NewGuid() + ".jpg";
                _imageUploadRequest.UploadBytes = imageBytes;

                FileUploadCallback _callBack2 = new FileUploadCallback();
                AppManager.FIREBASE_CONTROLLER.UploadFile(_imageUploadRequest, callback =>
                {
                    _callBack2 = callback;
                }
                );
                while (!_callBack2.IsComplete)
                {
                    yield return null;
                }
                if (!_callBack2.IsSuccess)
                {
                    AppManager.VIEW_CONTROLLER.HideLoading();
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.FailedUploadFeed);
                    Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                    yield break;
                }

                //_feed.VideoURL = _callBack.DownloadUrl;
                _feed.ImageURL = _callBack2.DownloadUrl;
            }

            _feed.DateCreated = GetDate();
            FeedUploadCallback _feedCallback = null;
            AppManager.FIREBASE_CONTROLLER.AddNewPost(_feed, callback =>
            {
                _feedCallback = callback;
            });
            while (_feedCallback == null)
            {
                yield return null;
            }
            if (!_feedCallback.IsSuccess)
            {
                AppManager.VIEW_CONTROLLER.HideLoading();
                AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.FailedUploadFeed);
                Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Failure);
                yield break;
            }
            if (_type == FeedType.Video)
            {
                string databasePath = AppSettings.AllPostsKey + "/" + _feed.Key + "/VideoURL";
                AppManager.Instance.Firebase.UploadAndCompressVideo(AppSettings.FeedUploadVideoPath + fileName + "." + Utils.GetFileExtension(_url), databasePath);
            }
            BodyTextInput.text = string.Empty;
            // upload finish
            AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.SuccessPost);
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.Success);
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (!PlayerPrefs.HasKey("review mostrato") && PlayerPrefs.HasKey("accessi"))
                    if (PlayerPrefs.GetInt("accessi") == 5)
                    {
#if UNITY_IOS
                        UnityEngine.iOS.Device.RequestStoreReview();
#endif
                        PlayerPrefs.SetInt("review mostrato", 0);
                    }
            }

            AppManager.VIEW_CONTROLLER.HideLoading();
            
            feedDadaLoader.ResetLoader();

            if (_feed.OwnerID == _feed.ToUserID)
            {
                //AppManager.FIREBASE_CONTROLLER.SharePostWithFriends(AppManager.USER_PROFILE.FIREBASE_USER.UserId, _feed.Key);
            }
        }

        private void ResizeTexture(Texture2D _texture)
        {
            int maxAllowResoulution = (int)AppManager.APP_SETTINGS.MaxAllowFeedImageQuality;
            int _width = _texture.width;
            int _height = _texture.height;
            if (_width > _height)
            {
                if (_width > maxAllowResoulution)
                {
                    float _delta = (float)_width / (float)maxAllowResoulution;
                    _height = Mathf.FloorToInt((float)_height / _delta);
                    _width = maxAllowResoulution;
                }
            }
            else
            {
                if (_height > maxAllowResoulution)
                {
                    float _delta = (float)_height / (float)maxAllowResoulution;
                    _width = Mathf.FloorToInt((float)_width / _delta);
                    _height = maxAllowResoulution;
                }
            }

            TextureScale.Bilinear(_texture, _width, _height);
        }

        private Texture2D ReadExternalTexture(Texture externalTexture)
        {
            Texture2D myTexture2D = new Texture2D(externalTexture.width, externalTexture.height);
            if (myTexture2D == null)
            {
                myTexture2D = new Texture2D(externalTexture.width, externalTexture.height);
            }

            //Make RenderTexture type variable
            RenderTexture tmp = RenderTexture.GetTemporary(
                externalTexture.width,
                externalTexture.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(externalTexture, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;

            myTexture2D.ReadPixels(new UnityEngine.Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return myTexture2D;
        }

        

        private string GetDate()
        {
            return System.DateTime.UtcNow.ToString();
        }

        private FeedType GetFeedType()
        {
            FeedType _type = FeedType.Text;
            if (FeedIndex == 0)
            {
                _type = FeedType.Text;
            }
            if (FeedIndex == 1)
            {
                _type = FeedType.Image;
            }
            if (FeedIndex == 2)
            {
                _type = FeedType.Video;
            }
            return _type;
        }

        public bool CheckVideoSize(byte[] _bytes)
        {
            int _mb = _bytes.Length / 1024 / 1024;
            return _mb <= AppManager.APP_SETTINGS.MaxUploadVideoSizeMB;
        }

        public static Texture2D rotateTexture(Texture2D t)
        {
            Texture2D newTexture = new Texture2D(t.height, t.width, t.format, false);

            for (int i = 0; i < t.width; i++)
            {
                for (int j = 0; j < t.height; j++)
                {
                    newTexture.SetPixel(j, i, t.GetPixel(t.width - i, j));
                }
            }
            newTexture.Apply();
            return newTexture;
        }
    }

    public class FeedPreviewRequest
    {
        public Texture2D PreviewImage;
        public bool IsComplete;
        public bool IsSuccess;
        public string BodyText;
    }

    public class FileUploadRequset
    {
        public byte[] UploadBytes;
        public FeedType FeedType;
        public string FileName;
    }

    public class FileUploadCallback
    {
        public bool IsComplete;
        public bool IsSuccess;
        public string DownloadUrl;
    }

    public class FeedUploadCallback
    {
        public bool IsComplete;
        public bool IsSuccess;
    }
}

