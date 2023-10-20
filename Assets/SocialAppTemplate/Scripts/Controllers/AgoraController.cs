using System.Collections;
using UnityEngine;
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

namespace SocialApp
{
    public class AgoraController : MonoBehaviour
    {
        // Use this for initialization
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        private ArrayList permissionList = new ArrayList();
#endif
#if ENABLE_VIDEO_CALL
        static AgoraApp app = null;
#endif

        public void Init()
        {
#if ENABLE_VIDEO_CALL
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            permissionList.Add(Permission.Microphone);
            permissionList.Add(Permission.Camera);
#endif
            if (ReferenceEquals(app, null))
            {
                app = new AgoraApp(); // create app
                app.loadEngine(AppManager.APP_SETTINGS.AgoraAppID); // load engine
            }
#endif
        }

        void OnApplicationPause(bool paused)
        {
#if ENABLE_VIDEO_CALL
            if (!ReferenceEquals(app, null))
            {
                app.EnableVideo(paused);
            }
#endif
        }

        void OnApplicationQuit()
        {
#if ENABLE_VIDEO_CALL
            if (!ReferenceEquals(app, null))
            {
                app.unloadEngine();
            }
#endif
        }

        void Update()
        {
#if ENABLE_VIDEO_CALL
            CheckPermissions();
#endif
        }
#if ENABLE_VIDEO_CALL
        private void CheckPermissions()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
            foreach (string permission in permissionList)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermission(permission);
                }
            }
#endif
        }
        public void JoinChannel(string _channelID, CallType _callType)
        {
            app.join(_channelID, _callType);
        }
#endif
#if ENABLE_VIDEO_CALL
        public AgoraApp GetActiveApp()
        {
            return app;
        }
#endif
    }
}
