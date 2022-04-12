using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public interface IDynamicWindowLifeCycleEvent
    {
        /// <summary>
        /// Call this method after the window is loaded.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask Initialize();
#else
        IEnumerator Initialize();
#endif
        /// <summary>
        /// Called just before this window is displayed by the Show transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillShowEnter();
#else
        IEnumerator WillPushEnter();
#endif
        /// <summary>
        /// Called just after this window is displayed by the Show transition.
        /// </summary>
        void DidShowEnter();

        /// <summary>
        /// Called just before this window is hidden by the Show transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillShowExit();
#else
        IEnumerator WillShowExit();
#endif
        /// <summary>
        /// Called just after this window is hidden by the Show transition.
        /// </summary>
        void DidShowExit();

        /// <summary>
        ///         /// <summary>
        /// Called just before this window is displayed by the Hide transition.
        /// </summary>
        /// <returns></returns>
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillHideEnter();
#else
        IEnumerator WillHideEnter();
#endif
        /// <summary>
        /// Called just after this window is displayed by the Hide transition.
        /// </summary>
        void DidHideEnter();

        /// <summary>
        /// Called just before this window is hidden by the Hide transition.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask WillHideExit();
#else
        IEnumerator WillHideExit();
#endif

        /// <summary>
        /// Called just after this window is hidden by the Hide transition.
        /// </summary>
        void DidHideExit();

        /// <summary>
        /// Called just before this window is released.
        /// </summary>
        /// <returns></returns>
#if USN_USE_ASYNC_METHODS
        UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }
}