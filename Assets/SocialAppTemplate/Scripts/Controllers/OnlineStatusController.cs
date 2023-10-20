using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SocialApp
{
    public class OnlineStatusController : MonoBehaviour
    {
        [SerializeField]
        private float UpdateInterval = default;
        [SerializeField]
        private bool CheckOnce = default;

        private Action UpdateAction;
        private string UserID;

        private long ServerTime;
        private long UserLastActivity;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void SetUser(string _userID)
        {
            UserID = _userID;
        }

        public void SetUpdateAction(Action _updateAction)
        {
            UpdateAction = _updateAction;
        }

        public void StartCheck()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(OnCheck());
            }
        }

        private IEnumerator OnCheck()
        {
            while (true)
            {
                AppManager.FIREBASE_CONTROLLER.GetServerTimestamp(_msg =>
                {
                    string _time = _msg.Data;
                    long timeStamp;
                    bool isInteger = long.TryParse(_time, out timeStamp);
                    if (isInteger)
                    {
                        AppManager.FIREBASE_CONTROLLER.GetUserLastActivity(UserID, _msg2 =>
                        {
                            string _userActivity = _msg2.Data;
                            long userActivity;
                            bool IsSucces = long.TryParse(_userActivity, out userActivity);
                            if (IsSucces)
                            {
                                ServerTime = timeStamp;
                                UserLastActivity = userActivity;
                                if (UpdateAction != null)
                                {
                                    UpdateAction.Invoke();
                                }
                                if (CheckOnce)
                                {
                                    StopAllCoroutines();
                                }
                            }
                        });
                    }
                });
                yield return new WaitForSeconds(UpdateInterval);
            }
        }

        public bool IsOnline()
        {
            return PassedMilisecondsFromLastActivity() < (long)AppManager.APP_SETTINGS.ValidOnlineTimeMinute * 60 * 1000;
        }

        public long PassedMilisecondsFromLastActivity()
        {
            return ServerTime - UserLastActivity;
        }

        public string GetStringLastActivity()
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(PassedMilisecondsFromLastActivity());
            string _date = string.Empty;

            if (IsOnline())
            {
                _date = "Online";
            }
            else if (timeSpan.Days > 0)
            {
                _date = "Active " + timeSpan.Days + " days ago";
            }
            else if (timeSpan.Hours > 0)
            {
                _date = "Active " + timeSpan.Hours + " hours ago";
            }
            else if (timeSpan.Minutes > 0)
            {
                _date = "Active " + timeSpan.Minutes + " minutes ago";
            }
            else if (timeSpan.Seconds > 0)
            {
                _date = "Active " + timeSpan.Seconds + " seconds ago";
            }
            return _date;
        }
    }
}
