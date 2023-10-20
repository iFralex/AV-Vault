using UnityEngine;
using SocialApp;

namespace SocialApp
{
    public static class RendererExtensions
    {
        /// <summary>
        /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
        /// </summary>
        /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            Vector3 tempScreenSpaceCorner; // Cached
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }

        /// <summary>
        /// Determines if this RectTransform is fully visible from the specified camera.
        /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
        }

        /// <summary>
        /// Determines if this RectTransform is at least partially visible from the specified camera.
        /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
        }

        public static bool IsRectOnScreen(this RectTransform rect)
        {
            Vector3[] v = new Vector3[4];
            rect.GetWorldCorners(v);

            float maxY = Mathf.Max(v[0].y, v[1].y, v[2].y, v[3].y);
            float minY = Mathf.Min(v[0].y, v[1].y, v[2].y, v[3].y);
            //No need to check horizontal visibility: there is only a vertical scroll rect
            //float maxX = Mathf.Max (v [0].x, v [1].x, v [2].x, v [3].x);
            //float minX = Mathf.Min (v [0].x, v [1].x, v [2].x, v [3].x);

            if (maxY < 0 || minY > Screen.height)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsRectTransformsOverlap(this RectTransform elem, Camera cam, RectTransform viewport = null)
        {
            Vector2 viewportMinCorner;
            Vector2 viewportMaxCorner;

            if (viewport != null)
            {
                //so that we don't have to traverse the entire parent hierarchy (just to get screen coords relative to screen),
                //ask the camera to do it for us.
                //first get world corners of our rect:
                Vector3[] v_wcorners = new Vector3[4];
                viewport.GetWorldCorners(v_wcorners); //bot left, top left, top right, bot right

                //+ow shove it back into screen space. Now the rect is relative to the bottom left corner of screen:
                viewportMinCorner = cam.WorldToScreenPoint(v_wcorners[0]);
                viewportMaxCorner = cam.WorldToScreenPoint(v_wcorners[2]);
            }
            else
            {
                //just use the scren as the viewport
                viewportMinCorner = new Vector2(0, 0);
                viewportMaxCorner = new Vector2(Screen.width, Screen.height);
            }

            //give 1 pixel border to avoid numeric issues:
            viewportMinCorner += Vector2.one;
            viewportMaxCorner -= Vector2.one;

            //do a similar procedure, to get the "element's" corners relative to screen:
            Vector3[] e_wcorners = new Vector3[4];
            elem.GetWorldCorners(e_wcorners);

            Vector2 elem_minCorner = cam.WorldToScreenPoint(e_wcorners[0]);
            Vector2 elem_maxCorner = cam.WorldToScreenPoint(e_wcorners[2]);

            //perform comparison:
            if (elem_minCorner.x > viewportMaxCorner.x) { return false; }//completelly outside (to the right)
            if (elem_minCorner.y > viewportMaxCorner.y) { return false; }//completelly outside (is above)

            if (elem_maxCorner.x < viewportMinCorner.x) { return false; }//completelly outside (to the left)
            if (elem_maxCorner.y < viewportMinCorner.y) { return false; }//completelly outside (is below)

            /*
                 commented out, but use it if need to check if element is completely inside:
                Vector2 minDif = viewportMinCorner - elem_minCorner;
                Vector2 maxDif = viewportMaxCorner - elem_maxCorner;
                if(minDif.x < 0  &&  minDif.y < 0  &&  maxDif.x > 0  &&maxDif.y > 0) { //return "is completely inside" }
            */

            return true;//passed all checks, is inside (at least partially)
        }
    }
}