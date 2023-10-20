#if ENABLE_VIDEO_CALL
using UnityEngine;
using UnityEngine.UI;

using agora_gaming_rtc;
using agora_utilities;
using System;


namespace SocialApp
{
    public class AgoraApp
    {

        // instance of agora engine
        private IRtcEngine mRtcEngine;

        public event Action<uint> OnUserJoined;
        public event Action<uint> OnUserLeave;

        private uint UserID;

        // load agora engine
        public void loadEngine(string appId)
        {
            // start sdk

            if (mRtcEngine != null)
            {
                Debug.Log("Engine exists. Please unload it first!");
                return;
            }

            // init engine
            mRtcEngine = IRtcEngine.GetEngine(appId);

            // enable log
            mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
        }

        private void OnUserRegistered(uint uid, string userAccount)
        {
            UserID = uid;
        }

        public void join(string channel, CallType _type)
        {
            if (mRtcEngine == null)
                return;

            // set callbacks (optional)
            mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
            mRtcEngine.OnUserJoined = onUserJoined;
            mRtcEngine.OnUserOffline = onUserOffline;
            mRtcEngine.OnLocalUserRegistered = OnUserRegistered;

            if (_type == CallType.VIDEO)
            {
                // enable video
                mRtcEngine.EnableVideo();
                // allow camera output callback
                mRtcEngine.EnableVideoObserver();
            }

            // join channel
            mRtcEngine.JoinChannel(channel, null, 0);

            // Optional: if a data stream is required, here is a good place to create it
            int streamID = mRtcEngine.CreateDataStream(true, true);
            Debug.Log("initializeEngine done, data stream id = " + streamID);
        }

        public string getSdkVersion()
        {
            string ver = IRtcEngine.GetSdkVersion();
            return ver;
        }

        public void leave()
        {
            if (mRtcEngine == null)
                return;

            // leave channel
            mRtcEngine.LeaveChannel();
            // deregister video frame observers in native-c code
            mRtcEngine.DisableVideoObserver();
        }

        // unload agora engine
        public void unloadEngine()
        {

            // delete
            if (mRtcEngine != null)
            {
                IRtcEngine.Destroy();  // Place this call in ApplicationQuit
                mRtcEngine = null;
            }
        }


        public void EnableVideo(bool pauseVideo)
        {
            if (mRtcEngine != null)
            {
                if (!pauseVideo)
                {
                    mRtcEngine.EnableVideo();
                }
                else
                {
                    mRtcEngine.DisableVideo();
                }
            }
        }

        // accessing GameObject in Scnene1
        // set video transform delegate for statically created GameObject
        public void onSceneHelloVideoLoaded()
        {
            // Attach the SDK Script VideoSurface for video rendering
            GameObject quad = GameObject.Find("Quad");
            if (ReferenceEquals(quad, null))
            {
                Debug.Log("BBBB: failed to find Quad");
                return;
            }
            else
            {
                quad.AddComponent<VideoSurface>();
            }

            GameObject cube = GameObject.Find("Cube");
            if (ReferenceEquals(cube, null))
            {
                Debug.Log("BBBB: failed to find Cube");
                return;
            }
            else
            {
                cube.AddComponent<VideoSurface>();
            }
        }

        // implement engine callbacks
        private void onJoinChannelSuccess(string channelName, uint uid, int elapsed)
        {
            Debug.Log("JoinChannelSuccessHandler: uid = " + uid);
        }

        // When a remote user joined, this delegate will be called. Typically
        // create a GameObject to render video on it
        private void onUserJoined(uint uid, int elapsed)
        {
            OnUserJoined?.Invoke(uid);
        }

        public VideoSurface makePlaneSurface(string goName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }
            go.name = goName;
            // set up transform
            go.transform.Rotate(-90.0f, 0.0f, 0.0f);
            float yPos = UnityEngine.Random.Range(3.0f, 5.0f);
            float xPos = UnityEngine.Random.Range(-2.0f, 2.0f);
            go.transform.position = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        private const float Offset = 100;
        public VideoSurface makeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;

            // to be renderered onto
            go.AddComponent<RawImage>();

            // make the object draggable
            go.AddComponent<UIElementDragger>();
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                go.transform.parent = canvas.transform;
            }
            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            float xPos = UnityEngine.Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            float yPos = UnityEngine.Random.Range(Offset, Screen.height / 2f - Offset);
            go.transform.localPosition = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(3f, 4f, 1f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }
        // When remote user is offline, this delegate will be called. Typically
        // delete the GameObject for this user
        private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
        {
            OnUserLeave?.Invoke(uid);
            // remove video stream
            Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
            // this is called in main thread
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                UnityEngine.Object.Destroy(go);
            }
        }
    }
}
#endif