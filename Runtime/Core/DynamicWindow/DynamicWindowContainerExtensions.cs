using System;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public static class DynamicWindowContainerExtensions
    {
        public static void AddCallbackReceiver(this DynamicWindowContainer self,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onBeforeShow = null,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onAfterShow = null,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onBeforeHide = null,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onAfterHide = null)
        {
            var callbackReceiver =
                new AnonymousDynamicContainerCallbackReceiver(onBeforeShow, onAfterShow, onBeforeHide, onAfterHide);
            self.AddCallbackReceiver(callbackReceiver);
        }
        
        public static void AddCallbackReceiver(this DynamicWindowContainer self, DynamicWindow modal,
            Action<DynamicWindow> onBeforePush = null, Action<DynamicWindow> onAfterPush = null,
            Action<DynamicWindow> onBeforePop = null, Action<DynamicWindow> onAfterPop = null)
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