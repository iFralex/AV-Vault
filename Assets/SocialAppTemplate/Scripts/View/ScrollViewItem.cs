using UnityEngine;

namespace SocialApp
{
    public class ScrollViewItem : MonoBehaviour
    {

        public void MoveToEnd()
        {
            gameObject.GetComponent<RectTransform>().SetAsLastSibling();
        }

        public void MoveToStart()
        {
            gameObject.GetComponent<RectTransform>().SetAsFirstSibling();
        }

        public void MoveToPosition(int _index)
        {
            gameObject.GetComponent<RectTransform>().SetSiblingIndex(_index);
        }

        public float GetScrollViewHeight()
        {
            print("altezza " + gameObject.GetComponent<RectTransform>().rect.height);
            return gameObject.GetComponent<RectTransform>().rect.height;
        }
    }
}
