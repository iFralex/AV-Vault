using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialApp
{
    public class Utils
    {
        public static string GetFileExtension(string _url)
        {
            string path = _url;
            string[] splitsPath = path.Split('/');
            string[] splitsLast = splitsPath[splitsPath.Length - 1].Split('.');
            return splitsLast[splitsLast.Length - 1];
        }
    }
}
