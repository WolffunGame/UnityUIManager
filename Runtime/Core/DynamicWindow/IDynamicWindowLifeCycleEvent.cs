using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public interface IDynamicWindowLifeCycleEvent
    {
#if USN_USE_ASYNC_METHODS
        UniTask Initialize();
#else
        IEnumerator Initialize();
#endif

#if USN_USE_ASYNC_METHODS
        UniTask WillShowEnter();
#else
        IEnumerator WillPushEnter();
#endif

        void DidShowEnter();

#if USN_USE_ASYNC_METHODS
        UniTask WillShowExit();
#else
        IEnumerator WillShowExit();
#endif

        void DidShowExit();

#if USN_USE_ASYNC_METHODS
        UniTask WillHideEnter();
#else
        IEnumerator WillHideEnter();
#endif

        void DidHideEnter();

#if USN_USE_ASYNC_METHODS
        UniTask WillHideExit();
#else
        IEnumerator WillHideExit();
#endif

        void DidHideExit();

#if USN_USE_ASYNC_METHODS
        UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }
}