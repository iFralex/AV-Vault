using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SocialApp
{
    public class IncomingCallViewController : MonoBehaviour
    {
        [SerializeField]
        private AvatarViewController AvatarController = default;
        [SerializeField]
        private Text UserNameLabel = default;
        [SerializeField]
        private GameObject AnswerBtn = default;
        [SerializeField]
        private GameObject CancellBtn = default;
        [SerializeField]
        private GameObject DeclineBtn = default;
        [SerializeField]
        private Text TitleLabel = default;

        private IncommingType CurrrentIncomingType = IncommingType.ANSWERS;
        private CallObject CurrentCall = default;


        public void SetupWindow(IncommingType _type, CallObject _call)
        {
            CurrrentIncomingType = _type;
            CurrentCall = _call;
            SetupUI();
            StartCoroutine(CancelCor());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void SetupUI()
        {
            if (CurrrentIncomingType == IncommingType.CALLER)
            {
                CancellBtn.SetActive(true);
                AnswerBtn.SetActive(false);
                DeclineBtn.SetActive(false);

                AvatarController.LoadBigAvatar(CurrentCall.TargetID);
                AppManager.FIREBASE_CONTROLLER.GetUserFullName(CurrentCall.TargetID, (_userName) => { UserNameLabel.text = _userName;});
            }
            else if (CurrrentIncomingType == IncommingType.ANSWERS)
            {
                CancellBtn.SetActive(false);
                AnswerBtn.SetActive(true);
                DeclineBtn.SetActive(true);

                AvatarController.LoadBigAvatar(CurrentCall.UserID);
                AppManager.FIREBASE_CONTROLLER.GetUserFullName(CurrentCall.UserID, (_userName) => { UserNameLabel.text = _userName; });
            }
            if (CurrentCall.CallType == CallType.AUDIO) TitleLabel.text = "Audio Call";
            if (CurrentCall.CallType == CallType.VIDEO) TitleLabel.text = "Video Call";
        }

        public void OnAnswer()
        {
            AppManager.FIREBASE_CONTROLLER.AnswerCallOffer(CurrentCall);
        }

        public void OnDecline()
        {
            AppManager.FIREBASE_CONTROLLER.CancelCallOffer(CurrentCall);
        }

        public void OnCancel()
        {
            AppManager.FIREBASE_CONTROLLER.CancelCallOffer(CurrentCall);
        }

        private IEnumerator CancelCor()
        {
            yield return new WaitForSeconds(AppSettings.IncomingCallMaxTime);
            OnCancel();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                OnAnswer();
            }
        }
    }

    public enum IncommingType
    {
        CALLER,
        ANSWERS
    }
}