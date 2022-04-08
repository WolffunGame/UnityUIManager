using System;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public static class DynamicWindowContainerExtensions
    {
        public static void AddCallbackReceiver(this DynamicWindowContainer self,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onBeforeShow = null,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onAfterShow = null,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onBeforeHide = null,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onAfterHide = null)
        {
            var callbackReceiver =
                new AnonymousDynamicContainerCallbackReceiver(onBeforeShow, onAfterShow, onBeforeHide, onAfterHide);
            self.AddCallbackReceiver(callbackReceiver);
        }
        
        public static void AddCallbackReceiver(this DynamicWindowContainer self, DynamicDynamicWindow modal,
            Action<DynamicDynamicWindow> onBeforePush = null, Action<DynamicDynamicWindow> onAfterPush = null,
            Action<DynamicDynamicWindow> onBeforePop = null, Action<DynamicDynamicWindow> onAfterPop = null)
        {
            var callbackReceiver = new AnonymousDynamicContainerCallbackReceiver();
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