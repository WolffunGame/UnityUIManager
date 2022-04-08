using System.Collections.Generic;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    /// <summary>
    /// Manage Modals and Screens
    /// </summary>
    public interface IContainerManager : IContainerLayer
    {
        Window Current { get; }

        /// <summary>
        /// Pop a window from WindowManager.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        AsyncProcessHandle Pop(bool playAnimation);

        /// <summary>
        /// Push a window to WindowManager.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        AsyncProcessHandle Push(WindowOption option);
    }
}