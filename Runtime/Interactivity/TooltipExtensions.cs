using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public static class TooltipExtensions
    {
        /// <summary>
        /// Set position of tooltip.
        /// Only support Canvas render mode is ScreenSpaceOverlay.
        /// </summary>
        /// <param name="tooltip"></param>
        /// <param name="tipPosition"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="preventOutOfBounds"></param>
        public static void SetPosition(this IUIView tooltip, TipPosition tipPosition, RectTransform target, int offset,
            bool preventOutOfBounds = true)
        {
            var triggerCorners = new Vector3[4];
            var triggerRectTransform = target;
            triggerRectTransform.GetWorldCorners(triggerCorners);
            //Set initial position
            tooltip.SetPosition(tipPosition, triggerCorners, offset);
            if (!preventOutOfBounds)
                return;
            //check if tooltip is out of bounds
            Vector3[] tooltipCorners = new Vector3[4];
            tooltip.RectTransform.GetWorldCorners(tooltipCorners);

            for (int i = 0; i < tooltipCorners.Length; i++)
                tooltipCorners[i] = RectTransformUtility.WorldToScreenPoint(null, tooltipCorners[i]);

            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            var tooltipOutBound = new TooltipOutbound()
            {
                //is out of bounds on bottom left corner
                BottomLeftCorner = !screenRect.Contains(tooltipCorners[0]),
                //is out of bounds on top left corner
                TopLeftCorner = !screenRect.Contains(tooltipCorners[1]),
                //is out of bounds on top right corner
                TopRightCorner = !screenRect.Contains(tooltipCorners[2]),
                //is out of bounds on bottom right corner
                BottomRightCorner = !screenRect.Contains(tooltipCorners[3])
            };
            //if tooltip is out of bounds, reposition it
            if (tooltipOutBound.IsAny)
            {
                tooltip.SetPosition(tooltipOutBound.SuggestNewPosition(tipPosition), triggerCorners, offset);
            }
        }
        // Tooltip Trigger Corners:
        // 0 = bottom left
        // 1 = top left
        // 2 = top right
        // 3 = bottom right
        private static void SetPosition(this IUIView tooltip, TipPosition tipPosition, Vector3[] triggerCorners, int offset)
        {
            Vector3 pos = Vector3.zero;
            Vector2 offsetVector = Vector2.zero;
            var tooltipRectTrans = tooltip.RectTransform;
            switch (tipPosition)
            {
                case TipPosition.TopRightCorner:
                    offsetVector = new Vector2(-1 * offset, -1 * offset);
                    pos = triggerCorners[2];
                    tooltipRectTrans.pivot = new Vector2(0, 0);
                    break;
                case TipPosition.BottomRightCorner:
                    offsetVector = new Vector2(-1 * offset, offset);
                    pos = triggerCorners[3];
                    tooltipRectTrans.pivot = new Vector2(0, 1);
                    break;
                case TipPosition.TopLeftCorner:
                    offsetVector = new Vector2(offset, -1 * offset);
                    pos = triggerCorners[1];
                    tooltipRectTrans.pivot = new Vector2(1, 0);
                    break;
                case TipPosition.BottomLeftCorner:
                    offsetVector = new Vector2(offset, offset);
                    pos = triggerCorners[0];
                    tooltipRectTrans.pivot = new Vector2(1, 1);
                    break;
                case TipPosition.TopMiddle:
                    offsetVector = new Vector2(0, -1 * offset);
                    pos = triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 0);
                    break;
                case TipPosition.BottomMiddle:
                    offsetVector = new Vector2(0, offset);
                    pos = triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 1);
                    break;
                case TipPosition.LeftMiddle:
                    offsetVector = new Vector2(offset, 0);
                    pos = triggerCorners[0] + (triggerCorners[1] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(1, .5f);
                    break;
                case TipPosition.RightMiddle:
                    offsetVector = new Vector2(-1 * offset, 0);
                    pos = triggerCorners[3] + (triggerCorners[2] - triggerCorners[3]) / 2;
                    tooltipRectTrans.pivot = new Vector2(0, .5f);
                    break;
                case TipPosition.CanvasTopMiddle:
                    offsetVector = new Vector2(0, -1 * offset);
                    pos = triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 1);
                    tooltipRectTrans.anchorMin = tooltipRectTrans.anchorMax = new Vector2(.5f, 1);
                    break;
                case TipPosition.CanvasBottomMiddle:
                    offsetVector = new Vector2(0, offset);
                    pos = triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 0);
                    tooltipRectTrans.anchorMin = tooltipRectTrans.anchorMax = new Vector2(.5f, 0);
                    break;
            }

            tooltip.RectTransform.position = pos;
            tooltipRectTrans.anchoredPosition += offsetVector;
        }
    }
}