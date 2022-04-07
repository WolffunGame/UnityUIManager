using System;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace UnityScreenNavigator.Runtime.Core.UnorderedModal
{
    public static class UnorderedModalContainerExtensions
    {
        public static void AddCallbackReceiver(this UnorderedModalContainer self,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onBeforeShow = null,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onAfterShow = null,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onBeforeHide = null,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onAfterHide = null)
        {
            var callbackReceiver =
                new AnonymousUnorderedModalContainerCallbackReceiver(onBeforeShow, onAfterShow, onBeforeHide, onAfterHide);
            self.AddCallbackReceiver(callbackReceiver);
        }
        
        public static void AddCallbackReceiver(this UnorderedModalContainer self, UnorderedModal modal,
            Action<UnorderedModal> onBeforePush = null, Action<UnorderedModal> onAfterPush = null,
            Action<UnorderedModal> onBeforePop = null, Action<UnorderedModal> onAfterPop = null)
        {
            var callbackReceiver = new AnonymousUnorderedModalContainerCallbackReceiver();
            callbackReceiver.OnBeforeShow += x =>
            {
                var (enterModal, exitModal) = x;
                if (enterModal.Equals(modal))
                {
                    onBeforePush?.Invoke(exitModal);
                }
            };
            callbackReceiver.OnAfterShow += x =>
            {
                var (enterModal, exitModal) = x;
                if (enterModal.Equals(modal))
                {
                    onAfterPush?.Invoke(exitModal);
                }
            };
            callbackReceiver.OnBeforeHide += x =>
            {
                var (enterModal, exitModal) = x;
                if (exitModal.Equals(modal))
                {
                    onBeforePop?.Invoke(enterModal);
                }
            };
            callbackReceiver.OnAfterHide += x =>
            {
                var (enterModal, exitModal) = x;
                if (exitModal.Equals(modal))
                {
                    onAfterPop?.Invoke(enterModal);
                }
            };

            var gameObj = self.gameObject;
            if (!gameObj.TryGetComponent<MonoBehaviourDestroyedEventDispatcher>(out var destroyedEventDispatcher))
            {
                destroyedEventDispatcher = gameObj.AddComponent<MonoBehaviourDestroyedEventDispatcher>();
            }

            destroyedEventDispatcher.OnDispatch += () => self.RemoveCallbackReceiver(callbackReceiver);

            self.AddCallbackReceiver(callbackReceiver);
        }
    }
}