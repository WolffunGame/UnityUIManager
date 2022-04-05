using System.Collections;
#if USN_USE_ASYNC_METHODS
using Cysharp.Threading.Tasks;
#endif

namespace UnityScreenNavigator.Runtime.Core.Page
{
    public interface IPageLifecycleEvent
    {
#if USN_USE_ASYNC_METHODS
        UniTask Initialize();
#else
        IEnumerator Initialize();
#endif

#if USN_USE_ASYNC_METHODS
        UniTask WillPushEnter();
#else
        IEnumerator WillPushEnter();
#endif

        void DidPushEnter();

#if USN_USE_ASYNC_METHODS
        UniTask WillPushExit();
#else
        IEnumerator WillPushExit();
#endif

        void DidPushExit();

#if USN_USE_ASYNC_METHODS
        UniTask WillPopEnter();
#else
        IEnumerator WillPopEnter();
#endif

        void DidPopEnter();

#if USN_USE_ASYNC_METHODS
        UniTask WillPopExit();
#else
        IEnumerator WillPopExit();
#endif

        void DidPopExit();

#if USN_USE_ASYNC_METHODS
        UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }
}