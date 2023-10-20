#if ENABLE_VIDEO_CALL
using agora_gaming_rtc;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SocialApp
{
    public class CallModeController : MonoBehaviour
    {
        public GameObject UserImage;
        public RawImage MyImage;
        public Text USerNameText;
        public Text Timer;

        private IncommingType CurrrentIncomingType;
        private CallObject CurrentCall;
        private bool CallIsActive;

        private int CallStartTime;

        private void Awake()
        {
#if ENABLE_VIDEO_CALL
            var surface = MyImage.gameObject.AddComponent<VideoSurface>();
            surface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
#endif
        }

        public void InitCall(IncommingType _type, CallObject _call)
        {
            CurrrentIncomingType = _type;
            CurrentCall = _call;
            //UserImage.GetComponent<VideoSurface>().SetEnable(false);
            // join to chanel
#if ENABLE_VIDEO_CALL
            AppManager.AGORA_CONTROLLER.JoinChannel(CurrentCall.CallID, _call.CallType);
#endif
            AddListeners();
            UserImage.SetActive(false);
            MyImage.gameObject.SetActive(CurrentCall.CallType == CallType.VIDEO);
            SetUserName();
        }

        private void OnDisable()
        {
            ClearUserImage();
            RemoveListeners();
            CallIsActive = false;
#if ENABLE_VIDEO_CALL
            AppManager.Instance.AgoraController.GetActiveApp().leave();
#endif
            if (UserImage.GetComponent<RawImage>() != null)
            {
                Destroy(UserImage.GetComponent<RawImage>());
            }
        }

        private void AddListeners()
        {
#if ENABLE_VIDEO_CALL
            AppManager.AGORA_CONTROLLER.GetActiveApp().OnUserJoined += OnUserJoined;
            AppManager.AGORA_CONTROLLER.GetActiveApp().OnUserLeave += OnUserLeave;
#endif
        }

        private void RemoveListeners()
        {
#if ENABLE_VIDEO_CALL
            AppManager.AGORA_CONTROLLER.GetActiveApp().OnUserJoined -= OnUserJoined;
            AppManager.AGORA_CONTROLLER.GetActiveApp().OnUserLeave -= OnUserLeave;
#endif
        }

        private void SetUserName()
        {
            AppManager.FIREBASE_CONTROLLER.GetUserFullName(GetTargetID(), (_userName) => { USerNameText.text = _userName; });
        }

        private void OnUserJoined(uint _userID)
        {
            CallStartTime = (int)Time.time;
            CallIsActive = true;
            if (CurrentCall.CallType == CallType.VIDEO)
            {
                UserImage.SetActive(true);
                ClearUserImage();
                UserImage.AddComponent<RawImage>();
#if ENABLE_VIDEO_CALL
                VideoSurface videoSurface = UserImage.gameObject.AddComponent<VideoSurface>();
                if (!ReferenceEquals(videoSurface, null))
                {
                    // configure videoSurface
                    videoSurface.SetForUser(_userID);
                    videoSurface.SetEnable(true);
                    videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
                    videoSurface.SetGameFps(30);
                }
#endif
            }
        }

        private void ClearUserImage()
        {
#if ENABLE_VIDEO_CALL
            if (UserImage.GetComponent<VideoSurface>() != null)
            {
                Destroy(UserImage.GetComponent<VideoSurface>());
            }
#endif
        }

        private void OnUserLeave(uint _userID)
        {
            AppManager.FIREBASE_CONTROLLER.CancelCallOffer(CurrentCall);
        }

        public void OnEndCall()
        {
            AppManager.FIREBASE_CONTROLLER.CancelCallOffer(CurrentCall);
        }

        private string GetTargetID()
        {
            string _id = string.Empty;
            if (CurrrentIncomingType == IncommingType.CALLER) _id = CurrentCall.TargetID;
            if (CurrrentIncomingType == IncommingType.ANSWERS) _id = CurrentCall.UserID;
            return _id;
        }

        private void Update()
        {
            if (CallIsActive)
            {
                int timer = (int)Time.time - CallStartTime;
                string minutes = Mathf.Floor(timer / 60).ToString("00");
                string seconds = (timer % 60).ToString("00");
                Timer.text = minutes + ":" + seconds;
            }
        }
    }
}
