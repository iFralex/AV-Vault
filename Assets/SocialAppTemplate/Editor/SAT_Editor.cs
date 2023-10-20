using UnityEditor;
using UnityEngine;
using System;

namespace SocialApp
{
    public class SAT_Editor
    {
        

        [MenuItem("SAT/Open Settings")]
        static void OpenSetting()
        {
            Selection.activeObject = Resources.Load("Config/AppSettings");
        }

        [MenuItem("SAT/Setup Android Project")]
        static void SetupAndroidProject()
        {
            AppSettings.AddCompileDefine(AppSettings.PROJECT_DEFINE_KEY, new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.Android });
            if (System.IO.Directory.Exists(Application.dataPath + "/SocialAppTemplate/Plugins/Android/sat.androidlib"))
            {
                if (!System.IO.Directory.Exists(Application.dataPath + "/Plugins/Android"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Plugins/Android");
                }
                FileUtil.MoveFileOrDirectory(Application.dataPath + "/SocialAppTemplate/Plugins/Android/sat.androidlib", Application.dataPath + "/Plugins/Android/sat.androidlib");
                AssetDatabase.Refresh();
            }
            EditorApplication.ExecuteMenuItem("Assets/External Dependency Manager/Android Resolver/Force Resolve");
        }

        [MenuItem("SAT/Setup IOS Project")]
        static void SetupIOSProject()
        {
            AppSettings.AddCompileDefine(AppSettings.PROJECT_DEFINE_KEY, new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.Android });
            EditorApplication.ExecuteMenuItem("Assets/External Dependency Manager/iOS Resolver/Install Cocoapods");
        }

        [MenuItem("SAT/Documentation")]
        static void OpenDocumentation()
        {
            Application.OpenURL("https://archbee.io/public/sXzjtP0LpJhdYCvnQBuHU");
        }

        [MenuItem("SAT/Add Firebase SDK")]
        static void AddFirebaseSDK()
        {
            string _filePath = Application.dataPath + "/../" + "Packages/manifest.json";
            if (System.IO.File.Exists(_filePath))
            {
                string _json = System.IO.File.ReadAllText(_filePath);
                string _googleFirebase = "";
                string _firebsepackages = "";
                TextAsset googleRegister = Resources.Load("GoogleRegistries") as TextAsset;
                TextAsset fPackages = Resources.Load("FirebasePackages") as TextAsset;
                _googleFirebase = googleRegister.text;
                _firebsepackages = Environment.NewLine+fPackages.text;
                if (_json.Contains(_googleFirebase))
                {
                    Debug.Log("Already have dependencies");
                }
                else
                {
                    string insertDependiesAfterKey = "}";
                    string insertFirebaseAfterKey = "{";
                    int insertAfterCount = 2;
                    int inserCounter = 0;
                    for (int i = 0; i < _json.Length; i++)
                    {
                        if (_json[i].ToString() == insertDependiesAfterKey && !_json.Contains(_googleFirebase))
                        {
                            string _newString = _json.Insert(i + 1, _googleFirebase);
                            System.IO.File.WriteAllText(_filePath, _newString);
                            _json = _newString;
                        }
                        if (_json[i].ToString() == insertFirebaseAfterKey)
                        {
                            inserCounter++;
                            if (inserCounter == insertAfterCount)
                            {
                                string _newString = _json.Insert(i + 1, _firebsepackages);
                                System.IO.File.WriteAllText(_filePath, _newString);
                                _json = _newString;
                            }
                        }
                    }
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
