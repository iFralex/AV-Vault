using UnityEngine;
using Firebase.Database;

namespace SocialApp
{
    public class CallController : MonoBehaviour
    {
        public GameObject IncomingWindows;
        public GameObject CallModeWindows;

        private DatabaseReference ActiveStateReference;
        private DatabaseReference BisyStateReference;
        private DatabaseReference AnswerStateReference;
        private IncommingType CurrrentIncomingType;
        private CallObject CurrentCall;

        private int CurrentSleepMode;

        private void OnEnable()
        {
            CurrentSleepMode = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void OnDisable()
        {
            Screen.sleepTimeout = CurrentSleepMode;
            RemoveListeners();
        }

        private void AddListeners()
        {
            ActiveStateReference = AppManager.FIREBASE_CONTROLLER.GetCallActiveStateReference(CurrentCall);
            BisyStateReference = AppManager.FIREBASE_CONTROLLER.GetCallBisyStateReference(CurrentCall);
            AnswerStateReference = AppManager.FIREBASE_CONTROLLER.GetCallAnswerStateReference(CurrentCall);
            ActiveStateReference.ValueChanged += ActiveStateChanged;
            BisyStateReference.ValueChanged += BisyStateChanged;
            AnswerStateReference.ValueChanged += AnswerStateChanged;
        }

        private void ActiveStateChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.DatabaseError == null && e.Snapshot != null && e.Snapshot.Exists)
            {
                bool newVal = (bool)e.Snapshot.Value;
                if (!newVal)
                {
                    //AppManager.VIEW_CONTROLLER.HideCall();
                }
            }
        }

        private void BisyStateChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.DatabaseError == null && e.Snapshot != null && e.Snapshot.Exists)
            {
                bool newVal = (bool)e.Snapshot.Value;
                if (newVal)
                {
                    AppManager.VIEW_CONTROLLER.ShowPopupMSG(MessageCode.CallIsBisy);
                    //AppManager.VIEW_CONTROLLER.HideCall();
                }
            }
        }

        private void AnswerStateChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.DatabaseError == null && e.Snapshot != null && e.Snapshot.Exists)
            {
                bool newVal = (bool)e.Snapshot.Value;
                if (newVal)
                {
                    CallModeWindows.SetActive(true);
                    CallModeWindows.GetComponent<CallModeController>().InitCall(CurrrentIncomingType, CurrentCall);
                    HideIncoming();
                }
            }
        }

        private void RemoveListeners()
        {
            if (ActiveStateReference != null) ActiveStateReference.ValueChanged -= ActiveStateChanged;
            if (BisyStateReference != null) BisyStateReference.ValueChanged -= BisyStateChanged;
            if (AnswerStateReference != null) AnswerStateReference.ValueChanged -= AnswerStateChanged;
        }

        public void ShowIncomming(IncommingType _type, CallObject _call)
        {
            CurrrentIncomingType = _type;
            CurrentCall = _call;
            IncomingWindows.SetActive(true);
            CallModeWindows.SetActive(false);
            IncomingWindows.GetComponent<IncomingCallViewController>().SetupWindow(_type, _call);
            AddListeners();
        }

        private void HideIncoming()
        {
            IncomingWindows.SetActive(false);
        }
    }

    [System.Serializable]
    public class CallObject
    {
        public string CallID;
        public string UserID;
        public string TargetID;
        public string CreateTimeStamp;
        public string DataKey;
        public CallType CallType;

        public bool IsActive = true;
        public bool IsBisy;
        public bool HasAnswer;
    }

    public enum CallType
    {
        VIDEO,
        AUDIO
    }
}
