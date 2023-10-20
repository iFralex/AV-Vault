using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using SocialApp;

namespace SocialApp
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private TextMeshProUGUI pTextMeshPro = default;

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, null);
            if (linkIndex != -1)
            { // was a link clicked?
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

                // open the link id as a url, which is the metadata we added in the text field
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }

        public void CheckLinks()
        {
            string _txt = pTextMeshPro.text;
            if (string.IsNullOrEmpty(pTextMeshPro.text))
                return;
            Regex regx = new Regex("((http://|https://|www\\.)([A-Z0-9.-:]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection matches = regx.Matches(_txt);
            foreach (Match match in matches)
                pTextMeshPro.text = pTextMeshPro.text.Replace(match.Value, ShortLink(match.Value));
        }

        string ShortLink(string link)
        {
            string text = link;
            int left = 9;
            int right = 16;
            string cut = "...";
            if (link.Length > (left + right + cut.Length))
                text = string.Format("{0}{1}{2}", link.Substring(0, left), cut, link.Substring(link.Length - right, right));
            return string.Format("<#7f7fe5><u><link=\"{0}\">{1}</link></u></color>", link, text);
        }
    }
}
