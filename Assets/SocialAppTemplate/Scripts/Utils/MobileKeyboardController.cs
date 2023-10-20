using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocialApp;

namespace SocialApp
{
    public class MobileKeyboardController : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField Input = default;
        [SerializeField]
        private RectTransform Rect = default;
        [SerializeField]
        private float TargetHeight = default;
        [SerializeField]
        private float InputHeight = default;

        private float StartHeight;

        private Vector2 StartAnchoredPosition;


        private void Start()
        {
            StartHeight = Rect.anchoredPosition.y;
            StartAnchoredPosition = Rect.anchoredPosition;
        }
        // Update is called once per frame
        void LateUpdate()
        {
            if (Input.isFocused)
            {
                Rect.anchoredPosition = new Vector2(0, TargetHeight * GetKeybordHeight() + InputHeight + StartHeight);
            }
            else
            {
                Rect.anchoredPosition = StartAnchoredPosition;
            }
        }

        private float GetKeybordHeight()
        {
            if (Application.isEditor)
            {
                return 0f; // fake TouchScreenKeyboard height ratio for debug in editor        
            }
#if UNITY_ANDROID
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    View.Call("getWindowVisibleDisplayFrame", rect);
                    return (float)(Screen.height - rect.Call<int>("height")) / Screen.height;
                }
            }
#else
        return (float)TouchScreenKeyboard.area.height / Screen.height;
#endif
        }
    }
}
