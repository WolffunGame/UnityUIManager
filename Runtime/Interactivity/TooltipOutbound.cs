namespace UnityScreenNavigator.Runtime.Interactivity
{
    /// <summary>
    /// Check if the tooltip is out of its bounding rect
    /// </summary>
    public class TooltipOutbound
    {
        public bool IsAny
        {
            get { return BottomLeftCorner || TopLeftCorner || TopRightCorner || BottomRightCorner; }
        }

        public bool TopEdge
        {
            get { return TopLeftCorner && TopRightCorner; }
        }

        public bool RightEdge
        {
            get { return TopRightCorner && BottomRightCorner; }
        }

        public bool LeftEdge
        {
            get { return TopLeftCorner && BottomLeftCorner; }
        }

        public bool BottomEdge
        {
            get { return BottomLeftCorner && BottomRightCorner; }
        }

        public bool TopRightCorner { get; set; }
        public bool TopLeftCorner { get; set; }
        public bool BottomRightCorner { get; set; }
        public bool BottomLeftCorner { get; set; }

        /// <summary>
        /// Suggests a better tooltip position, based on where the tooltip is overflowing and the previously-desired position.
        /// </summary>
        /// <param name="fromPosition">The previously-desired tip position.</param>
        public TipPosition SuggestNewPosition(TipPosition fromPosition)
        {
            switch (fromPosition) // desired tip position
            {
                case TipPosition.TopRightCorner:
                    if (TopEdge && RightEdge) // flip to opposite corner.
                        return TipPosition.BottomLeftCorner;
                    if (TopEdge) // flip to bottom.
                        return TipPosition.BottomRightCorner;
                    if (RightEdge) // flip to left.
                        return TipPosition.TopLeftCorner;
                    break;
                case TipPosition.BottomRightCorner:
                    if (BottomEdge && RightEdge) // flip to opposite corner.
                        return TipPosition.TopLeftCorner;
                    if (BottomEdge) // flip to top.
                        return TipPosition.TopRightCorner;
                    if (RightEdge) // flip to left.
                        return TipPosition.BottomLeftCorner;
                    break;
                case TipPosition.TopLeftCorner:
                    if (TopEdge && LeftEdge) // flip to opposite corner.
                        return TipPosition.BottomRightCorner;
                    if (TopEdge) // flip to bottom.
                        return TipPosition.BottomLeftCorner;
                    if (LeftEdge) // flip to right.
                        return TipPosition.TopRightCorner;
                    break;
                case TipPosition.BottomLeftCorner:
                    if (BottomEdge && LeftEdge) // flip to opposite corner.
                        return TipPosition.TopRightCorner;
                    if (BottomEdge) // flip to top.
                        return TipPosition.TopLeftCorner;
                    if (LeftEdge) // flip to right.
                        return TipPosition.BottomRightCorner;
                    break;
                case TipPosition.TopMiddle:
                    if (TopEdge && RightEdge) // flip to opposite corner.
                        return TipPosition.BottomLeftCorner;
                    if (TopEdge && LeftEdge) // flip to opposite corner.
                        return TipPosition.BottomRightCorner;
                    if (TopEdge) // flip to bottom.
                        return TipPosition.BottomMiddle;
                    if (RightEdge) // flip to left.
                        return TipPosition.LeftMiddle;
                    if (LeftEdge) // flip to right.
                        return TipPosition.RightMiddle;
                    break;
                case TipPosition.BottomMiddle:
                    if (BottomEdge && RightEdge) // flip to opposite corner.
                        return TipPosition.TopLeftCorner;
                    if (BottomEdge && LeftEdge) // flip to opposite corner.
                        return TipPosition.TopRightCorner;
                    if (BottomEdge) // flip to top.
                        return TipPosition.TopMiddle;
                    if (RightEdge) // flip to left.
                        return TipPosition.LeftMiddle;
                    if (LeftEdge) // flip to right.
                        return TipPosition.RightMiddle;
                    break;
                case TipPosition.LeftMiddle:
                    if (TopEdge && LeftEdge) // flip to opposite corner.
                        return TipPosition.BottomRightCorner;
                    if (BottomEdge && LeftEdge) // flip to opposite corner.
                        return TipPosition.TopRightCorner;
                    if (TopEdge) // flip to bottom.
                        return TipPosition.BottomMiddle;
                    if (BottomEdge) // flip to top.
                        return TipPosition.TopMiddle;
                    if (LeftEdge) // flip to right.
                        return TipPosition.RightMiddle;
                    break;
                case TipPosition.RightMiddle:
                    if (TopEdge && RightEdge) // flip to opposite corner.
                        return TipPosition.BottomLeftCorner;
                    if (BottomEdge && RightEdge) // flip to opposite corner.
                        return TipPosition.TopLeftCorner;
                    if (TopEdge) // flip to bottom.
                        return TipPosition.BottomMiddle;
                    if (BottomEdge) // flip to top.
                        return TipPosition.TopMiddle;
                    if (RightEdge) // flip to left.
                        return TipPosition.LeftMiddle;
                    break;
            }

            return fromPosition;
        }
    }
}