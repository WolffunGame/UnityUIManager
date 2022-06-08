using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public delegate void UILayout(RectTransform transform);
    /// <summary>
    /// Present a group of views, such as a <see cref="Window"/> have a <see cref="ToastView"/> to show messages, then these views in a <see cref="IUIViewGroup"/>.
    /// </summary>
    public interface IUIViewGroup : IUIView
    {
        List<IUIView> Views { get; }
        /// <summary>
        /// Get the view by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IUIView GetView(string name);
        /// <summary>
        /// Add a view to the view group.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="worldPositionStays"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddView(IUIView view, bool worldPositionStays = false);
        /// <summary>
        /// Add a view to the view group.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="layout"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddView(IUIView view, UILayout layout);
        /// <summary>
        /// Removes the view from the view group.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="worldPositionStays"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveView(IUIView view, bool worldPositionStays = false);
    }
}