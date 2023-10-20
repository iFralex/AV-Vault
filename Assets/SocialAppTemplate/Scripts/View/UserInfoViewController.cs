using System;
using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEngine.UI;

namespace SocialApp
{
    public class UserInfoViewController : MonoBehaviour
    {
        public GameObject loading;
        public GameObject elementiPan;
        public AvatarViewController avatar;
        public Text userNameT;
        public Text followersT;
        public Text memberByT;
        public Text postCountT;
        public Button viewPostsBt;
        public Button followBt;
        public Text followBtT;
        public GameObject followBtLoading;
        User user;

        public void ShowUserInfo(string _id)
        {
            int count = 0, tot = 5, followersC = 0;
            bool follow;
            void Loaded()
            {
                loading.SetActive(false);
                elementiPan.SetActive(true);
                avatar.LoadBigAvatar(user.UserID);
                userNameT.text = user.FullName;
                LoadDateRegistration();
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.RigidImpact);
            }
            void LoadingFollowBt(bool b)
            {
                followBtLoading.SetActive(!b);
                followBt.interactable = b;
                followBtT.gameObject.SetActive(b);
            }
            void FollowButton(bool f)
            {
                followBt.colors = !f ? new ColorBlock() { colorMultiplier = 1, fadeDuration = .2f, normalColor = Color.blue, highlightedColor = Color.blue, pressedColor = new Color(0, 0, .4f, 1), selectedColor = Color.blue, disabledColor = new Color(0, 0, .3f, .5f) } : new ColorBlock() { colorMultiplier = 1, fadeDuration = .2f, normalColor = Color.black, highlightedColor = Color.black, pressedColor = new Color(0, 0, .4f, 1), selectedColor = Color.black, disabledColor = new Color(0, 0, .3f, .5f) };
                followBtT.text = !f ? "Follow" : "Following";
            }
            ClearData();
            gameObject.SetActive(true);
            StartCoroutine(TransizioneVersoAlto(elementiPan.transform.parent.gameObject, elementiPan.transform.parent.parent.gameObject, 0, 1, true));
            AppManager.FIREBASE_CONTROLLER.GetUserData(_id, _user =>
            {
                if (_user == null)
                {
                    AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = "Error", Message = "Unexpected error.\nImpossible to open the user info panel." });
                    gameObject.SetActive(false);
                    return;
                }
                print(AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                user = _user;
                followBt.gameObject.SetActive(user.UserID != AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                viewPostsBt.gameObject.SetActive(user.UserID != AppManager.USER_PROFILE.FIREBASE_USER.UserId);
                if (!(AppManager.USER_PROFILE.FIREBASE_USER == null || AppSettings.guest))
                {
                    AppManager.FIREBASE_CONTROLLER.CheckYouAreAFollower(user.UserID, f =>
                    {
                        follow = f;
                        LoadingFollowBt(true);
                        FollowButton(f);
                        followBt.onClick.RemoveAllListeners();
                        followBt.onClick.AddListener(() =>
                        {
                            HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
                            LoadingFollowBt(false);
                            if (!follow)
                                AppManager.FIREBASE_CONTROLLER.FollowUser(user.UserID, _b =>
                                {
                                    follow = true;
                                    if (_b)
                                    {
                                        followersC++;
                                        followersT.text = followersC.ToString();
                                        avatar.SetFollowerBorder(true);
                                        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
                                    }
                                    else
                                        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
                                    LoadingFollowBt(true);
                                    FollowButton(_b);
                                    AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = _b ? "Good!" : "Error", Message = _b ? "You are following " + user.FullName + " now." : "unexpected error.\nTry again." });
                                });
                            else
                                AppManager.FIREBASE_CONTROLLER.StopFollowUser(user.UserID, _b =>
                                {
                                    follow = false;
                                    if (_b)
                                    {
                                        followersC--;
                                        followersT.text = followersC.ToString();
                                        avatar.SetFollowerBorder(false);
                                        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
                                    }
                                    else
                                        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
                                    LoadingFollowBt(true);
                                    FollowButton(!_b);
                                    //AppManager.VIEW_CONTROLLER.ShowPopupMessage(new PopupMessage() { Title = _b ? "Good!" : "Error", Message = b ? "You are following " + user.FullName + " now." : "unexpected error.\nTry again." });
                                });
                        });
                        count++;
                        if (count == tot)
                            Loaded();
                    });
                }
                else
                {
                    followBt.gameObject.SetActive(false);
                    count++;
                }
                AppManager.FIREBASE_CONTROLLER.CheckUserIsVaultPlus(user.UserID, b =>
                {
                    if (b)
                        elementiPan.transform.parent.GetComponent<Image>().color = new Color(.8f, .8f, 0, 1);
                    count++;
                    if (count == tot)
                        Loaded();
                });
                AppManager.FIREBASE_CONTROLLER.UserCountFollowers(user.UserID, b =>
                {
                    followersC = b;
                    followersT.text = b.ToString();
                    count++;
                    if (count == tot)
                        Loaded();
                });
                viewPostsBt.onClick.RemoveAllListeners();
                viewPostsBt.onClick.AddListener(() =>
                {
                    AppManager.VIEW_CONTROLLER.ShowUserPosts(user.UserID);
                    gameObject.SetActive(false);
                    HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
                });
                count++;
                if (count == tot)
                    Loaded();
            });
            AppManager.FIREBASE_CONTROLLER.GetUserPostsCount(_id, _count =>
            {
                postCountT.text = _count.ToString();
                count++;
                if (count == tot)
                    Loaded();
            });
        }

        private void LoadDateRegistration()
        {
            string _date = user.DataRegistration;
            //if (_date[0] + _date[1] + _date[2] + "" != "148")
                //return;
            DateTime date = DateTime.Parse(_date);
            int tempo = (int)(DateTime.UtcNow - date).TotalDays;
            string n = "D";
            if (tempo > 30)
            {
                if (tempo < 365)
                {
                    n = "M";
                    tempo = (int)Mathf.Round(tempo / 30f);
                }
                else
                {
                    n = "Y";
                    tempo = (int)Mathf.Round(tempo / 365f);
                }
            }
            memberByT.text = tempo + n;
        }

        void ClearData()
        {
            loading.SetActive(true);
            elementiPan.SetActive(false);
            elementiPan.transform.parent.GetComponent<Image>().color = new Color(.13f, .13f, .13f, 1);
            followBtT.gameObject.SetActive(false);
            followBt.interactable = false;
            followBtLoading.SetActive(true);
            user = null;
        }

        IEnumerator TransizioneVersoAlto(GameObject a, GameObject b, float inizio, int verso, bool aper)
        {
            for (float i = inizio; verso == 1 ? i <= 1.1f : i >= -0.1f; i += .05f * verso)
            {
                StatoTransizioneScendi(i, a, b);
                yield return new WaitForSeconds(.005f);
            }
            gameObject.SetActive(aper);
            if (inizio < .98f)
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
        }

        void StatoTransizioneScendi(float i, GameObject a, GameObject b)
        {
            b.GetComponent<Image>().color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, i));
            a.GetComponent<RectTransform>().anchoredPosition = new Vector2(a.GetComponent<RectTransform>().anchoredPosition.x, Mathf.Lerp(-1240, -142.8671f, i));
        }
        bool cambiaPost;
        float tempo = 0;
        Vector2 direz, posIniz;

        void Update()
        {
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0))//Input.touchCount > 0)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    tempo = Time.time;
                    cambiaPost = true;
                    posIniz = Input.mousePosition;
                    direz = Vector2.zero;
                }

                if (cambiaPost)
                {
                    if (Input.GetKey(KeyCode.Mouse0))
                    {
                        if (Vector2.Distance(Input.mousePosition, posIniz) > 100)
                        {
                            if (direz == Vector2.zero)
                            {
                                Vector2 vet = (Vector2)Input.mousePosition - posIniz;
                                if (Mathf.Abs(vet.x) < Mathf.Abs(vet.y))
                                    direz = new Vector2(0, vet.y > 0 ? 1 : -1);
                            }
                        }
                        else
                            return;
                        StatoTransizioneScendi(((Input.mousePosition.y - posIniz.y) / (Screen.width * 16f / 9f)) + 1f, elementiPan.transform.parent.gameObject, elementiPan.transform.parent.parent.gameObject);
                    }
                    else if (Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        float vel = (Input.mousePosition.y - posIniz.y) / (Time.time - tempo);
                        
                        if ((elementiPan.transform.parent.GetComponent<RectTransform>().anchoredPosition.y < -678 || Mathf.Abs(vel) > 1500) && Mathf.Abs(Input.mousePosition.y - posIniz.y) > Screen.width * 16 / 9f / 15f)
                        {
                            if (vel > 0)
                                StartCoroutine(TransizioneVersoAlto(elementiPan.transform.parent.gameObject, elementiPan.transform.parent.parent.gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), 1, true));
                            else
                                StartCoroutine(TransizioneVersoAlto(elementiPan.transform.parent.gameObject, elementiPan.transform.parent.parent.gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), -1, false));
                        }
                        else
                            StartCoroutine(TransizioneVersoAlto(elementiPan.transform.parent.gameObject, elementiPan.transform.parent.parent.gameObject, 1f + (Input.mousePosition.y - posIniz.y) / (Screen.width * 16 / 9f), 1, true));
                        cambiaPost = false;
                    }
                }
            }
        }
    }
}