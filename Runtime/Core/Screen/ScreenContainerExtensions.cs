using System;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace UnityScreenNavigator.Runtime.Core.Screen
{
    public static class ScreenContainerExtensions
    {
        /// <summary>
        ///     Add callbacks.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="onBeforePush"></param>
        /// <param name="onAfterPush"></param>
        /// <param name="onBeforePop"></param>
        /// <param name="onAfterPop"></param>
        public static void AddCallbackReceiver(this ScreenContainer self,
            Action<(Screen enterScreen, Screen exitScreen)> onBeforePush = null,
            Action<(Screen enterScreen, Screen exitScreen)> onAfterPush = null,
            Action<(Screen enterScreen, Screen exitScreen)> onBeforePop = null,
            Action<(Screen enterScreen, Screen exitScreen)> onAfterPop = null)
        {
            var callbackReceiver =
                new AnonymousScreenContainerCallbackReceiver(onBeforePush, onAfterPush, onBeforePop, onAfterPop);
            self.AddCallbackReceiver(callbackReceiver);
        }

        /// <summary>
        ///     Add callbacks.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="screen"></param>
        /// <param name="onBeforePush"></param>
        /// <param name="onAfterPush"></param>
        /// <param name="onBeforePop"></param>
        /// <param name="onAfterPop"></param>
        public static void AddCallbackReceiver(this ScreenContainer self, Screen screen,
            Action<Screen> onBeforePush = null, Action<Screen> onAfterPush = null,
            Action<Screen> onBeforePop = null, Action<Screen> onAfterPop = null)
        {
            var callbackReceiver = new AnonymousScreenContainerCallbackReceiver();
            callbackReceiver.OnBeforePush += x =>
            {
                var (enterScreen, exitScreen) = x;
                if (enterScreen.Equals(screen))
                {
                    onBeforePush?.Invoke(exitScreen);
                }
            };
            callbackReceiver.OnAfterPush += x =>
            {
                var (enterScreen, exitScreen) = x;
                if (enterScreen.Equals(screen))
                {
                    onAfterPush?.Invoke(exitScreen);
                }
            };
            callbackReceiver.OnBeforePop += x =>
            {
                var (enterScreen, exitScreen) = x;
                if (exitScreen.Equals(screen))
                {
                    onBeforePop?.Invoke(enterScreen);
                }
            };
            callbackReceiver.OnAfterPop += x =>
            {
                var (enterScreen, exitScreen) = x;
                if (exitScreen.Equals(screen))
                {
                    onAfterPop?.Invoke(enterScreen);
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