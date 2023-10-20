using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SocialApp
{
    public class ScrollViewController : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {

        [SerializeField]
        private List<ScrollViewItem> ScrollItemList = new List<ScrollViewItem>();
        [SerializeField]
        private ScrollRect Scroll = default;
        [SerializeField]
        private RectTransform ContentRect = default;
        [SerializeField]
        private VerticalLayoutGroup ContentVerticalGroup = default;
        [SerializeField]
        private GameObject DataLoaderObject = default;
        [SerializeField]
        private float ScrollSpeed = default;
        [SerializeField]
        private int TopIndex = 1;
        [SerializeField]
        private ContentDirection Direction = default;

        private int LoadIndex = 0;

        private int loadIndexInList = 0;

        private float LastScrollPosition = 1f;

        private bool IsBlocked = false;

        [SerializeField]
        private bool AutoLoadOnStartDrag = true;
        [SerializeField]
        private bool AutoLoadOnEndDrag = true;

        public void OnEndDrag(PointerEventData data)
        {
            if (!AutoLoadOnEndDrag)
                return;
            if (IsListFull() && !IsScrollBlocked())
            {
                CheckNextIteration();
            }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (!AutoLoadOnStartDrag)
                return;
            if (IsListFull() && !IsScrollBlocked())
            {
                CheckNextIteration();
            }
        }

        private void CheckNextIteration()
        {
            if (Scroll.verticalNormalizedPosition < 0.3f)
            {
                DataLoaderObject.SendMessage("AutoLoadContent", true);
            }
            else if (Scroll.verticalNormalizedPosition > 0.7f)
            {
                if (Scroll.verticalNormalizedPosition > LastScrollPosition)
                    DataLoaderObject.SendMessage("AutoLoadContent", false);
            }
            LastScrollPosition = Scroll.verticalNormalizedPosition;
        }

        public void UpdateScrollViewPosition(List<ScrollViewItem> _activeItems, bool _bottom)
        {
            print(222222);
            if (_bottom)
            {
                foreach (ScrollViewItem _item in _activeItems)
                {
                    _item.MoveToEnd();
                    print(_item.GetScrollViewHeight());
                    MoveScrollBy(_item.GetScrollViewHeight() + ContentVerticalGroup.spacing);
                }
            }
            else
            {
                foreach (ScrollViewItem _item in _activeItems)
                {
                    _item.MoveToPosition(TopIndex);
                    MoveScrollBy(-(_item.GetScrollViewHeight() + ContentVerticalGroup.spacing));
                }

            }
        }

        public List<ScrollViewItem> PushItem(int _num, bool _bottom)
        {
            List<ScrollViewItem> _activeItems = new List<ScrollViewItem>();
            if (_bottom)
            {
                int _movedCount = 0;
                loadIndexInList = 0;
                for (int i = 0; i < _num; i++)
                {
                    bool _full = IsListFull();
                    ScrollViewItem _current = ScrollItemList[i];
                    if (!_full)
                    {
                        _current = ScrollItemList[loadIndexInList];
                        loadIndexInList++;
                    }

                    if (!_full)
                    {
                        //_current.gameObject.SetActive(true);
                    }
                    else
                    {
                        print("ok");
                        _movedCount++;
                        _current.MoveToEnd();
                        //MoveScrollBy(_current.GetScrollViewHeight() + ContentVerticalGroup.spacing);
                        ScrollItemList.Add(_current);
                        LoadIndex++;
                    }
                    _activeItems.Add(_current);
                }
                for (int i = 0; i < _movedCount; i++)
                {
                    ScrollItemList.RemoveAt(0);
                }
                ScrollItemList.TrimExcess();
            }
            else
            {
                if (LoadIndex > 0 || Direction == ContentDirection.BottomToTop)
                {
                    for (int i = 0; i < _num; i++)
                    {
                        ScrollViewItem _current = ScrollItemList[ScrollItemList.Count - 1];
                        _activeItems.Add(_current);
                        LoadIndex--;
                        ScrollItemList.Insert(0, _current);
                        ScrollItemList.RemoveAt(ScrollItemList.Count - 1);
                    }
                }
                ScrollItemList.TrimExcess();
            }
            return _activeItems;
        }

        public bool IsListFull()
        {
            bool _full = true;
            foreach (ScrollViewItem _item in ScrollItemList)
            {
                if (!_item.gameObject.activeInHierarchy)
                {
                    _full = false;
                    break;
                }
            }
            return _full;
        }

        private void MoveScrollBy(float _value)
        {
            Vector2 _position = ContentRect.anchoredPosition;
            _position.y -= _value;
            ContentRect.anchoredPosition = _position;
        }

        public void ResetSroll()
        {
            LoadIndex = 0;
            loadIndexInList = 0;
            ContentRect.anchoredPosition = Vector2.zero;
        }

        public void HideAllScrollItems()
        {
            for (int i = 0; i < ScrollItemList.Count; i++)
            {
                ScrollItemList[i].gameObject.SetActive(false);
            }
        }

        public int GetContentListCount()
        {
            return ScrollItemList.Count;
        }

        public void BlockScroll()
        {
            IsBlocked = true;
        }

        public void UnblockScroll()
        {
            IsBlocked = false;
        }

        private bool IsScrollBlocked()
        {
            return IsBlocked;
        }

        public void ScrollToBottom()
        {
            StartCoroutine(OnScrollToBottom());
        }

        public void ForceScrollToBottom()
        {
            Scroll.verticalNormalizedPosition = 0;
        }

        private IEnumerator OnScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            /*print(Scroll.name);
            while (Scroll.verticalNormalizedPosition > 0)
            {
                yield return new WaitForFixedUpdate();
                Scroll.verticalNormalizedPosition -= ScrollSpeed * Time.deltaTime;
                print(Scroll.verticalNormalizedPosition);
            }*/
        }

        public GameObject GetDataLoaderObject()
        {
            return DataLoaderObject;
        }

        public enum ContentDirection
        {
            TopToBottom,
            BottomToTop
        }
    }
}
