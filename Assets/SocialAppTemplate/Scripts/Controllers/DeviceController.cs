using System.Collections;
using UnityEngine;

namespace SocialApp
{

    public class DeviceController : MonoBehaviour
    {

        public string GetSystemDate()
        {
            return System.DateTime.UtcNow.ToString(new System.Globalization.CultureInfo("en-US"));
        }

        public void UnloadAssets()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        /*
        public void StartOnlineChecker()
        {
            StartCoroutine(OnStartOnlineChecker());
        }*/

        private IEnumerator OnStartOnlineChecker()
        {
            while (true)
            {
                AppManager.FIREBASE_CONTROLLER.UpdateUserActivity();
                yield return new WaitForSeconds(AppManager.APP_SETTINGS.UpdateActivityInterval);
            }
        }
        /*
        public void StopOnlineChecker()
        {
            StopAllCoroutines();
        }*/
    }
}