using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialApp;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace SocialApp
{
    public class AppSettings : ScriptableObject
    {
        public static bool VaultPLUS = false;
        public static int GiorniRimastiVaultPLUS;
        public static FasiTutorial TutorialMode = FasiTutorial.Nulla;
        public static bool mostraCondividi;
        public static bool audioMuted = true;
        // Registration
        public int MinAllowPasswordCharacters;
        public string EmailValidationCharacter;
        public bool UseEmailConfirm = true;
        // Scriptable
        public static string AppSettingPath = "Config/AppSettings";
        public static bool guest = false;
        // Login
        public static string LoginSaveKey = "UserLogin";
        public static string PasswordSaveKey = "UserPassword";
        // Database
        public static string RootUserKey = "Users";
        public static string RootUserDeletedKey = "DeletedUsers";
        public static string UserActivityKey = "LastActivity";
        public static string RootUserStorageKey = "UsersData";
        public static string UserAvatarKey = "Avatar";
        public static string UserCustomBackgroundKey = "CustomBackground";
        public static string UserPostsKey = "UserPosts";
        public static string FriendsPostsKey = "FriendsPosts";
        public static string UserFriendsKey = "UserFriends";
        public static string UserRequestedFriendsKey = "UserRequestedFriends";
        public static string UserPendingFriendsKey = "UserPendingFriends";
        public static string UserMessagesKey = "UserMessages";
        public static string UserMessagesList = "UserMessagesList";
        public static string UnreadMessagesKey = "UnreadMessages";
        public static string AllPostsKey = "AllPosts";
        public static string PremiumPosts = "PremiumPosts";
        public static string SubscriptionsVaultPLUSKey = "SubscriptionsVaultPLUS";
        public static string PostInteractionKey = "PostInteractions";
        public static string PostLikesKey = "PostLikes";
        public static string PostDislikesKey = "PostDislikes";
        public static string PostCommentsKey = "PostComments";
        public static string ContainerListKey = "List";
        public static string ListCountKey = "Count";
        public static string ActivityKey = "LastActivity";
        public static string TypingMSGKey = "Typing";
        public static string UserFullNameKey = "FullName";
        public static string UserDateRegistrationKey = "DataRegistration";
        public static string UserPhoneKey = "Phone";
        public static string UserFirstNameKey = "FirstName";
        public static string UserLastNameKey = "LastName";
        public static string DeviceTokensKey = "DeviceTokens";
        public static string UserMessagesGroups = "UserMessagesGroups";
        public static string UserCallList = "UserCallList";
        public static string CallActiveKey = "IsActive";
        public static string CallBisyKey = "IsBisy";
        public static string CallHasAnswerKey = "HasAnswer";
        public static string ReportedPosts = "ReportedPosts";
        public static string memeCondivisoKey = "";
        // Storage
        public static string FeedUploadVideoPath = "Feeds/Video/";
        public static string FeedUploadImagePath = "Feeds/Image/";
        public static string ConvertedVideoOutPath = "_output.mp4";
        // View
        public static float DefaultGroupChatIconSize = 60f;

        public static string PROJECT_DEFINE_KEY = "SAT_ENABLED";
        private static string VideoCallDefineKey = "ENABLE_VIDEO_CALL";
        public static int IncomingCallMaxTime = 30;

        // Device
        public string SystemDateFormat;
        public int UpdateActivityInterval;
        // Profile
        public int ValidOnlineTimeMinute = 5;
        // Images
        [Range(1, 100)]
        public int UploadImageQuality = 50;
        public ImageSize MaxAllowFeedImageQuality;
        // Videos
        public bool UseOriginVideoFile;
        public int MaxUploadVideoSizeMB;
        // Avatars
        public Texture2D DefaultAvatarTexture;
        // Groups
        public Texture2D DefaultGroupChatTexture;
        // Video/Audio calls
        [SerializeField]
        private bool EnableVideoAudioCalls;

        public bool _EnableVideoAudioCalls
        {
            get
            {
                return EnableVideoAudioCalls;
            }
            set
            {
                EnableVideoAudioCalls = value;
            }
        }

        // Agora App ID
        public string AgoraAppID;
#if UNITY_EDITOR
        void OnValidate()
        {
            if (EnableVideoAudioCalls)
            {
                AddCompileDefine(VideoCallDefineKey, new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                RemoveCompileDefine(VideoCallDefineKey, new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        public static void PrepaeDefinesForUpload()
        {
            RemoveCompileDefine(VideoCallDefineKey, new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.Android });
            AddCompileDefine(PROJECT_DEFINE_KEY, new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.Android });
        }

        public static void AddCompileDefine(string newDefineCompileConstant, BuildTargetGroup[] targetGroups = null)
        {
            if (targetGroups == null)
                targetGroups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));

            foreach (BuildTargetGroup grp in targetGroups)
            {
                if (grp == BuildTargetGroup.Unknown)        //the unknown group does not have any constants location
                    continue;

                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                if (!defines.Contains(newDefineCompileConstant))
                {
                    if (defines.Length > 0)         //if the list is empty, we don't need to append a semicolon first
                        defines += ";";

                    defines += newDefineCompileConstant;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
                }
            }
        }

        public static void RemoveCompileDefine(string defineCompileConstant, BuildTargetGroup[] targetGroups = null)
        {
            if (targetGroups == null)
                targetGroups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));

            foreach (BuildTargetGroup grp in targetGroups)
            {
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                int index = defines.IndexOf(defineCompileConstant);
                if (index < 0)
                    continue;           //this target does not contain the define
                else if (index > 0)
                    index -= 1;         //include the semicolon before the define
                                        //else we will remove the semicolon after the define

                //Remove the word and it's semicolon, or just the word (if listed last in defines)
                int lengthToRemove = Math.Min(defineCompileConstant.Length + 1, defines.Length - index);

                //remove the constant and it's associated semicolon (if necessary)
                defines = defines.Remove(index, lengthToRemove);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, defines);
            }
        }
#endif
    }
    public enum ImageSize
    {
        Origin = 0,
        Size_1024 = 1024,
        Size_512 = 512,
        Size_256 = 256,
        Size_128 = 128
    }
}

public enum FasiTutorial
{
    ApriPublicaPost,
    ChiudiPublicaPost,
    ApriMieiPosts,
    ChiudiMieiPosts,
    ApriPostPremium,
    ApriPostWorld,
    Attendi,
    ScorriADestraPost,
    ScorriASinistraPost,
    ApriCommenti,
    ChiudiCommenti,
    Nulla
}