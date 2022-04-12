using System.Collections.Generic;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public class WindowView : UIView, IUIViewGroup
    {
        public List<IUIView> Views
        {
            get
            {
                List<IUIView> views = new List<IUIView>();
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var view = transform.GetChild(i).GetComponent<IUIView>();
                    if (view != null)
                    {
                        views.Add(view);
                    }
                }

                return views;
            }
        }

        public IUIView GetView(string viewName)
        {
            return Views.Find(v => v.Name.Equals(viewName));
        }

        public void AddView(IUIView view, bool worldPositionStays = false)
        {
            if (view == null) return;
            var t = view.RectTransform;
            if (t == null || t.parent == transform) return;
            view.Owner.layer = gameObject.layer;
            t.SetParent(transform, worldPositionStays);
        }

        public virtual void AddView(IUIView view, UILayout layout)
        {
            if (view == null)
                return;

            Transform t = view.RectTransform;
            if (t == null)
                return;

            if (t.parent == transform)
            {
                if (layout != null)
                    layout(view.RectTransform);
                return;
            }

            view.Owner.layer = gameObject.layer;
            t.SetParent(transform, false);
            if (layout != null)
                layout(view.RectTransform);
        }

        public void RemoveView(IUIView view, bool worldPositionStays = false)
        {
            if (view == null) return;
            var t = view.RectTransform;
            if (t == null || t.parent != transform) return;
            t.SetParent(null, worldPositionStays);
        }
    }
}